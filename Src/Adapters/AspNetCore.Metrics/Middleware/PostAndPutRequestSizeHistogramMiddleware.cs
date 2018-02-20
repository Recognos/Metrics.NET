using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics.Middleware
{
    public class PostAndPutRequestSizeHistogramMiddleware : MetricMiddleware
    {
        private readonly Histogram histogram;
        
        public PostAndPutRequestSizeHistogramMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.histogram = context.Histogram(metricName, Unit.Bytes);
        }

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            if (PerformMetric(context))
            {
                var httpMethod = context.Request.Method.ToString().ToUpper();

                if (httpMethod == "POST" || httpMethod == "PUT")
                {
                    var contentLength = context.Request.Headers.ContentLength;
                    if (contentLength.HasValue)
                    {
                        this.histogram.Update(contentLength.Value);
                    }
                }
            }

            await next();
        }
    }
}
