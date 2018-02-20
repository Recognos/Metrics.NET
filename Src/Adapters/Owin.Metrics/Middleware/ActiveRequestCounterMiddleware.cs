using Metrics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Owin middleware that counts the number of active requests.
    /// </summary>
    public class ActiveRequestCounterMiddleware : MetricMiddleware
    {
        private readonly Counter activeRequests;
        private AppFunc next;

        public ActiveRequestCounterMiddleware(AppFunc next, MetricsContext context, string metricName, Regex[] ignorePatterns)
            : this(context, metricName, ignorePatterns)
        {
            this.next = next;
        }

        public ActiveRequestCounterMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.activeRequests = context.Counter(metricName, Unit.Custom("ActiveRequests"));
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                this.activeRequests.Increment();

                await this.next(environment);

                this.activeRequests.Decrement();
            }
            else
            {
                await this.next(environment);
            }
        }
    }
}
