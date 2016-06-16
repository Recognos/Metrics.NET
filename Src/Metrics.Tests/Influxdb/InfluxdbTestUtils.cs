using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metrics.Influxdb;

namespace Metrics.Tests.Influxdb
{
	public class InfluxdbTestUtils
	{

	}

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


		protected override Byte[] WriteToTransport(Byte[] bytes) {
			var lastBatch = LastBatch = new InfluxBatch(Batch.ToArray());
			FlushHistory.Add(lastBatch);

			Debug.WriteLine($"InfluxDB LineProtocol Write (count={lastBatch.Count} bytes={fmtSize(bytes.Length)})");
			Stopwatch sw = Stopwatch.StartNew();
			Byte[] res = base.WriteToTransport(bytes);
			Debug.WriteLine($"Uploaded {lastBatch.Count} measurements to InfluxDB in {sw.ElapsedMilliseconds:n0}ms. :: Bytes written: {fmtSize(bytes.Length)} - Response string ({fmtSize(res.Length)}): {Encoding.UTF8.GetString(res)}\n");
			return res;
		}
	}
}
