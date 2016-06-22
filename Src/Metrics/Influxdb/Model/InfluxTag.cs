using System;

namespace Metrics.Influxdb.Model
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
}
