using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metrics.Influxdb;
using Metrics.Influxdb.Adapters;
using Metrics.Influxdb.Model;

namespace Metrics.Tests.Influxdb
{
	/// <summary>
	/// Defines a test case for an <see cref="InfluxTag"/>.
	/// </summary>
	public class TagTestCase
	{
		public InfluxTag Tag { get; set; }
		public String Output { get; set; }

		public TagTestCase(String key, String value, String output)
			: this(new InfluxTag(key, value), output) {
		}

		public TagTestCase(InfluxTag tag, String output) {
			Tag = tag;
			Output = output;
		}

		public Object[] ToArray() { return this; }

		public static implicit operator Object[] (TagTestCase item) {
			return new Object[] { item.Tag, item.Output };
		}
	}

	/// <summary>
	/// Defines a test case for an <see cref="InfluxField"/>.
	/// </summary>
	public class FieldTestCase
	{
		public InfluxField Field { get; set; }
		public String Output { get; set; }

		public FieldTestCase(String key, Object value, String output)
			: this(new InfluxField(key, value), output) {
		}

		public FieldTestCase(InfluxField field, String output) {
			Field = field;
			Output = output;
		}

		public Object[] ToArray() { return this; }

		public static implicit operator Object[] (FieldTestCase item) {
			return new Object[] { item.Field, item.Output };
		}
	}


	/// <summary>
	/// An <see cref="InfluxdbWriter"/> implementation used for unit testing. This writer keeps a list of all batches flushed to the writer.
	/// </summary>
	public class InfluxdbTestWriter : InfluxdbLineWriter
	{
		/// <summary>
		/// The list of all batches flushed by the writer.
		/// </summary>
		public List<InfluxBatch> FlushHistory { get; } = new List<InfluxBatch>();

		/// <summary>
		/// A copy of the last batch that was flushed by the writer.
		/// </summary>
		public InfluxBatch LastBatch { get; private set; } = new InfluxBatch();

		/// <summary>
		/// Creates a new <see cref="InfluxdbTestWriter"/> with the specified configuration and batch size.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbTestWriter(InfluxConfig config, Int32 batchSize = 0)
			: base(config, batchSize) {
		}


		protected override Byte[] WriteToTransport(Byte[] bytes) {
			var lastBatch = LastBatch = new InfluxBatch(Batch.ToArray());
			FlushHistory.Add(lastBatch);
			return null;
		}
	}

	/// <summary>
	/// An <see cref="InfluxdbWriter"/> implementation used for unit testing. This writer keeps a list of all batches flushed to the writer.
	/// </summary>
	public class InfluxdbHttpWriterExt : InfluxdbHttpWriter
	{
		/// <summary>
		/// The list of all batches flushed by the writer.
		/// </summary>
		public List<InfluxBatch> FlushHistory { get; } = new List<InfluxBatch>();

		/// <summary>
		/// A copy of the last batch that was flushed by the writer.
		/// </summary>
		public InfluxBatch LastBatch { get; private set; } = new InfluxBatch();


		/// <summary>
		/// Creates a new <see cref="InfluxdbHttpWriterExt"/> with the specified URI.
		/// </summary>
		/// <param name="influxDbUri">The HTTP URI of the InfluxDB server.</param>
		public InfluxdbHttpWriterExt(Uri influxDbUri)
			: base(influxDbUri) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbHttpWriterExt"/> with the specified URI.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbHttpWriterExt(InfluxConfig config, Int32 batchSize = 0) 
			: base(config, batchSize) {
		}


		protected override Byte[] WriteToTransport(Byte[] bytes) {
			var lastBatch = LastBatch = new InfluxBatch(Batch.ToArray());
			FlushHistory.Add(lastBatch);

			Debug.WriteLine($"[HTTP] InfluxDB LineProtocol Write (count={lastBatch.Count} bytes={fmtSize(bytes.Length)})");
			Stopwatch sw = Stopwatch.StartNew();
			Byte[] res = base.WriteToTransport(bytes);
			Debug.WriteLine($"[HTTP] Uploaded {lastBatch.Count} measurements to InfluxDB in {sw.ElapsedMilliseconds:n0}ms. :: Bytes written: {fmtSize(bytes.Length)} - Response string ({fmtSize(res.Length)}): {Encoding.UTF8.GetString(res)}");
			return res;
		}
	}

