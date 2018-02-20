using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HttpStatusCodeMeterMiddleware : MetricMiddleware
    {
        private readonly Meter httpStatusCodeMeter;

        public HttpStatusCodeMeterMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.httpStatusCodeMeter = context.Meter(metricName, Unit.Errors);
        }

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            await next();

            if (PerformMetric(context))
            {
                this.httpStatusCodeMeter.Mark(context.Response.StatusCode.ToString());
            }
        }
    }
}
