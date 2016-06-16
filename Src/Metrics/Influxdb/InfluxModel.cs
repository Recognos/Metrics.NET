using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Metrics.Utils;

namespace Metrics.Influxdb
{
	/// <summary>
	/// An InfluxDB tag key/value pair which can be added to an <see cref="InfluxRecord"/>.
	/// </summary>
	public struct InfluxTag : IEquatable<InfluxTag>
	{

		/// <summary>
		/// The tag key.
		/// </summary>
		public String Key { get; }

		/// <summary>
		/// The tag value.
		/// </summary>
		public String Value { get; }

		/// <summary>
		/// Returns true if this instance is equal to the Empty instance.
		/// </summary>
		public Boolean IsEmpty { get { return Empty.Equals(this); } }

		/// <summary>
		/// An empty <see cref="InfluxTag"/>.
		/// </summary>
		public static readonly InfluxTag Empty = new InfluxTag { };


		/// <summary>
		/// Creates a new <see cref="InfluxTag"/> from the specified key/value pair.
		/// Both the key and value are required and cannot be null or empty.
		/// </summary>
		/// <param name="key">The tag key name.</param>
		/// <param name="value">The tag value.</param>
		public InfluxTag(String key, String value) {
			if (String.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));
			if (String.IsNullOrWhiteSpace(value))
				throw new ArgumentNullException(nameof(value));

			this.Key = key;
			this.Value = value;
		}


		/// <summary>
		/// Converts the <see cref="InfluxTag"/> to a string in the line protocol format.
		/// </summary>
		/// <returns>A string representing the tag in the line protocol format.</returns>
		public override String ToString() {
			return this.ToLineProtocol();
		}


		#region Equality Methods

		/// <summary>
		/// Returns true if the specified object is an <see cref="InfluxTag"/> object and both the key and value are equal.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		/// <returns>true if the two objects are equal; false otherwise.</returns>
		public override Boolean Equals(Object obj) {
			return obj is InfluxTag && Equals((InfluxTag)obj);
		}

		/// <summary>
		/// Returns true if both the key and value are equal.
		/// </summary>
		/// <param name="other">The <see cref="InfluxTag"/> to compare.</param>
		/// <returns>true if the two objects are equal; false otherwise.</returns>
		public Boolean Equals(InfluxTag other) {
			return other.Key == this.Key && other.Value == this.Value;
		}

		/// <summary>
		/// Gets the hash code of the key.
		/// </summary>
		/// <returns>The hash code of the key.</returns>
		public override Int32 GetHashCode() {
			return Key.GetHashCode();
		}


		public static bool operator ==(InfluxTag t1, InfluxTag t2) {
			return t1.Equals(t2);
		}

		public static bool operator !=(InfluxTag t1, InfluxTag t2) {
			return !t1.Equals(t2);
		}

		#endregion

	}

	/// <summary>
	/// An InfluxDB field key/value pair which can be added to an <see cref="InfluxRecord"/>.
	/// </summary>
	public struct InfluxField : IEquatable<InfluxField>
	{

		/// <summary>
		/// The field key.
		/// </summary>
		public String Key { get; }

		/// <summary>
		/// The field value.
		/// </summary>
		public Object Value { get; }

		/// <summary>
		/// Returns true if this instance is equal to the Empty instance.
		/// </summary>
		public Boolean IsEmpty { get { return Empty.Equals(this); } }

		/// <summary>
		/// An empty <see cref="InfluxField"/>.
		/// </summary>
		public static readonly InfluxField Empty = new InfluxField { };


		/// <summary>
		/// Creates a new <see cref="InfluxField"/> with the specified key and value.
		/// </summary>
		/// <param name="key">The field key name.</param>
		/// <param name="value">The field value.</param>
		public InfluxField(String key, Object value) {
			if (String.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));
			if (value == null || (value is String && String.IsNullOrWhiteSpace((String)value)))
				throw new ArgumentNullException(nameof(value));

			this.Key = key;
			this.Value = value;
		}

		/// <summary>
		/// Converts the <see cref="InfluxField"/> to a string in the line protocol format.
		/// </summary>
		/// <returns>A string representing the field in the line protocol format.</returns>
		public override String ToString() {
			return this.ToLineProtocol();
		}


		#region Equality Methods

		/// <summary>
		/// Returns true if the specified object is an InfluxTag object and both the key and value are equal.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		/// <returns>true if the two objects are equal; false otherwise.</returns>
		public override Boolean Equals(Object obj) {
			return obj is InfluxField && Equals((InfluxField)obj);
		}

		/// <summary>
		/// Returns true if both the key and value are equal.
		/// </summary>
		/// <param name="other">The <see cref="InfluxField"/> to compare.</param>
		/// <returns>true if the two objects are equal; false otherwise.</returns>
		public Boolean Equals(InfluxField other) {
			return other.Key == this.Key && other.Value == this.Value;
		}

		/// <summary>
		/// Gets the hash code of the key.
		/// </summary>
		/// <returns>The hash code of the key.</returns>
		public override Int32 GetHashCode() {
			return Key.GetHashCode();
		}


		public static bool operator ==(InfluxField f1, InfluxField f2) {
			return f1.Equals(f2);
		}

		public static bool operator !=(InfluxField f1, InfluxField f2) {
			return !f1.Equals(f2);
		}

		#endregion

	}

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
			Tags   = tags?.ToList()   ?? new List<InfluxTag>();
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

	/// <summary>
	/// A collection of <see cref="InfluxRecord"/> elements that exposes helper methods to
	/// genereate a batch insert query string formatted in the InfluxDB line protocol syntax.
	/// </summary>
	public class InfluxBatch : List<InfluxRecord>
	{

		/// <summary>
		/// Creates a new <see cref="InfluxBatch"/> that is empty and has the default values.
		/// </summary>
		public InfluxBatch()
			: base() {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxBatch"/> that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of elements that the collection can initially store.</param>
		public InfluxBatch(Int32 capacity)
			: base(capacity) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxBatch"/> that contains elements copied from the specified collection.
		/// </summary>
		/// <param name="collection">The collection whose elements are copied to the new batch.</param>
		public InfluxBatch(IEnumerable<InfluxRecord> collection)
			: base(collection) {
		}


		/// <summary>
		/// Converts the <see cref="InfluxBatch"/> to a string in the line protocol syntax.
		/// Each record is separated by a newline character, but the complete output does not end in one.
		/// </summary>
		/// <returns>A string representing all records in the batch formatted in the line protocol format.</returns>
		public override String ToString() {
			return this.ToLineProtocol();
		}

	}


	/// <summary>
	/// The precision to format timestamp values in the InfluxDB line protocol syntax.
	/// </summary>
	public enum InfluxPrecision
	{
		Nanoseconds = 0,
		Microseconds,
		Milliseconds,
		Seconds,
		Minutes,
		Hours
	}
}
