using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Influxdb.Model
{
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
}
