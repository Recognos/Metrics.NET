using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Influxdb.Model
{
	/// <summary>
	/// A single InfluxDB record that defines the name, tags, fields, and timestamp values to insert into InfluxDB.
	/// </summary>
	public class InfluxRecord
	{
		/// <summary>
		/// The measurement or series name. This value is required.
		/// </summary>
		public String Name { get; set; }

		/// <summary>
		/// A list of tag key/value pairs associated with this record. This value is optional.
		/// </summary>
		public List<InfluxTag> Tags { get; }

		/// <summary>
		/// A list of field key/value pairs associated with this record.
		/// This field is required, at least one field must be specified.
		/// </summary>
		public List<InfluxField> Fields { get; }

		/// <summary>
		/// The record timestamp. This value is optional. If this is null the timestamp is not included
		/// in the line value and the current timestamp will be used by default by the InfluxDB database.
		/// </summary>
		public DateTime? Timestamp { get; set; }

		/// <summary>
		/// The timestamp precision to use when formatting the timestamp in the line protocol format.
		/// It is recommended to use as large a precision as possible to improve compression and bandwidth usage.
		/// </summary>
		public InfluxPrecision Precision { get; set; }


		/// <summary>
		/// Creates a new <see cref="InfluxRecord"/>.
		/// </summary>
		/// <param name="name">The measurement or series name. This value is required and cannot be null or empty.</param>
		/// <param name="fields">The field values for this record.</param>
		/// <param name="timestamp">The optional timestamp for this record.</param>
		/// <param name="precision">The formatted timestamp precision. If null, uses <see cref="InfluxPrecision.Seconds"/>.</param>
		public InfluxRecord(String name, IEnumerable<InfluxField> fields, DateTime? timestamp = null, InfluxPrecision? precision = null)
			: this(name, null, fields, timestamp, precision) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxRecord"/>.
		/// </summary>
		/// <param name="name">The measurement or series name. This value is required and cannot be null or empty.</param>
		/// <param name="tags">The optional tags to associate with this record.</param>
		/// <param name="fields">The field values for this record.</param>
		/// <param name="timestamp">The optional timestamp for this record.</param>
		/// <param name="precision">The formatted timestamp precision. If null, uses <see cref="InfluxPrecision.Seconds"/>.</param>
		public InfluxRecord(String name, IEnumerable<InfluxTag> tags, IEnumerable<InfluxField> fields, DateTime? timestamp = null, InfluxPrecision? precision = null) {
			Name = name ?? String.Empty;
			Timestamp = timestamp;
			Precision = precision ?? InfluxPrecision.Seconds;
			Tags = tags?.ToList() ?? new List<InfluxTag>();
			Fields = fields?.ToList() ?? new List<InfluxField>();
		}


		/// <summary>
		/// Converts the <see cref="InfluxRecord"/> to a string in the line protocol syntax.
		/// The returned string does not end in a newline character.
		/// </summary>
		/// <returns>A string representing the record in the line protocol format.</returns>
		public override String ToString() {
			return this.ToLineProtocol();
		}
	}
}
