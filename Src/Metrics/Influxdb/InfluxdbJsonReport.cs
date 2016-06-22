using System;
using Metrics.Influxdb.Adapters;
using Metrics.Influxdb.Model;

namespace Metrics.Influxdb
{
	/// <summary>
	/// A metrics report that sends data to an InfluxDB server.
	/// This sends the data using the InfluxDB JSON protocol over HTTP.
	/// </summary>
	[Obsolete(InfluxdbConfigExtensions.JsonObsoleteMsg)]
	public class InfluxdbJsonReport : InfluxdbBaseReport
	{
		/// <summary>
		/// Creates a new InfluxDB report that uses the JSON protocol over HTTP.
		/// </summary>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters.</param>
		public InfluxdbJsonReport(Uri influxDbUri)
			: base(influxDbUri) {
		}

		/// <summary>
		/// Creates a new InfluxDB report that uses the JSON protocol over HTTP.
		/// </summary>
		/// <param name="config">The InfluxDB configuration object.</param>
		public InfluxdbJsonReport(InfluxConfig config = null)
			: base(config) {
		}

		protected override InfluxConfig GetDefaultConfig(InfluxConfig defaultConfig) {
			var config = base.GetDefaultConfig(defaultConfig) ?? new InfluxConfig();
			config.Writer = config.Writer ?? new InfluxdbJsonWriter(config);
			return config;

		}
	}
}
