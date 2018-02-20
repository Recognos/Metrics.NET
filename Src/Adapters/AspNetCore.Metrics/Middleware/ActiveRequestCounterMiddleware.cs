using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics.Middleware
{
    /// <summary>
    /// Owin middleware that counts the number of active requests.
    /// </summary>
    public class ActiveRequestCounterMiddleware : MetricMiddleware
    {
        private readonly Counter activeRequests;

        public ActiveRequestCounterMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.activeRequests = context.Counter(metricName, Unit.Custom("ActiveRequests"));
        }

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            if (PerformMetric(context))
            {
                this.activeRequests.Increment();

                await next();

                this.activeRequests.Decrement();
            }
            else
            {
                await next();
            }
        }
    }
}
