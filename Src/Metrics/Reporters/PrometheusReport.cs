using System;
using System.Text;
using System.Text.RegularExpressions;
using Metrics.MetricData;
using System.Threading;

namespace Metrics.Reporters
{
    public sealed class PrometheusReport : BaseReport
    {
        private StringBuilder reportText;

        private PrometheusReport()
        {
            reportText = new StringBuilder();
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private long CurrentTimeMillis()
        {
            TimeSpan ts = base.CurrentContextTimestamp - epoch;
            return ts.Ticks / TimeSpan.TicksPerMillisecond;
        }

        private static Regex rgx = new Regex("[^a-z0-9A-Z:_]");
        private static string FormatName(string name)
        {
            return rgx.Replace(name, "_");
        }

        protected override string FormatMetricName<T>(string context, MetricValueSource<T> metric)
        {
            return FormatName(metric.Name);
        }


        private static string formatLong(long value)
        {
            return value.ToString();
        }

        private static string formatDouble(double value) {
            // This deals with the case where the locale is not set to en-US: we don't want to
            // change the locale, and just want to whitelist the values that prometheus accepts.
            if (Double.IsNaN(value))
            {
                return "NaN";
            }
            else if (Double.IsPositiveInfinity(value))
            {
                return "Inf";
            }
            else if (Double.IsNegativeInfinity(value))
            {
                return "-Inf";
            }
            else
            {
                return value.ToString();
            }
        }

        private static string suffixFromUnit(Unit unit)
        {
            if (unit.Name.Equals(Unit.KiloBytes.Name))
            {
                return "_in_kb";
            }
            else if (unit.Name.Equals(Unit.MegaBytes.Name))
            {
                return "_in_mb";
            }
            else if (unit.Name.Equals(Unit.Percent))
            {
                return "_pct";
            }
            else
            {
                return "";
            }
        }

        private void WriteStringMetric(string type, string name, string value, Unit unit, MetricTags tags)
        {
            // Type line
            reportText.Append("# TYPE ");
            reportText.Append(name);
            reportText.Append(" ");
            reportText.Append(type);
            reportText.Append("\n");

            // Actual metric line
            reportText.Append(name.ToLower());
            reportText.Append(suffixFromUnit(unit));
            if (tags.Tags.Length > 0)
            {
                reportText.Append('{');
                for (int i = 0; i < tags.Tags.Length; i++) {
                    if (i != 0) reportText.Append(",");
                    reportText.Append("tag");
                    reportText.Append(i);
                    reportText.Append('=');
                    reportText.Append('"');
                    reportText.Append(FormatName(tags.Tags[i]));
                    reportText.Append('"');
                }
                reportText.Append('}');
            }
            reportText.Append(' ');
            reportText.Append(value);
            reportText.Append(' ');
            reportText.Append(CurrentTimeMillis());
            reportText.Append("\n\n"); // Extra end-of-line
            return;
        }

        private void WriteDoubleMetric(string type, string name, double value, Unit unit, MetricTags tags)
        {
            this.WriteStringMetric(type, name, formatDouble(value), unit, tags);
        }

        private void WriteLongMetric(string type, string name, long value, Unit unit, MetricTags tags)
        {
            this.WriteStringMetric(type, name, formatLong(value), unit, tags);
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
            this.WriteDoubleMetric("gauge", name, value, unit, tags);
        }

        protected override void ReportCounter(string name, CounterValue value, Unit unit, MetricTags tags)
        {
            // Metrics.NET counters can be decremented, but Prometheus' are expected to be monotonically increasing,
            // so this maps to a Prometheus gauge.
            this.WriteLongMetric("gauge", name, value.Count, unit, tags);
        }

        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            // Metrics.NET counters can be decremented, but Prometheus' are expected to be monotonically increasing,
            // so this maps to a Prometheus gauge.
            this.WriteLongMetric("gauge", name, value.Count, unit, tags);
        }

        protected override void ReportHistogram(string name, HistogramValue value, Unit unit, MetricTags tags)
        {
            // The semantics between prometheus and Metrics.NET are different enough that we just want to pass a gauge
            this.WriteLongMetric("gauge", name, value.Count, unit, tags);
        }

        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            // The semantics between prometheus and Metrics.NET are different enough that we just want to pass a gauge
            this.WriteLongMetric("gauge", name, value.Histogram.Count, unit, tags);
        }

        protected override void ReportHealth(HealthStatus status)
        {
            this.WriteLongMetric("gauge", "healthz", (long) (status.IsHealthy ? 1 : 0), Unit.None, MetricTags.None);
        }

        internal static string RenderMetrics(MetricsData currentMetricsData, Func<HealthStatus> healthStatus)
        {
            var report = new PrometheusReport();
            report.RunReport(currentMetricsData, healthStatus, CancellationToken.None);
            return report.reportText.ToString();
        }
    }
}
