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

		#region Default Reporter (wrapper for HTTP extensions)

		/// <summary>
		/// Schedules the default <see cref="InfluxdbBaseReport"/> to be executed at a fixed interval. Uses the HTTP protocol by default.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters. The expected format is: http[s]://{host}:{port}/write?db={database}&u={username}&p={password}&precision={n,u,ms,s,m,h}&rp={retentionPolicy}</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDb(this MetricsReports reports, Uri influxDbUri, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDb(new InfluxConfig(influxDbUri), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules the default <see cref="InfluxdbBaseReport"/> to be executed at a fixed interval. Uses the HTTP protocol by default.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="database">The database name to write values to.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDb(this MetricsReports reports, String host, UInt16? port, String database, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDb(new InfluxConfig(host, port, database), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules the default <see cref="InfluxdbBaseReport"/> to be executed at a fixed interval. Uses the HTTP protocol by default.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="database">The database name to write values to.</param>
		/// <param name="username">The username to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="password">The password to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="retentionPolicy">The retention policy to use when writing datapoints to the InfluxDB database, or null to use the database's default retention policy.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDb(this MetricsReports reports, String host, UInt16? port, String database, String username, String password, String retentionPolicy, InfluxPrecision? precision, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDb(new InfluxConfig(host, port, database, username, password, retentionPolicy, precision), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules the default <see cref="InfluxdbBaseReport"/> to be executed at a fixed interval. Uses the HTTP protocol by default.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDb(this MetricsReports reports, InfluxConfig config, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDbHttp(config, interval, filter, configFunc);
		}

		#endregion

		#region HTTP Reporter

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters. The expected format is: http[s]://{host}:{port}/write?db={database}&u={username}&p={password}&precision={n,u,ms,s,m,h}&rp={retentionPolicy}</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, Uri influxDbUri, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDbHttp(new InfluxConfig(influxDbUri), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="database">The database name to write values to.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, String host, String database, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDbHttp(new InfluxConfig(host, database), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="database">The database name to write values to.</param>
		/// <param name="retentionPolicy">The retention policy to use when writing datapoints to the InfluxDB database, or null to use the database's default retention policy.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, String host, String database, String retentionPolicy, InfluxPrecision? precision, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDbHttp(new InfluxConfig(host, database, retentionPolicy, precision), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="database">The database name to write values to.</param>
		/// <param name="username">The username to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="password">The password to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="retentionPolicy">The retention policy to use when writing datapoints to the InfluxDB database, or null to use the database's default retention policy.</param>
		/// <param name="precision">The precision of the timestamp value in the line protocol syntax.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, String host, UInt16? port, String database, String username, String password, String retentionPolicy, InfluxPrecision? precision, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDbHttp(new InfluxConfig(host, port, database, username, password, retentionPolicy, precision), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbHttpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbHttp(this MetricsReports reports, InfluxConfig config, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			InfluxConfig conf = config ?? new InfluxConfig();
			configFunc?.Invoke(conf);
			return reports.WithReport(new InfluxdbHttpReport(conf), interval, filter);
		}

		#endregion

		#region UDP Reporter

		/// <summary>
		/// Schedules an <see cref="InfluxdbUdpReport"/> to be executed at a fixed interval.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="influxDbUri">The URI of the InfluxDB server. The expected format is: net.udp://{host}:{port}/</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbUdp(this MetricsReports reports, Uri influxDbUri, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDbUdp(new InfluxConfig(influxDbUri), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbUdpReport"/> to be executed at a fixed interval.
		/// This reporter writes metric values to the InfluxDB database using the UDP transport.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, this is required for UDP connections.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbUdp(this MetricsReports reports, String host, UInt16 port, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDbUdp(new InfluxConfig(host, port, null), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbUdpReport"/> to be executed at a fixed interval.
		/// This reporter writes metric values to the InfluxDB database using the UDP transport.
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbUdp(this MetricsReports reports, InfluxConfig config, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			InfluxConfig conf = config ?? new InfluxConfig();
			configFunc?.Invoke(conf);
			return reports.WithReport(new InfluxdbUdpReport(conf), interval, filter);
		}

		#endregion

		#region JSON Reporter (pre InfluxDB v0.9.1 only)

		/// <summary>
		/// Schedules an <see cref="InfluxdbJsonReport"/> to be executed at a fixed interval using the deprecated JSON protocol.
		/// NOTE: It is recommended to NOT use the JSON reporter because of performance issues, and support for JSON has been
		/// removed from InfluxDB in versions later than v0.9.1. It is recommended to use the HTTP or UDP reporters instead.
		/// See for more information: https://docs.influxdata.com/influxdb/v0.13/write_protocols/json/
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="host">The hostname or IP address of the InfluxDB server.</param>
		/// <param name="port">The port number to connect to on the InfluxDB server, or null to use the default port number.</param>
		/// <param name="database">The database name to write values to.</param>
		/// <param name="username">The username to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="password">The password to use to connect to the InfluxDB server, or null if authentication is not used.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbJson(this MetricsReports reports, String host, UInt16? port, String database, String username, String password, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			return reports.WithInfluxDbJson(new InfluxConfig(host, port, database, username, password, null, InfluxPrecision.Seconds), interval, filter, configFunc);
		}

		/// <summary>
		/// Schedules an <see cref="InfluxdbJsonReport"/> to be executed at a fixed interval using the deprecated JSON protocol.
		/// NOTE: It is recommended to NOT use the JSON reporter because of performance issues, and support for JSON has been
		/// removed from InfluxDB in versions later than v0.9.1. It is recommended to use the HTTP or UDP reporters instead.
		/// See for more information: https://docs.influxdata.com/influxdb/v0.13/write_protocols/json/
		/// </summary>
		/// <param name="reports">The <see cref="MetricsReports"/> instance.</param>
		/// <param name="config">The InfluxDB reporter configuration.</param>
		/// <param name="interval">Interval at which to run the report.</param>
		/// <param name="filter">Only report metrics that match the filter.</param> 
		/// <param name="configFunc">A lambda expression that allows further configuration of the InfluxDB reporter using fluent syntax.</param>
		/// <returns>The <see cref="MetricsReports"/> instance.</returns>
		public static MetricsReports WithInfluxDbJson(this MetricsReports reports, InfluxConfig config, TimeSpan interval, MetricsFilter filter = null, Action<InfluxConfig> configFunc = null) {
			InfluxConfig conf = config ?? new InfluxConfig();
			configFunc?.Invoke(conf);
			return reports.WithReport(new InfluxdbJsonReport(conf), interval, filter);
		}

		#endregion

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

		/// <summary>
		/// Sets the <see cref="InfluxdbFormatter.LowercaseNames"/> property on this instance to the specified value and returns this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="formatter">The InfluxDB metric formatter.</param>
		/// <param name="lowercaseNames">If set to true will convert all context names, metric names, tag keys, and field keys to lowercase. If false, it does not modify the names. The default value is true.</param>
		/// <returns>This <see cref="InfluxdbFormatter"/> instance.</returns>
		public static InfluxdbFormatter WithLowercase(this InfluxdbFormatter formatter, Boolean lowercaseNames = true) {
			formatter.LowercaseNames = lowercaseNames;
			return formatter;
		}

		/// <summary>
		/// Sets the <see cref="InfluxdbFormatter.ReplaceSpaceChar"/> property on this instance to the specified value and returns this <see cref="InfluxdbFormatter"/> instance.
		/// </summary>
		/// <param name="formatter">The InfluxDB metric formatter.</param>
		/// <param name="replaceChars">The character(s) to replace all space characters with (underscore by default). If <see cref="String.Empty"/>, removes all spaces. If null, spaces are not replaced.</param>
		/// <returns>This <see cref="InfluxdbFormatter"/> instance.</returns>
		public static InfluxdbFormatter WithReplaceSpaces(this InfluxdbFormatter formatter, String replaceChars = "_") {
			formatter.ReplaceSpaceChar = replaceChars;
			return formatter;
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

		#region Deprecated Config Extensions

		public const String JsonObsoleteMsg = "The old InfluxDB format uses JSON over HTTP which was deprecated and removed in InfluxDB v0.9.1. See for more information: https://docs.influxdata.com/influxdb/v0.13/write_protocols/json/";

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
