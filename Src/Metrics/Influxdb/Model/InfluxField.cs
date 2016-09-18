using System;

namespace Metrics.Influxdb.Model
{
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
}
