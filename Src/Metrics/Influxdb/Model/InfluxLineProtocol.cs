using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Metrics.Influxdb.Model
{
	/// <summary>
	/// Defines static methods to convert influx tags, fields, records, and batches to a string in the line protocol format.
	/// </summary>
	public static class InfluxLineProtocol
	{

		#region Convert InfluxDB model objects to LineProtocol syntax

		/// <summary>
		/// Converts the <see cref="InfluxTag"/> to a string in the line protocol format.
		/// </summary>
		/// <param name="tag">The <see cref="InfluxTag"/> to generate the line protocol string for.</param>
		/// <returns>A string representing the tag in the line protocol format.</returns>
		public static String ToLineProtocol(this InfluxTag tag) {
			if (tag.IsEmpty) throw new ArgumentNullException(nameof(tag));
			String key = EscapeValue(tag.Key);
			String val = EscapeValue(tag.Value);
			return $"{key}={val}";
		}

		/// <summary>
		/// Converts the <see cref="InfluxField"/> to a string in the line protocol format.
		/// </summary>
		/// <param name="field">The <see cref="InfluxField"/> to generate the line protocol string for.</param>
		/// <returns>A string representing the field in the line protocol format.</returns>
		public static String ToLineProtocol(this InfluxField field) {
			if (field.IsEmpty) throw new ArgumentNullException(nameof(field));
			String key = EscapeValue(field.Key);
			String val = FormatValue(field.Value);
			return $"{key}={val}";
		}

		/// <summary>
		/// Converts the <see cref="InfluxRecord"/> to a string in the line protocol format.
		/// The returned string does not end in a newline character.
		/// </summary>
		/// <param name="record">The <see cref="InfluxRecord"/> to generate the line protocol string for.</param>
		/// <returns>A string representing the record in the line protocol format.</returns>
		/// <remarks>Creates output in line protocol syntax (tags and timestamp are optional):
		/// <c>measurement[,tag_key1=tag_value1...] field_key=field_value[,field_key2=field_value2] [timestamp]</c>
		/// According to the InfluxDB docs, sorted tags get a performance boost when inserting data, see:
		/// https://docs.influxdata.com/influxdb/v0.13/write_protocols/line/#key
		/// </remarks>
		public static String ToLineProtocol(this InfluxRecord record) {
			if (record == null)
				throw new ArgumentNullException(nameof(record));
			if (String.IsNullOrWhiteSpace(record.Name))
				throw new ArgumentNullException(nameof(record.Name), "The measurement name must be specified.");
			if (record.Fields.Count == 0)
				throw new ArgumentNullException(nameof(record.Fields), $"Must specify at least one field. Metric name: {record.Name}");

			StringBuilder sb = new StringBuilder();
			sb.Append(EscapeValue(record.Name));
			var sortedTags = record.Tags.OrderBy(t => t.Key); // sort for better insert performance, recommended by influxdb docs
			foreach (var tag in sortedTags) sb.AppendFormat(",{0}", tag.ToLineProtocol());
			sb.AppendFormat(" {0}", String.Join(",", record.Fields.Select(f => f.ToLineProtocol())));
			if (record.Timestamp.HasValue) sb.AppendFormat(" {0}", FormatTimestamp(record.Timestamp.Value, record.Precision));
			return sb.ToString();
		}

		/// <summary>
		/// Converts the <see cref="InfluxBatch"/> to a string in the line protocol syntax.
		/// Each record is separated by a newline character, but the complete output does not end in a newline.
		/// </summary>
		/// <param name="batch">The <see cref="InfluxBatch"/> to generate the line protocol string for.</param>
		/// <returns>A string representing all records in the batch formatted in the line protocol format.</returns>
		public static String ToLineProtocol(this InfluxBatch batch) {
			if (batch == null) throw new ArgumentNullException(nameof(batch));
			return String.Join("\n", batch.Select(r => r.ToLineProtocol()));
		}

		#endregion

		#region Format Field Values

		/// <summary>
		/// Formats the field value in the appropriate line protocol format based on the type of the value object.
		/// The value type must be a string, boolean, or integral or floating-point type.
		/// </summary>
		/// <param name="value">The field value to format.</param>
		/// <returns>The field value formatted as a string used in the line protocol format.</returns>
		public static String FormatValue(Object value) {
			Type type = value?.GetType();
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (!InfluxUtils.IsValidValueType(type))
				throw new ArgumentException(nameof(value), $"Value is not one of the supported types: {type} - Valid types: {String.Join(", ", InfluxUtils.ValidValueTypes.Select(t => t.Name))}");

			if (InfluxUtils.IsIntegralType(type))
				return FormatValue(Convert.ToInt64(value));
			if (InfluxUtils.IsFloatingPointType(type))
				return FormatValue(Convert.ToDouble(value));
			if (value is Boolean)
				return FormatValue((Boolean)value);
			if (value is String)
				return FormatValue((String)value);
			if (value is Char)
				return FormatValue(value.ToString());
			return FormatValue(value.ToString());
		}

		/// <summary>
		/// Formats the integral value to a string used in the line protocol format.
		/// </summary>
		/// <param name="value">The integral value to format.</param>
		/// <returns>The value formatted as a string used in the line protocol format.</returns>
		public static String FormatValue(Int64 value) {
			return String.Format(CultureInfo.InvariantCulture, "{0:d}i", value);
		}

		/// <summary>
		/// Formats the floating-point value to a string used in the line protocol format.
		/// </summary>
		/// <param name="value">The floating-point value to format.</param>
		/// <returns>The value formatted as a string used in the line protocol format.</returns>
		public static String FormatValue(Double value) {
			return String.Format(CultureInfo.InvariantCulture, "{0:r}", value); // use 'r' round-trip format specifier to avoid rounding/truncating in output
		}

		/// <summary>
		/// Formats the boolean value to a True or False string used in the line protocol format.
		/// </summary>
		/// <param name="value">The boolean value to format.</param>
		/// <returns>The value formatted as a string used in the line protocol format.</returns>
		public static String FormatValue(Boolean value) {
			// returns Boolean.TrueString or Boolean.FalseString (ie. "True" or "False")
			return value.ToString();
		}

		/// <summary>
		/// Formats the string by enclosing it in quotes and escaping any existing quotes within the string.
		/// This method should only be used when formatting string field values, but not tag values.
		/// </summary>
		/// <param name="value">The string field value to format.</param>
		/// <returns>The specified string field value properly escaped and enclosed in quotes.</returns>
		public static String FormatValue(String value) {
			if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			String escapeQuote = value.Replace("\"", @"\""");
			return String.Format("\"{0}\"", escapeQuote);
		}

		/// <summary>
		/// Escapes the value by replacing spaces, commas, and equal signs with a leading backslash.
		/// </summary>
		/// <param name="value">The value to escape.</param>
		/// <returns>The escaped value.</returns>
		public static String EscapeValue(String value) {
			if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
			return value.Replace(" ", @"\ ").Replace(",", @"\,").Replace("=", @"\=");
		}


		private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Formats the timestamp to an integer string with the specified precision.
		/// </summary>
		/// <param name="timestamp">The timestamp to format.</param>
		/// <param name="precision">The precision to format the timestamp to.</param>
		/// <returns>The timestamp formatted to a string with the specified precision.</returns>
		public static String FormatTimestamp(DateTime timestamp, InfluxPrecision precision) {
			if (timestamp < unixEpoch)
				throw new ArgumentOutOfRangeException(nameof(timestamp), "The timestamp cannot be earlier than the UNIX epoch (1970/1/1).");
			
			Int64 longTime = 0L;
			TimeSpan sinceEpoch = timestamp - unixEpoch;
			switch (precision) {
				case InfluxPrecision.Nanoseconds:  longTime = sinceEpoch.Ticks * 100L; break; // 100ns per tick
				case InfluxPrecision.Microseconds: longTime = sinceEpoch.Ticks / 10L;  break; // 10 ticks per us
				case InfluxPrecision.Milliseconds: longTime = sinceEpoch.Ticks / TimeSpan.TicksPerMillisecond; break;
				case InfluxPrecision.Seconds:      longTime = sinceEpoch.Ticks / TimeSpan.TicksPerSecond; break;
				case InfluxPrecision.Minutes:      longTime = sinceEpoch.Ticks / TimeSpan.TicksPerMinute; break;
				case InfluxPrecision.Hours:        longTime = sinceEpoch.Ticks / TimeSpan.TicksPerHour; break;
				default: throw new ArgumentException(nameof(precision), $"Invalid timestamp precision: {precision}");
			}

			return String.Format(CultureInfo.InvariantCulture, "{0:d}", longTime);
		}

		#endregion

	}
}
