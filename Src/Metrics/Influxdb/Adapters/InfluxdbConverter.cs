using System;
using System.Collections.Generic;
using System.Linq;
using Metrics.MetricData;
using Metrics.Influxdb.Model;
using System.Text.RegularExpressions;

namespace Metrics.Influxdb.Adapters
{
	/// <summary>
	/// This class converts Metrics.NET metric values into <see cref="InfluxRecord"/> objects.
	/// </summary>
	public abstract class InfluxdbConverter
	{

		/// <summary>
		/// Gets or sets the current timestamp. This value is used when creating new <see cref="InfluxRecord"/> instances.
		/// </summary>
		public DateTime? Timestamp { get; set; }

		/// <summary>
		/// Gets or sets the global tags. Global tags are added to all created <see cref="InfluxRecord"/> instances.
		/// </summary>
		public MetricTags GlobalTags { get; set; }


		/// <summary>
		/// Creates a new <see cref="InfluxdbConverter"/> using the default precision defined by <see cref="InfluxConfig.Default.Precision"/>.
		/// </summary>
		public InfluxdbConverter()
			: this(null) {
		}

		/// <summary>
		/// Creates a new <see cref="InfluxdbConverter"/> using the specified precision and tags.
		/// </summary>
		/// <param name="globalTags">The global tags that are added to all created <see cref="InfluxRecord"/> instances.</param>
		public InfluxdbConverter(MetricTags? globalTags = null) {
			GlobalTags = globalTags ?? MetricTags.None;
		}



		/// <summary>
		/// Creates a new <see cref="InfluxRecord"/> instance for the gauge value.
		/// </summary>
		/// <param name="name">The measurement name.</param>
		/// <param name="tags">Any additional tags to add to the <see cref="InfluxRecord"/>, these tags overwrite any global tags with the same name.</param>
		/// <param name="unit">The metric unit.</param>
		/// <param name="value">The metric value object.</param>
		/// <returns>A list of <see cref="InfluxRecord"/> instances for the specified metric value.</returns>
		public IEnumerable<InfluxRecord> GetRecords(String name, MetricTags tags, Unit unit, Double value) {
			yield return GetRecord(name, tags, new[] {
				new InfluxField("Value", value),
			});
		}

		/// <summary>
		/// Creates a new <see cref="InfluxRecord"/> instance for the counter value and any set items.
		/// </summary>
		/// <param name="name">The measurement name.</param>
		/// <param name="tags">Any additional tags to add to the <see cref="InfluxRecord"/>, these tags overwrite any global tags with the same name.</param>
		/// <param name="unit">The metric unit.</param>
		/// <param name="value">The metric value object.</param>
		/// <returns>A list of <see cref="InfluxRecord"/> instances for the specified metric value.</returns>
		public IEnumerable<InfluxRecord> GetRecords(String name, MetricTags tags, Unit unit, CounterValue value) {
			yield return GetRecord(name, tags, new[] {
				new InfluxField("Count", value.Count),
			});

			foreach (var i in value.Items) {
				yield return GetRecord(name, i.Item, tags, new[] {
					new InfluxField("Count",   i.Count),
					new InfluxField("Percent", i.Percent),
				});
			}
		}

		/// <summary>
		/// Creates a new <see cref="InfluxRecord"/> instance for the histogram value.
		/// </summary>
		/// <param name="name">The measurement name.</param>
		/// <param name="tags">Any additional tags to add to the <see cref="InfluxRecord"/>, these tags overwrite any global tags with the same name.</param>
		/// <param name="unit">The metric unit.</param>
		/// <param name="value">The metric value object.</param>
		/// <returns>A list of <see cref="InfluxRecord"/> instances for the specified metric value.</returns>
		public IEnumerable<InfluxRecord> GetRecords(String name, MetricTags tags, Unit unit, MeterValue value) {
			if (value == null) throw new ArgumentNullException(nameof(value));

			yield return GetRecord(name, tags, new[] {
				new InfluxField("Count",       value.Count),
				new InfluxField("Mean Rate",   value.MeanRate),
				new InfluxField("1 Min Rate",  value.OneMinuteRate),
				new InfluxField("5 Min Rate",  value.FiveMinuteRate),
				new InfluxField("15 Min Rate", value.FifteenMinuteRate),
			});

			foreach (var i in value.Items) {
				yield return GetRecord(name, i.Item, tags, new[] {
					new InfluxField("Count",       i.Value.Count),
					new InfluxField("Percent",     i.Percent),
					new InfluxField("Mean Rate",   i.Value.MeanRate),
					new InfluxField("1 Min Rate",  i.Value.OneMinuteRate),
					new InfluxField("5 Min Rate",  i.Value.FiveMinuteRate),
					new InfluxField("15 Min Rate", i.Value.FifteenMinuteRate),
				});
			}
		}

