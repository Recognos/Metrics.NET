using System;
using Metrics.Core;
using Metrics.MetricData;
using Metrics.Sampling;
using Metrics.Utils;

namespace Metrics.Tests
{
    public class TestMetricsBuilder : MetricsBuilder
    {
        private readonly Clock clock;
        private readonly Scheduler scheduler;

        public TestMetricsBuilder(Clock clock, Scheduler scheduler)
        {
            this.clock = clock;
            this.scheduler = scheduler;
        }

#if NET45
        public MetricValueProvider<double> BuildPerformanceCounter(string name, Unit unit, string counterCategory, string counterName, string counterInstance)
        {
            return new Metrics.PerfCounters.PerformanceCounterGauge(counterCategory, counterName, counterInstance);
        }
#else
        public MetricValueProvider<double> BuildPerformanceCounter(string name, Unit unit, string counterCategory, string counterName, string counterInstance)
        {
            throw new NotSupportedException();
        }
#endif
        
        public MetricValueProvider<double> BuildGauge(string name, Unit unit, Func<double> valueProvider)
        {
            return new FunctionGauge(valueProvider);
        }

        public CounterImplementation BuildCounter(string name, Unit unit)
        {
            return new CounterMetric();
        }

        public MeterImplementation BuildMeter(string name, Unit unit, TimeUnit rateUnit)
        {
            return new MeterMetric(this.clock, this.scheduler);
        }

        public HistogramImplementation BuildHistogram(string name, Unit unit, SamplingType samplingType)
        {
            return new HistogramMetric(new UniformReservoir());
        }

        public HistogramImplementation BuildHistogram(string name, Unit unit, Reservoir reservoir)
        {
            return new HistogramMetric(new UniformReservoir());
        }

        public TimerImplementation BuildTimer(string name, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, SamplingType samplingType)
        {
            return new TimerMetric(new HistogramMetric(new UniformReservoir()), new MeterMetric(this.clock, this.scheduler), this.clock);
        }

        public TimerImplementation BuildTimer(string name, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, HistogramImplementation histogram)
        {
            return new TimerMetric(new HistogramMetric(new UniformReservoir()), new MeterMetric(this.clock, this.scheduler), this.clock);
        }

        public TimerImplementation BuildTimer(string name, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, Reservoir reservoir)
        {
            return new TimerMetric(new HistogramMetric(new UniformReservoir()), new MeterMetric(this.clock, this.scheduler), this.clock);
        }
    }
}
