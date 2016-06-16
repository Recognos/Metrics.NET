using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Metrics.MetricData;
using Metrics.Logging;
using Metrics.Reporters;
using Metrics.Utils;
using System.Text;

namespace Metrics.Influxdb
{
	/// <summary>
	/// A metrics report that sends data to an InfluxDB server.
	/// This sends the data using the InfluxDB Line Protocol syntax over HTTP.
	/// </summary>
	public class InfluxdbHttpReport : BaseReport
	{

		private static readonly ILog log = LogProvider.GetCurrentClassLogger();

		private readonly InfluxdbConfig config;
		private readonly InfluxdbWriter writer;
		private readonly InfluxdbConverter converter;
		private readonly InfluxdbFormatter formatter;


		/// <summary>
		/// The <see cref="InfluxdbHttpReport"/> configuration settings.
		/// </summary>
		public InfluxdbConfig Config { get { return config; } }

		/// <summary>
		/// Gets the <see cref="InfluxdbConverter"/> used by this <see cref="InfluxdbHttpReport"/>
		/// instance to convert Metrics.NET metrics into <see cref="InfluxRecord"/>s.
		/// </summary>
		public InfluxdbConverter Converter { get { return converter; } }

		/// <summary>
		/// Gets the <see cref="InfluxdbFormatter"/> used by this <see cref="InfluxdbHttpReport"/>
		/// instance to format identifier names.
		/// </summary>
		public InfluxdbFormatter Formatter { get { return formatter; } }

		/// <summary>
		/// Gets the <see cref="InfluxdbWriter"/> used by this <see cref="InfluxdbHttpReport"/>
		/// instance to write <see cref="InfluxRecord"/>s to the InfluxDB server.
		/// </summary>
		public InfluxdbWriter Writer { get { return writer; } }



		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax over HTTP.
		/// </summary>
		/// <param name="influxDbUri">The URI of the InfluxDB server, including any query string parameters.</param>
		public InfluxdbHttpReport(Uri influxDbUri)
			: this (new InfluxdbConfig(influxDbUri)) {
		}

		/// <summary>
		/// Creates a new InfluxDB report that uses the Line Protocol syntax over HTTP.
		/// </summary>
		/// <param name="config">The InfluxDB configuration object.</param>
		public InfluxdbHttpReport(InfluxdbConfig config) {
			this.config    = config           ?? new InfluxdbConfig();
			this.writer    = config.Writer    ?? new InfluxdbHttpWriter(config.FormatInfluxUri());
			this.converter = config.Converter ?? new InfluxdbConverter(config.Precision);
			this.formatter = config.Formatter ?? new InfluxdbFormatter();
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
			//return formatter.ContextNameFormatter?.Invoke(contextStack, contextName) ?? base.FormatContextName(contextStack, contextName);
		}

		protected override String FormatMetricName<T>(String context, MetricValueSource<T> metric) {
			return formatter?.FormatMetricName(context, metric.Name, metric.Unit, metric.Tags) ?? base.FormatMetricName(context, metric);
			//return formatter.MetricNameFormatter?.Invoke(context, metric.Name, metric.Unit, metric.Tags) ?? base.FormatMetricName(context, metric);
		}



		protected override void ReportGauge(String name, Double value, Unit unit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(formatter.FormatRecord));
		}