		/// <summary>
		/// Creates a new <see cref="InfluxRecord"/> instance for the histogram value.
		/// </summary>
		/// <param name="name">The measurement name.</param>
		/// <param name="tags">Any additional tags to add to the <see cref="InfluxRecord"/>, these tags overwrite any global tags with the same name.</param>
		/// <param name="unit">The metric unit.</param>
		/// <param name="value">The metric value object.</param>
		/// <returns>A list of <see cref="InfluxRecord"/> instances for the specified metric value.</returns>
		public IEnumerable<InfluxRecord> GetRecords(String name, MetricTags tags, Unit unit, HistogramValue value) {
			if (value == null) throw new ArgumentNullException(nameof(value));

			yield return GetRecord(name, tags, new[] {
				new InfluxField("Count",            value.Count),
				new InfluxField("Last",             value.LastValue),
				new InfluxField("Min",              value.Min),
				new InfluxField("Mean",             value.Mean),
				new InfluxField("Max",              value.Max),
				new InfluxField("StdDev",           value.StdDev),
				new InfluxField("Median",           value.Median),
				new InfluxField("Sample Size",      value.SampleSize),
				new InfluxField("Percentile 75%",   value.Percentile75),
				new InfluxField("Percentile 95%",   value.Percentile95),
				new InfluxField("Percentile 98%",   value.Percentile98),
				new InfluxField("Percentile 99%",   value.Percentile99),
				new InfluxField("Percentile 99.9%", value.Percentile999),

				// ignored histogram values
				//new InfluxField("Last User Value",  value.LastUserValue),
				//new InfluxField("Min User Value",   value.MinUserValue),
				//new InfluxField("Max User Value",   value.MaxUserValue),
			});
		}

		/// <summary>
		/// Creates new <see cref="InfluxRecord"/> instances for the timer values and any items in the meter item sets.
		/// </summary>
		/// <param name="name">The measurement name.</param>
		/// <param name="tags">Any additional tags to add to the <see cref="InfluxRecord"/>, these tags overwrite any global tags with the same name.</param>
		/// <param name="unit">The metric unit.</param>
		/// <param name="value">The metric value object.</param>
		/// <returns>A list of <see cref="InfluxRecord"/> instances for the specified metric value.</returns>
		public IEnumerable<InfluxRecord> GetRecords(String name, MetricTags tags, Unit unit, TimerValue value) {
			if (value == null) throw new ArgumentNullException(nameof(value));

			yield return GetRecord(name, tags, new[] {
				new InfluxField("Active Sessions",  value.ActiveSessions),
				new InfluxField("Total Time",       value.TotalTime),
				new InfluxField("Count",            value.Rate.Count),
				new InfluxField("Mean Rate",        value.Rate.MeanRate),
				new InfluxField("1 Min Rate",       value.Rate.OneMinuteRate),
				new InfluxField("5 Min Rate",       value.Rate.FiveMinuteRate),
				new InfluxField("15 Min Rate",      value.Rate.FifteenMinuteRate),
				new InfluxField("Last",             value.Histogram.LastValue),
				new InfluxField("Min",              value.Histogram.Min),
				new InfluxField("Mean",             value.Histogram.Mean),
				new InfluxField("Max",              value.Histogram.Max),
				new InfluxField("StdDev",           value.Histogram.StdDev),
				new InfluxField("Median",           value.Histogram.Median),
				new InfluxField("Sample Size",      value.Histogram.SampleSize),
				new InfluxField("Percentile 75%",   value.Histogram.Percentile75),
				new InfluxField("Percentile 95%",   value.Histogram.Percentile95),
				new InfluxField("Percentile 98%",   value.Histogram.Percentile98),
				new InfluxField("Percentile 99%",   value.Histogram.Percentile99),
				new InfluxField("Percentile 99.9%", value.Histogram.Percentile999),
				
				// ignored histogram values
				//new InfluxField("Last User Value",  value.Histogram.LastUserValue),
				//new InfluxField("Min User Value",   value.Histogram.MinUserValue),
				//new InfluxField("Max User Value",   value.Histogram.MaxUserValue),
			});

			// NOTE: I'm not sure if this is needed, it appears the timer only adds set item values
			// to the histogram and not to the meter. I'm not sure if this is a bug or by design.
			foreach (var i in value.Rate.Items) {
				yield return GetRecord(name, i.Item, tags, new[] {
					new InfluxField("Count",       i.Value.Count),
					new InfluxField("Percent",     i.Percent),
					new InfluxField("Mean Rate",   i.Value.MeanRate),
					new InfluxField("1 Min Rate",  i.Value.OneMinuteRate),
					new InfluxField("5 Min Rate",  i.Value.FiveMinuteRate),
					new InfluxField("15 Min Rate", i.Value.FifteenMinuteRate),
				});
			}
		}

