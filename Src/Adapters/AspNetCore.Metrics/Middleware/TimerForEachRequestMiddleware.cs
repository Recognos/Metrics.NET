using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;
using Metrics.Utils;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics.Middleware
{
    public class TimerForEachRequestMiddleware : MetricMiddleware
    {
        private readonly MetricsContext context;

        public TimerForEachRequestMiddleware(MetricsContext context, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.context = context;
        }

        public async Task Invoke(HttpContext context, Func<Task> next)
        {
            if (PerformMetric(context))
            {
                var startTime = Clock.Default.Nanoseconds;
                
                await next();

                var httpResponseStatusCode = int.Parse(context.Response.StatusCode.ToString());
                var metricName = context.Request.Path.ToString();

//                if (environment.ContainsKey("metrics-net.routetemplate"))
//                {
//                    var requestMethod = environment["owin.RequestMethod"] as string;
//                    var routeTemplate = environment["metrics-net.routetemplate"] as string;
//
//                    metricName = requestMethod.ToUpperInvariant() + " " + routeTemplate;
//                }

                if (httpResponseStatusCode != (int)HttpStatusCode.NotFound)
                {
                    var elapsed = Clock.Default.Nanoseconds - startTime;
                    this.context.Timer(metricName, Unit.Requests)
                        .Record(elapsed, TimeUnit.Nanoseconds);
                }
            }
            else
            {
                await next();
            }
        }
    }
}
