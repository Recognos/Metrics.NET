using System;
using Metrics.Influxdb.Model;
using Metrics.Influxdb.Adapters;

namespace Metrics.Influxdb
{
	/// <summary>
	/// A metrics report that sends data to an InfluxDB server.
	/// This sends the data using the InfluxDB LineProtocol syntax over UDP.
	/// </summary>
	public class InfluxdbUdpReport : DefaultInfluxdbReport
	{
		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax over UDP.
		/// </summary>
		/// <param name="influxDbUri">The UDP URI of the InfluxDB server.</param>
		public InfluxdbUdpReport(Uri influxDbUri)
			: base(influxDbUri) {
		}

		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax over UDP.
		/// </summary>
		/// <param name="config">The InfluxDB configuration object.</param>
		public InfluxdbUdpReport(InfluxConfig config = null)
			: base(config) {
		}

		protected override InfluxConfig GetDefaultConfig(InfluxConfig defaultConfig) {
			var config = base.GetDefaultConfig(defaultConfig) ?? new InfluxConfig();
			config.Writer = config.Writer ?? new InfluxdbUdpWriter(config);
			return config;

		}
	}
}
