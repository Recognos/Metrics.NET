using System;
using System.Text.RegularExpressions;
using Metrics.MetricData;
using System.Threading;
using System.Globalization;

namespace Metrics.Reporters
{
    public sealed class PrometheusReport : BaseReport
    {
        private string reportText;
        private long reportTime;

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private PrometheusReport()
        {
            reportText = "";
            reportTime = CurrentTimeMillis();
        }

        private static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
        }

        private static Regex rgx = new Regex("[^a-z0-9A-Z:_]");

        protected override string FormatMetricName<T>(string context, MetricValueSource<T> metric)
        {
            return rgx.Replace(metric.Name, "_");
        }

        private void WriteMetric<T>(string type, string name, T value, Unit unit, MetricTags tags)
        {
            IFormatProvider culture = new CultureInfo("en-US");
            reportText += string.Format(culture, "# TYPE {0} {1}\n", name, type);
            string lowercased = name.ToLower();

            if (unit.Name.Equals(Unit.KiloBytes.Name))
            {
                reportText += string.Format(culture, "{0}_in_kb {1} {2}\n", lowercased, value, reportTime);
            }
            else if (unit.Name.Equals(Unit.MegaBytes.Name))
            {
                reportText += string.Format(culture, "{0}_in_mb {1} {2}\n", lowercased, value, reportTime);
            }
            else if (unit.Name.Equals(Unit.Percent))
            {
                reportText += string.Format(culture, "{0}_pct {1} {2}\n", lowercased, value, reportTime);
            }
            else
            {
                reportText += string.Format(culture, "{0} {1} {2}\n", lowercased, value, reportTime);
            }

            reportText += "\n";
            return;
        }

        protected override void StartReport(string contextName)
        {
            // Do nothing
        }

        protected override void StartMetricGroup(string metricType)
        {
            // Do nothing
        }

        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            this.WriteMetric("gauge", name, value, unit, tags);
        }

        protected override void ReportCounter(string name, CounterValue value, Unit unit, MetricTags tags)
        {
            this.WriteMetric("counter", name, value.Count, unit, tags);
        }

        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            this.WriteMetric("counter", name, value.Count, unit, tags);
        }

        protected override void ReportHistogram(string name, HistogramValue value, Unit unit, MetricTags tags)
        {
            // Do nothing -- not implemented YET, but we don't wanna crash the program
        }

        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            // Do nothing -- not implemented YET, but we don't wanna crash the program
        }

        protected override void ReportHealth(HealthStatus status)
        {
            this.WriteMetric("gauge", "healthz", (long) (status.IsHealthy ? 1 : 0), Unit.None, MetricTags.None);
        }

        internal static string RenderMetrics(MetricsData currentMetricsData, Func<HealthStatus> healthStatus)
        {
            var report = new PrometheusReport();
            report.RunReport(currentMetricsData, healthStatus, CancellationToken.None);
            return report.reportText;
        }
    }
}