		/// <summary>
		/// Creates new <see cref="InfluxRecord"/> instances for each HealthCheck result in the specified <paramref name="status"/>.
		/// </summary>
		/// <param name="status">The health status.</param>
		/// <returns></returns>
		public IEnumerable<InfluxRecord> GetRecords(HealthStatus status) {
			foreach (var result in status.Results) {
				//var name = InfluxUtils.LowerAndReplaceSpaces(result.Name);
				//var nameWithTags = Regex.IsMatch(result.Name, "^[Nn]ame=") ? result.Name : $"Name={result.Name}";
				var split = Regex.Split(result.Name, @"(?<!\\)[,]").Select(t => t.Trim()).Where(t => t.Length > 0).ToArray();
				if (!Regex.IsMatch(split[0], "^[Nn]ame=")) split[0] = $"Name={InfluxUtils.LowerAndReplaceSpaces(split[0])}";
				var name = String.Join(",", split);
				yield return GetRecord("Health Checks", name, new[] {
					new InfluxField("IsHealthy", result.Check.IsHealthy),
					new InfluxField("Message",   result.Check.Message),
				});
			}
		}



		/// <summary>
		/// Creates a new <see cref="InfluxRecord"/> from the specified name, tags, and fields.
		/// This uses the timestamp defined on this metrics converter instance.
		/// </summary>
		/// <param name="name">The measurement or series name. This value is required and cannot be null or empty.</param>
		/// <param name="tags">The optional tags to associate with this record.</param>
		/// <param name="fields">The <see cref="InfluxField"/> values for the output fields.</param>
		/// <returns>A new <see cref="InfluxRecord"/> from the specified name, tags, and fields.</returns>
		public InfluxRecord GetRecord(String name, MetricTags tags, IEnumerable<InfluxField> fields) {
			return GetRecord(name, null, tags, fields);
		}

		/// <summary>
		/// Creates a new <see cref="InfluxRecord"/> from the specified name, item name, tags, and fields.
		/// This uses the timestamp defined on this metrics converter instance.
		/// </summary>
		/// <param name="name">The measurement or series name. This value is required and cannot be null or empty.</param>
		/// <param name="itemName">The set item name. Can contain comma-separated key/value pairs.</param>
		/// <param name="tags">The optional tags to associate with this record.</param>
		/// <param name="fields">The <see cref="InfluxField"/> values for the output fields.</param>
		/// <returns>A new <see cref="InfluxRecord"/> from the specified name, tags, and fields.</returns>
		public InfluxRecord GetRecord(String name, String itemName, MetricTags tags, IEnumerable<InfluxField> fields) {
			var jtags = InfluxUtils.JoinTags(itemName, GlobalTags, tags); // global tags must be first so they can get overridden
			var record = new InfluxRecord(name, jtags, fields, Timestamp);
			return record;
		}
	}

	public class DefaultConverter : InfluxdbConverter
	{
		/// <summary>
		/// Creates a new <see cref="DefaultConverter"/> using the specified tags.
		/// </summary>
		/// <param name="globalTags">The global tags that are added to all created <see cref="InfluxRecord"/> instances.</param>
		public DefaultConverter(MetricTags? globalTags = null)
			: base(globalTags) {
		}
	}
}
