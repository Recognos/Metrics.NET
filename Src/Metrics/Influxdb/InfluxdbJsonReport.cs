using System;
using Metrics.Influxdb.Adapters;
using Metrics.Influxdb.Model;

namespace Metrics.Influxdb
{
	/// <summary>
	/// A metrics report that sends data to an InfluxDB server. This sends the data using the InfluxDB JSON protocol over HTTP.
	/// NOTE: It is recommended to NOT use the JSON reporter because of performance issues, and support for JSON has been
	/// removed from InfluxDB in versions later than v0.9.1. It is recommended to use the HTTP or UDP reporters instead.
	/// See for more information: https://docs.influxdata.com/influxdb/v0.13/write_protocols/json/
	/// </summary>
	public class InfluxdbJsonReport : InfluxdbBaseReport
	{
		/// <summary>
		/// Creates a new InfluxDB report that uses the JSON protocol over HTTP.
		/// NOTE: It is recommended to NOT use the JSON reporter because of performance issues, and support for JSON has been
		/// removed from InfluxDB in versions later than v0.9.1. It is recommended to use the HTTP or UDP reporters instead.
		/// See for more information: https://docs.influxdata.com/influxdb/v0.13/write_protocols/json/
		/// </summary>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters.</param>
		public InfluxdbJsonReport(Uri influxDbUri)
			: base(influxDbUri) {
		}

		/// <summary>
		/// Creates a new InfluxDB report that uses the JSON protocol over HTTP.
		/// NOTE: It is recommended to NOT use the JSON reporter because of performance issues, and support for JSON has been
		/// removed from InfluxDB in versions later than v0.9.1. It is recommended to use the HTTP or UDP reporters instead.
		/// See for more information: https://docs.influxdata.com/influxdb/v0.13/write_protocols/json/
		/// </summary>
		/// <param name="config">The InfluxDB configuration object.</param>
		public InfluxdbJsonReport(InfluxConfig config = null)
			: base(config) {
		}

		protected override InfluxConfig GetDefaultConfig(InfluxConfig defaultConfig) {
			var config = base.GetDefaultConfig(defaultConfig) ?? new InfluxConfig();
			config.Formatter = (config.Formatter ?? new DefaultFormatter());
			config.Formatter.LowercaseNames = false;
			config.Formatter.ReplaceSpaceChar = null;
			config.Writer = config.Writer ?? new InfluxdbJsonWriter(config);
			return config;

		}
	}
}
