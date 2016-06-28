using System;
using System.Collections.Generic;
using System.Linq;
using Metrics.MetricData;
using Metrics.Logging;
using Metrics.Reporters;
using Metrics.Influxdb.Model;
using Metrics.Influxdb.Adapters;

namespace Metrics.Influxdb
{
	/// <summary>
	/// A metrics report that sends data to an InfluxDB server.
	/// This sends the data using the InfluxDB LineProtocol syntax over HTTP.
	/// </summary>
	public abstract class InfluxdbBaseReport : BaseReport
	{

		private static readonly ILog log = LogProvider.GetCurrentClassLogger();

		protected readonly InfluxConfig config;
		protected readonly InfluxdbWriter writer;
		protected readonly InfluxdbConverter converter;
		protected readonly InfluxdbFormatter formatter;


		/// <summary>
		/// The <see cref="InfluxdbHttpReport"/> configuration settings.
		/// </summary>
		public InfluxConfig Config { get { return config; } }

		/// <summary>
		/// Gets the <see cref="InfluxdbConverter"/> used by this <see cref="InfluxdbBaseReport"/>
		/// instance to convert Metrics.NET metrics into <see cref="InfluxRecord"/>s.
		/// </summary>
		public InfluxdbConverter Converter { get { return converter; } }

		/// <summary>
		/// Gets the <see cref="InfluxdbFormatter"/> used by this <see cref="InfluxdbBaseReport"/>
		/// instance to format identifier names.
		/// </summary>
		public InfluxdbFormatter Formatter { get { return formatter; } }

		/// <summary>
		/// Gets the <see cref="InfluxdbWriter"/> used by this <see cref="InfluxdbBaseReport"/>
		/// instance to write <see cref="InfluxRecord"/>s to the InfluxDB server.
		/// </summary>
		public InfluxdbWriter Writer { get { return writer; } }


		/// <summary>
		/// Gets a new <see cref="InfluxdbBaseReport"/> instance with the default values.
		/// </summary>
		public static InfluxdbBaseReport Default { get { return new DefaultInfluxdbReport(); } }



		/// <summary>
		/// Creates a new InfluxDB report that uses the LineProtocol syntax over the protocol specified by the URI schema.
		/// </summary>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters.</param>
		public InfluxdbBaseReport(Uri influxDbUri)
			: this (new InfluxConfig(influxDbUri)) {
		}

		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax.
		/// </summary>
		/// <param name="config">The InfluxDB configuration object.</param>
		public InfluxdbBaseReport(InfluxConfig config = null) {
			this.config    = GetDefaultConfig(config) ?? new InfluxConfig();
			this.converter = config.Converter;
			this.formatter = config.Formatter;
			this.writer    = config.Writer;
			ValidateConfig(this.config);
		}


		/// <summary>
		/// Returns an instance with the default configuration for the derived type's implementation.
		/// If <paramref name="defaultConfig"/> is specified, the default converter, formatter, and
		/// writer are applied to it if any are not set; otherwise creates and returns a new instance.
		/// </summary>
		/// <param name="defaultConfig">The configuration instance to apply the default converter, formatter, and writer to.</param>
		/// <returns>A default <see cref="InfluxConfig"/> instance for the derived type's implementation.</returns>
		protected virtual InfluxConfig GetDefaultConfig(InfluxConfig defaultConfig) {
			return defaultConfig;
		}

		/// <summary>
		/// Validates the configuration to ensure the configuration is valid and throws an exception on invalid settings.
		/// </summary>
		/// <param name="config">The configuration to validate.</param>
		protected virtual void ValidateConfig(InfluxConfig config) {
			if (config.Converter == null)
				throw new ArgumentNullException(nameof(config.Converter), $"InfluxDB configuration invalid: {nameof(config.Converter)} cannot be null");
			if (config.Formatter == null)
				throw new ArgumentNullException(nameof(config.Formatter), $"InfluxDB configuration invalid: {nameof(config.Formatter)} cannot be null");
			if (config.Writer == null)
				throw new ArgumentNullException(nameof(config.Writer),    $"InfluxDB configuration invalid: {nameof(config.Writer)} cannot be null");
		}


		protected override void StartReport(String contextName) {
			converter.Timestamp = ReportTimestamp;
			base.StartReport(contextName);
		}

		protected override void StartContext(String contextName) {
			converter.Timestamp = CurrentContextTimestamp;
			base.StartContext(contextName);
		}

		protected override void EndReport(String contextName) {
			base.EndReport(contextName);
			writer.Flush();
		}



		protected override String FormatContextName(IEnumerable<String> contextStack, String contextName) {
			return formatter?.FormatContextName(contextStack, contextName) ?? base.FormatContextName(contextStack, contextName);
		}

		protected override String FormatMetricName<T>(String context, MetricValueSource<T> metric) {
			return formatter?.FormatMetricName(context, metric.Name, metric.Unit, metric.Tags) ?? base.FormatMetricName(context, metric);
		}



		protected override void ReportGauge(String name, Double value, Unit unit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(r => formatter?.FormatRecord(r) ?? r));
		}

		protected override void ReportCounter(String name, CounterValue value, Unit unit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(r => formatter?.FormatRecord(r) ?? r));
		}

		protected override void ReportMeter(String name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(r => formatter?.FormatRecord(r) ?? r));
		}

		protected override void ReportHistogram(String name, HistogramValue value, Unit unit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(r => formatter?.FormatRecord(r) ?? r));
		}

		protected override void ReportTimer(String name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(r => formatter?.FormatRecord(r) ?? r));
		}

		protected override void ReportHealth(HealthStatus status) {
			writer.Write(converter.GetRecords(status).Select(r => formatter?.FormatRecord(r) ?? r));
		}

	}

	/// <summary>
	/// The default implementation of the <see cref="InfluxdbBaseReport"/> using default settings.
	/// </summary>
	public class DefaultInfluxdbReport : InfluxdbBaseReport
	{
		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax over HTTP.
		/// </summary>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters.</param>
		public DefaultInfluxdbReport(Uri influxDbUri)
			: base(influxDbUri) {
		}

		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax over HTTP.
		/// </summary>
		/// <param name="config">The InfluxDB configuration object.</param>
		public DefaultInfluxdbReport(InfluxConfig config = null)
			: base(config) {
		}

		protected override InfluxConfig GetDefaultConfig(InfluxConfig defaultConfig) {
			var config = base.GetDefaultConfig(defaultConfig) ?? new InfluxConfig();
			config.Converter = config.Converter ?? new DefaultConverter();
			config.Formatter = config.Formatter ?? new DefaultFormatter();
			//config.Writer    = config.Writer    ?? new InfluxdbHttpWriter(config);
			return config;

		}
	}
}
