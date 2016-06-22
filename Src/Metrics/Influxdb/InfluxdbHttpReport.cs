using System;
using Metrics.Influxdb.Model;
using Metrics.Influxdb.Adapters;

namespace Metrics.Influxdb
{
	/// <summary>
	/// A metrics report that sends data to an InfluxDB server.
	/// This sends the data using the InfluxDB LineProtocol syntax over HTTP.
	/// </summary>
	public class InfluxdbHttpReport : DefaultInfluxdbReport
	{
		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax over HTTP.
		/// </summary>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters.</param>
		public InfluxdbHttpReport(Uri influxDbUri)
			: base(influxDbUri) {
		}

		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax over HTTP.
		/// </summary>
		/// <param name="config">The InfluxDB configuration object.</param>
		public InfluxdbHttpReport(InfluxConfig config = null)
			: base(config) {
		}

		protected override InfluxConfig GetDefaultConfig(InfluxConfig defaultConfig) {
			var config = base.GetDefaultConfig(defaultConfig) ?? new InfluxConfig();
			config.Writer = config.Writer ?? new InfluxdbHttpWriter(config);
			return config;

		}
	}
}
