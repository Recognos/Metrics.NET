using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics.Middleware
{
    public class ErrorMeterMiddleware : MetricMiddleware
    {
        private readonly Meter errorMeter;
        
        public ErrorMeterMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.errorMeter = context.Meter(metricName, Unit.Errors);
        }

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            await next();
            
            if (PerformMetric(context))
            {
                var httpResponseStatusCode = int.Parse(context.Response.StatusCode.ToString());

                if (httpResponseStatusCode == (int)HttpStatusCode.InternalServerError)
                {
                    this.errorMeter.Mark();
                }
            }
        }
    }
}
