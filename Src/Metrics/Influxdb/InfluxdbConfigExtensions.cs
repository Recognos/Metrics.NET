using System;
using Metrics.Influxdb;
using Metrics.Reports;
using Metrics.MetricData;
using Metrics.Influxdb.Model;
using Metrics.Influxdb.Adapters;

namespace Metrics
{
	/// <summary>
	/// Configuration extension methods for InfluxDB.
	/// </summary>
	public static class InfluxdbConfigExtensions
	{

		#region MetricsReports Extensions

		/// <summary>
		/// Schedules the default <see cref="InfluxdbBaseReport"/> to be executed at a fixed interval. Uses the HTTP protocol by default.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDb(this MetricsReports reports, InfluxConfig config, TimeSpan interval, MetricsFilter filter = null) {
			return reports.WithInfluxDbHttp(config, interval, filter);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="database">The database name to write values to. This should be null if using UDP since the database is defined in the UDP endpoint configuration on the InfluxDB server.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, String host, String database, TimeSpan interval, MetricsFilter filter = null) {
			return reports.WithInfluxDbHttp(new InfluxConfig(host, database), interval, filter);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="database">The database name to write values to. This should be null if using UDP since the database is defined in the UDP endpoint configuration on the InfluxDB server.</param>
		/// <param name="retentionPolicy">The retention policy to use when writing datapoints to the InfluxDB database, or null to use the database's default retention policy.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, String host, String database, String retentionPolicy, InfluxPrecision? precision, TimeSpan interval, MetricsFilter filter = null) {
			return reports.WithInfluxDbHttp(new InfluxConfig(host, database, retentionPolicy, precision), interval, filter);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="database">The database name to write values to. This should be null if using UDP since the database is defined in the UDP endpoint configuration on the InfluxDB server.</param>
		/// <param name="username">The username to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="password">The password to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="retentionPolicy">The retention policy to use when writing datapoints to the InfluxDB database, or null to use the database's default retention policy.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, String host, UInt16 port, String database, String username, String password, String retentionPolicy, InfluxPrecision? precision, TimeSpan interval, MetricsFilter filter = null) {
			return reports.WithInfluxDbHttp(new InfluxConfig(host, port, database, username, password, retentionPolicy, precision), interval, filter);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, Uri influxdbUri, TimeSpan interval, MetricsFilter filter = null) {
			return reports.WithInfluxDbHttp(new InfluxConfig(influxdbUri), interval, filter);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, InfluxConfig config, TimeSpan interval, MetricsFilter filter = null) {
			return reports.WithReport(new InfluxdbHttpReport(config), interval, filter);
		}

		#endregion

		#region InfluxConfig Configuration Extensions

		/// <summary>
		/// Sets the Writer on the InfluxDB reporter configuration and returns the same instance.
		/// </summary>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="writer">The InfluxDB metric writer.</param>
		/// <returns>This <see cref="InfluxConfig"/> instance.</returns>
		public static InfluxConfig WithWriter(this InfluxConfig config, InfluxdbWriter writer) {
			config.Writer = writer;
			return config;
		}

		/// <summary>
		/// Sets the Converter on the InfluxDB reporter configuration and returns the same instance.
		/// </summary>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="converter">The InfluxDB metric converter.</param>
		/// <returns>This <see cref="InfluxConfig"/> instance.</returns>
		public static InfluxConfig WithConverter(this InfluxConfig config, InfluxdbConverter converter) {
			config.Converter = converter;
			return config;
		}

		/// <summary>
		/// Sets the Formatter on the InfluxDB reporter configuration and returns the same instance.
		/// </summary>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="formatter">The InfluxDB metric formatter.</param>
		/// <returns>This <see cref="InfluxConfig"/> instance.</returns>
		public static InfluxConfig WithFormatter(this InfluxConfig config, InfluxdbFormatter formatter) {
			config.Formatter = formatter;
			return config;
		}

		#endregion

		#region InfluxdbWriter Configuration Extensions

		/// <summary>
		/// Sets the BatchSize on this instance to the specified value and returns this <see cref="InfluxdbWriter"/> instance.
		/// </summary>
		/// <param name="writer">The InfluxDB metric writer.</param>
		/// <param name="batchSize">The maximum number of records to write per flush. Set to zero to write all records in a single flush. Negative numbers are not allowed.</param>
		/// <returns>This <see cref="InfluxdbWriter"/> instance.</returns>
		public static InfluxdbWriter WithBatchSize(this InfluxdbWriter writer, Int32 batchSize) {
			writer.BatchSize = batchSize;
			return writer;
		}

		#endregion