		protected override void ReportCounter(String name, CounterValue value, Unit unit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(formatter.FormatRecord));
		}

		protected override void ReportMeter(String name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(formatter.FormatRecord));
		}

		protected override void ReportHistogram(String name, HistogramValue value, Unit unit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(formatter.FormatRecord));
		}

		protected override void ReportTimer(String name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) {
			writer.Write(converter.GetRecords(name, tags, unit, value).Select(formatter.FormatRecord));
		}

		protected override void ReportHealth(HealthStatus status) {
			writer.Write(converter.GetRecords(status).Select(formatter.FormatRecord));
		}



		#region Old Report Gauges

		//private static readonly String[] GaugeFields = new[] { "Value" };
		//private static readonly String[] CounterFields = new[] { "Count" };
		//private static readonly String[] CounterItemFields = new[] { "Count", "Percent" };
		//private static readonly String[] MeterFields = new[] { "Count", "Mean Rate", "1 Min Rate", "5 Min Rate", "15 Min Rate" };
		//private static readonly String[] MeterItemFields = new[] { "Count", "Percent", "Mean Rate", "1 Min Rate", "5 Min Rate", "15 Min Rate" };
		//private static readonly String[] HistogramFields = new[] {
		//	"Count", "Last", "Min", "Mean", "Max", "StdDev", "Median",
		//	"Percentile 75%", "Percentile 95%", "Percentile 98%", "Percentile 99%", "Percentile 99.9%" , "Sample Size",
		//	//"Total Count", "Last", "Last User Value", "Min", "Min User Value", "Mean", "Max", "Max User Value",
		//};
		//private static readonly String[] TimerFields = new[] {
		//	"Count", "Active Sessions", "Mean Rate", "Last", "Min", "Mean", "Max", "1 Min Rate", "5 Min Rate", "15 Min Rate",
		//	"StdDev", "Median", "Percentile 75%", "Percentile 95%", "Percentile 98%", "Percentile 99%", "Percentile 99.9%" , "Sample Size",
		//	//"Last", "Last User Value", "Min", "Min User Value", "Mean", "Max", "Max User Value",
		//};

		//protected void ReportGauge2(String name, Double value, Unit unit, MetricTags tags) {
		//	if (!Double.IsNaN(value) && !Double.IsInfinity(value)) {
		//		writer.Batch.Add(name, tags, GaugeFields, value);
		//	}
		//}

		//protected void ReportCounter2(String name, CounterValue value, Unit unit, MetricTags tags) {
		//	// add total
		//	writer.Batch.Add(name, tags, CounterFields, value.Count);

		//	// add set items
		//	foreach (var i in value.Items) {
		//		writer.Batch.Add(name, i.Item, tags, CounterItemFields, new Object[] {
		//			i.Count,
		//			i.Percent
		//		});
		//	}
		//}

		//protected  void ReportMeter2(String name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags) {
		//	// add total
		//	writer.Batch.Add(name, tags, MeterFields, new Object[] {
		//		value.Count,
		//		value.MeanRate,
		//		value.OneMinuteRate,
		//		value.FiveMinuteRate,
		//		value.FifteenMinuteRate
		//	});

		//	// add set items
		//	foreach (var i in value.Items) {
		//		writer.Batch.Add(name, i.Item, tags, MeterItemFields, new Object[] {
		//			i.Value.Count,
		//			i.Percent,
		//			i.Value.MeanRate,
		//			i.Value.OneMinuteRate,
		//			i.Value.FiveMinuteRate,
		//			i.Value.FifteenMinuteRate
		//		});
		//	}
		//}

		//protected void ReportHistogram2(String name, HistogramValue value, Unit unit, MetricTags tags) {
		//	writer.Batch.Add(name, tags, HistogramFields, new Object[] {
		//		value.Count,
		//		value.LastValue,
		//		value.Min,
		//		value.Mean,
		//		value.Max,
		//		value.StdDev,
		//		value.Median,
		//		value.Percentile75,
		//		value.Percentile95,
		//		value.Percentile98,
		//		value.Percentile99,
		//		value.Percentile999,
		//		value.SampleSize,
		//		//value.LastUserValue,
		//		//value.MinUserValue,
		//		//value.MaxUserValue,
		//	});
		//}

		//protected void ReportTimer2(String name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) {
		//	writer.Batch.Add(name, tags, TimerFields, new Object[] {
		//		value.Rate.Count,
		//		value.ActiveSessions,
		//		value.Rate.MeanRate,
		//		value.Rate.OneMinuteRate,
		//		value.Rate.FiveMinuteRate,
		//		value.Rate.FifteenMinuteRate,
		//		value.Histogram.LastValue,
		//		value.Histogram.Min,
		//		value.Histogram.Mean,
		//		value.Histogram.Max,
		//		value.Histogram.StdDev,
		//		value.Histogram.Median,
		//		value.Histogram.Percentile75,
		//		value.Histogram.Percentile95,
		//		value.Histogram.Percentile98,
		//		value.Histogram.Percentile99,
		//		value.Histogram.Percentile999,
		//		value.Histogram.SampleSize
		//		//value.Histogram.LastUserValue,
		//		//value.Histogram.MinUserValue,
		//		//value.Histogram.MaxUserValue,
		//	});
		//}

		#endregion
	}
}
