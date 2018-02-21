using System;

namespace Metrics.Core
{
    public class HealthCheck
    {
        public struct Result
        {
            public readonly string Name;
            public readonly HealthCheckResult Check;
            public readonly MetricTags Tags;

            public Result(string name, HealthCheckResult check, MetricTags tags = default(MetricTags))
            {
                this.Name = name;
                this.Check = check;
                this.Tags = tags;
            }
        }

        private readonly Func<HealthCheckResult> check;

        protected HealthCheck(string name, MetricTags tags = default(MetricTags))
            : this(name, () => { }, tags)
        { }

        public HealthCheck(string name, Action check, MetricTags tags = default(MetricTags))
            : this(name, () => { check(); return string.Empty; }, tags)
        { }

        public HealthCheck(string name, Func<string> check, MetricTags tags = default(MetricTags))
            : this(name, () => HealthCheckResult.Healthy(check()), tags)
        { }

        public HealthCheck(string name, Func<HealthCheckResult> check, MetricTags tags = default(MetricTags))
        {
            this.Name = name;
            this.check = check;
            this.Tags = tags;
        }

        public string Name { get; }
        public MetricTags Tags { get; set; }

        protected virtual HealthCheckResult Check()
        {
            return this.check();
        }

        public Result Execute()
        {
            try
            {
                return new Result(this.Name, this.Check(), Tags);
            }
            catch (Exception x)
            {
                return new Result(this.Name, HealthCheckResult.Unhealthy(x), Tags);
            }
        }
    }
}
