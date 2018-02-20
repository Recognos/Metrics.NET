using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics.Middleware
{
    public class RequestTimerMiddleware : MetricMiddleware
    {
        private readonly Timer requestTimer;

        public RequestTimerMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.requestTimer = context.Timer(metricName, Unit.Requests);
        }

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            if (base.PerformMetric(context))
            {
                using (this.requestTimer.NewContext())
                {
                    await next();
                }
            }
            else
            {
                await next();
            }
        }
    }
}