		#region InfluxdbConverter Configuration Extensions

		/// <summary>
		/// Sets the Precision on this instance to the specified value and returns this <see cref="InfluxdbConverter"/> instance.
		/// </summary>
		/// <param name="converter">The InfluxDB metric converter.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		/// <returns>This <see cref="InfluxdbConverter"/> instance.</returns>
		public static InfluxdbConverter WithPrecision(this InfluxdbConverter converter, InfluxPrecision precision) {
			converter.Precision = precision;
			return converter;
		}

		/// <summary>
		/// Sets the GlobalTags on this instance to the specified value and returns this <see cref="InfluxdbConverter"/> instance.
		/// </summary>
		/// <param name="converter">The InfluxDB metric converter.</param>
		/// <param name="globalTags">The global tags that are added to all created <see cref="InfluxRecord"/> instances.</param>
		/// <returns>This <see cref="InfluxdbConverter"/> instance.</returns>
		public static InfluxdbConverter WithGlobalTags(this InfluxdbConverter converter, MetricTags globalTags) {
			converter.GlobalTags = globalTags;
			return converter;
		}

		#endregion

		#region InfluxdbFormatter Configuration Extensions

		/// <summary>
		/// Sets the ContextNameFormatter on this instance to the specified value and returns this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="formatter">The InfluxDB metric formatter.</param>
		/// <param name="contextFormatter">The context name formatter function.</param>
		/// <returns>This <see cref="InfluxdbFormatter"/> instance.</returns>
		public static InfluxdbFormatter WithContextFormatter(this InfluxdbFormatter formatter, InfluxdbFormatter.ContextFormatterDelegate contextFormatter) {
			formatter.ContextNameFormatter = contextFormatter;
			return formatter;
		}

		/// <summary>
		/// Sets the MetricNameFormatter on this instance to the specified value and returns this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="formatter">The InfluxDB metric formatter.</param>
		/// <param name="metricFormatter">The metric name formatter function.</param>
		/// <returns>This <see cref="InfluxdbFormatter"/> instance.</returns>
		public static InfluxdbFormatter WithMetricFormatter(this InfluxdbFormatter formatter, InfluxdbFormatter.MetricFormatterDelegate metricFormatter) {
			formatter.MetricNameFormatter = metricFormatter;
			return formatter;
		}

		/// <summary>
		/// Sets the TagKeyFormatter on this instance to the specified value and returns this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="formatter">The InfluxDB metric formatter.</param>
		/// <param name="tagFormatter">The tag key formatter function.</param>
		/// <returns>This <see cref="InfluxdbFormatter"/> instance.</returns>
		public static InfluxdbFormatter WithTagFormatter(this InfluxdbFormatter formatter, InfluxdbFormatter.TagKeyFormatterDelegate tagFormatter) {
			formatter.TagKeyFormatter = tagFormatter;
			return formatter;
		}

		/// <summary>
		/// Sets the FieldKeyFormatter on this instance to the specified value and returns this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="formatter">The InfluxDB metric formatter.</param>
		/// <param name="fieldFormatter">The field key formatter function.</param>
		/// <returns>This <see cref="InfluxdbFormatter"/> instance.</returns>
		public static InfluxdbFormatter WithFieldFormatter(this InfluxdbFormatter formatter, InfluxdbFormatter.FieldKeyFormatterDelegate fieldFormatter) {
			formatter.FieldKeyFormatter = fieldFormatter;
			return formatter;
		}

		#endregion


		#region Deprecated Config Extensions

		// old config extensions
		public const String JsonObsoleteMsg = "The old format uses JSON over HTTP which was deprecated in InfluxDB v0.9.1. See for more information: https://docs.influxdata.com/influxdb/v0.13/write_protocols/json/";

		[Obsolete(JsonObsoleteMsg)]
		public static MetricsReports WithInfluxDb(this MetricsReports reports, String host, UInt16 port, String user, String pass, String database, TimeSpan interval, MetricsFilter filter = null) {
			var config = new InfluxConfig(host, port, database, user, pass, null, InfluxPrecision.Seconds);
			return reports.WithReport(new InfluxdbJsonReport(config), interval, filter);
			//return reports.WithInfluxDb(new Uri($@"http://{host}:{port}/db/{database}/series?u={user}&p={pass}&time_precision=s"), interval);
		}

		[Obsolete(JsonObsoleteMsg)]
		public static MetricsReports WithInfluxDb(this MetricsReports reports, Uri influxdbUri, TimeSpan interval) {
			return reports.WithReport(new InfluxdbJsonReport(influxdbUri), interval);
		}

		#endregion

	}
}