	/// <summary>
	/// An <see cref="InfluxdbWriter"/> implementation used for unit testing. This writer keeps a list of all batches flushed to the writer.
	/// </summary>
	public class InfluxdbUdpWriterExt : InfluxdbUdpWriter
	{
		/// <summary>
		/// The list of all batches flushed by the writer.
		/// </summary>
		public List<InfluxBatch> FlushHistory { get; } = new List<InfluxBatch>();

		/// <summary>
		/// A copy of the last batch that was flushed by the writer.
		/// </summary>
		public InfluxBatch LastBatch { get; private set; } = new InfluxBatch();


		/// <summary>
		/// Creates a new <see cref="InfluxdbUdpWriterExt"/> with the specified URI.
		/// </summary>
		/// <param name="influxDbUri">The UDP URI of the InfluxDB server.</param>
		public InfluxdbUdpWriterExt(Uri influxDbUri)
			: base(influxDbUri) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbUdpWriterExt"/> with the specified URI.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbUdpWriterExt(InfluxConfig config, Int32 batchSize = 0) 
			: base(config, batchSize) {
		}


		protected override Byte[] WriteToTransport(Byte[] bytes) {
			var lastBatch = LastBatch = new InfluxBatch(Batch.ToArray());
			FlushHistory.Add(lastBatch);

			Debug.WriteLine($"[UDP] InfluxDB LineProtocol Write (count={lastBatch.Count} bytes={fmtSize(bytes.Length)})");
			Stopwatch sw = Stopwatch.StartNew();
			Byte[] res = base.WriteToTransport(bytes);
			Debug.WriteLine($"[UDP] Uploaded {lastBatch.Count} measurements to InfluxDB in {sw.ElapsedMilliseconds:n0}ms. :: Bytes written: {fmtSize(bytes.Length)} - Response string ({fmtSize(res.Length)}): {Encoding.UTF8.GetString(res)}");
			return res;
		}
	}

	/// <summary>
	/// An <see cref="InfluxdbWriter"/> implementation used for unit testing. This writer keeps a list of all batches flushed to the writer.
	/// </summary>
	public class InfluxdbJsonWriterExt : InfluxdbJsonWriter
	{
		/// <summary>
		/// The list of all batches flushed by the writer.
		/// </summary>
		public List<InfluxBatch> FlushHistory { get; } = new List<InfluxBatch>();

		/// <summary>
		/// A copy of the last batch that was flushed by the writer.
		/// </summary>
		public InfluxBatch LastBatch { get; private set; } = new InfluxBatch();


		/// <summary>
		/// Creates a new <see cref="InfluxdbJsonWriterExt"/> with the specified URI.
		/// </summary>
		/// <param name="influxDbUri">The JSON URI of the InfluxDB server.</param>
		public InfluxdbJsonWriterExt(Uri influxDbUri)
			: base(influxDbUri) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbJsonWriterExt"/> with the specified URI.
		/// </summary>
		/// <param name="config">The InfluxDB configuration.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		public InfluxdbJsonWriterExt(InfluxConfig config, Int32 batchSize = 0)
			: base(config, batchSize) {
		}


		protected override Byte[] WriteToTransport(Byte[] bytes) {
			var lastBatch = LastBatch = new InfluxBatch(Batch.ToArray());
			FlushHistory.Add(lastBatch);

			Debug.WriteLine($"[JSON] InfluxDB LineProtocol Write (count={lastBatch.Count} bytes={fmtSize(bytes.Length)})");
			Stopwatch sw = Stopwatch.StartNew();
			Byte[] res = base.WriteToTransport(bytes);
			Debug.WriteLine($"[JSON] Uploaded {lastBatch.Count} measurements to InfluxDB in {sw.ElapsedMilliseconds:n0}ms. :: Bytes written: {fmtSize(bytes.Length)} - Response string ({fmtSize(res.Length)}): {Encoding.UTF8.GetString(res)}");
			return res;
		}
	}
}
