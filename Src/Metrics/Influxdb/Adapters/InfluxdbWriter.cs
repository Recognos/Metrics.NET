using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Metrics.Logging;
using Metrics.Influxdb.Model;

namespace Metrics.Influxdb.Adapters
{
	/// <summary>
	/// The <see cref="InfluxdbWriter"/> is responsible for writing <see cref="InfluxRecord"/>s to the InfluxDB server.
	/// Derived classes can implement various methods and protocols for writing the data (ie. HTTP API, UDP, etc).
	/// </summary>
	public abstract class InfluxdbWriter : IDisposable {
		// formats bytes into a string with units either in bytes or KiB
		protected static readonly Func<Int64, String> fmtSize = bytes => bytes < (1 << 12) ? $"{bytes:n0} bytes" : $"{bytes / 1024.0:n2} KiB";

		protected readonly InfluxBatch batch;
		protected Int32 batchSize;


		#region Public Data Members

		/// <summary>
		/// The currently buffered <see cref="InfluxBatch"/> that has not yet been flushed to the underlying writer.
		/// </summary>
		public InfluxBatch Batch {
			get { return batch; }
		}

		/// <summary>
		/// The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.
		/// </summary>
		public Int32 BatchSize {
			get { return batchSize; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException(nameof(value), "Batch size cannot be negative.");
				batchSize = value;
			}
		}

		#endregion


		/// <summary>
		/// Creates a new instance of a <see cref="InfluxdbWriter"/>.
		/// </summary>
		public InfluxdbWriter()
			: this(0) {
		}

		/// <summary>
		/// Creates a new instance of a <see cref="InfluxdbWriter"/>.
		/// </summary>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbWriter(Int32 batchSize) {
			if (batchSize < 0)
				throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size cannot be negative.");

			this.batch = new InfluxBatch();
			this.batchSize = batchSize;
		}


		#region Abstract Methods

		/// <summary>
		/// Gets the byte representation of the <see cref="InfluxBatch"/>. This will convert the
		/// batch to the correct string syntax and then get the byte array for it in UTF8 encoding.
		/// </summary>
		/// <param name="batch">The batch to get the bytes for.</param>
		/// <returns>The byte representation of the batch.</returns>
		protected abstract Byte[] GetBatchBytes(InfluxBatch batch);

		/// <summary>
		/// Writes the byte array to the InfluxDB server using the underlying transport.
		/// </summary>
		/// <param name="bytes">The bytes to write to the InfluxDB server.</param>
		/// <returns>The response from the server after writing the message, or null if there is no response (like for UDP).</returns>
		protected abstract Byte[] WriteToTransport(Byte[] bytes);

		#endregion

		#region Public Methods

		/// <summary>
		/// Flushes all buffered records in the batch by writing them to the server in a single write operation.
		/// </summary>
		public virtual void Flush() {
			if (Batch.Count == 0) return;
			Byte[] bytes = new Byte[0];

			try {
				bytes = GetBatchBytes(Batch);
				WriteToTransport(bytes);
			} catch (Exception ex) {
				String firstNLines = "\n" + String.Join("\n", Encoding.UTF8.GetString(bytes).Split('\n').Take(5)) + "\n";
				MetricsErrorHandler.Handle(ex, $"Error while flushing {Batch.Count} measurements to InfluxDB. Bytes: {fmtSize(bytes.Length)} - First 5 lines: {firstNLines}");
			} finally {
				// clear always, regardless if it was successful or not
				Batch.Clear();
			}
		}

		/// <summary>
		/// Writes the record to the InfluxDB server. If batching is used, the record will be added to the
		/// batch buffer but will not immediately be written to the server. If the number of buffered records
		/// is greater than or equal to the BatchSize, then the batch will be flushed to the underlying writer.
		/// </summary>
		/// <param name="record">The record to write.</param>
		public virtual void Write(InfluxRecord record) {
			if (record == null) throw new ArgumentNullException(nameof(record));
			batch.Add(record);
			if (batchSize > 0 && batch.Count >= batchSize)
				Flush(); // flush if batch is full
		}

		/// <summary>
		/// Writes the records to the InfluxDB server. Flushing will occur in increments of the defined BatchSize.
		/// </summary>
		/// <param name="records">The records to write.</param>
		public virtual void Write(IEnumerable<InfluxRecord> records) {
			if (records == null) throw new ArgumentNullException(nameof(records));
			foreach (var r in records)
				Write(r);
		}

		/// <summary>
		/// Flushes all buffered records and clears the batch.
		/// </summary>
		public virtual void Dispose() {
			try {
				Flush();
			} finally {
				batch.Clear();
			}
		}

		#endregion

	}

	/// <summary>
	/// This class writes <see cref="InfluxRecord"/>s formatted in the LineProtocol to the InfluxDB server.
	/// </summary>
	public abstract class InfluxdbLineWriter : InfluxdbWriter
	{
		protected readonly InfluxConfig config;

		/// <summary>
		/// Creates a new <see cref="InfluxdbLineWriter"/> with the specified configuration and batch size.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbLineWriter(InfluxConfig config, Int32 batchSize = 0)
			: base(batchSize) {
			this.config = config;
			if (config == null)
				throw new ArgumentNullException(nameof(config));
		}

		/// <summary>
		/// Gets the byte representation of the <see cref="InfluxBatch"/> in LineProtocol syntax using UTF8 encoding.
		/// </summary>
		/// <param name="batch">The batch to get the bytes for.</param>
		/// <returns>The byte representation of the batch.</returns>
		protected override Byte[] GetBatchBytes(InfluxBatch batch) {
			var strBatch = batch.ToLineProtocol(config.Precision);
			var bytes = Encoding.UTF8.GetBytes(strBatch);
			return bytes;
		}
	}
}
