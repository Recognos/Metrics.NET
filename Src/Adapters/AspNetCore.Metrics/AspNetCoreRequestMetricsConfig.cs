using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspNetCore.Metrics.Middleware;
using Metrics;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics
{
    public class AspNetCoreRequestMetricsConfig
    {
        private readonly MetricsContext metricsContext;
        private readonly Action<Func<HttpContext, Func<Task>, Task>> middlewareRegistration;
        private readonly Regex[] ignoreRequestPathPatterns;

        public AspNetCoreRequestMetricsConfig(Action<Func<HttpContext, Func<Task>, Task>> middlewareRegistration, MetricsContext metricsContext, Regex[] ignoreRequestPathPatterns)
        {
            this.middlewareRegistration = middlewareRegistration;
            this.metricsContext = metricsContext;
            this.ignoreRequestPathPatterns = ignoreRequestPathPatterns;
        }

        /// <summary>
        /// Configure global OWIN Metrics.
        /// Available global metrics are: Request Timer, Active Requests Counter, Error Meter
        /// </summary>
        /// <returns>
        /// This instance to allow chaining of the configuration.
        /// </returns>
        public AspNetCoreRequestMetricsConfig WithAllAspNetCoreMetrics()
        {
            WithRequestTimer();
            WithActiveRequestCounter();
            WithPostAndPutRequestSizeHistogram();
            WithTimerForEachRequest();
            WithErrorsMeter();
            return this;
        }

        /// <summary>
        /// Registers a Timer metric named "Owin.Requests" that records how many requests per second are handled and also
        /// keeps a histogram of the request duration.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public AspNetCoreRequestMetricsConfig WithRequestTimer(string metricName = "Requests")
        {
            var metricsMiddleware = new RequestTimerMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            this.middlewareRegistration(metricsMiddleware.Invoke);
            return this;
        }

        /// <summary>
        /// Registers a Counter metric named "Owin.ActiveRequests" that shows the current number of active requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public AspNetCoreRequestMetricsConfig WithActiveRequestCounter(string metricName = "Active Requests")
        {
            var metricsMiddleware = new ActiveRequestCounterMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            this.middlewareRegistration(metricsMiddleware.Invoke);
            return this;
        }

        /// <summary>
        /// Register a Histogram metric named "Owin.PostAndPutRequestsSize" on the size of the POST and PUT requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public AspNetCoreRequestMetricsConfig WithPostAndPutRequestSizeHistogram(string metricName = "Post & Put Request Size")
        {
            var metricsMiddleware = new PostAndPutRequestSizeHistogramMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            this.middlewareRegistration(metricsMiddleware.Invoke);
            return this;
        }

        /// <summary>
        /// Registers a timer for each request.
        /// Timer is created based on route and will be named:
        /// Owin.{HTTP_METHOD_NAME} [{ROUTE_PATH}]
        /// </summary>
        public AspNetCoreRequestMetricsConfig WithTimerForEachRequest()
        {
            var metricsMiddleware = new TimerForEachRequestMiddleware(this.metricsContext, this.ignoreRequestPathPatterns);
            this.middlewareRegistration(metricsMiddleware.Invoke);
            return this;
        }

        /// <summary>
        /// Registers a Meter metric named "Owin.Errors" that records the rate at which unhanded errors occurred while 
        /// processing Nancy requests.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public AspNetCoreRequestMetricsConfig WithErrorsMeter(string metricName = "Errors")
        {
            var metricsMiddleware = new ErrorMeterMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            this.middlewareRegistration(metricsMiddleware.Invoke);
            return this;
        }

        /// <summary>
        /// Registers a Meter metric named "Owin.HttpStatusCodes" that records the rate at which given HTTP stats codes 
        /// are returned.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public AspNetCoreRequestMetricsConfig WithHttpStatusCodeMeter(string metricName = "HttpStatusCodes")
        {
            var metricsMiddleware = new HttpStatusCodeMeterMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            this.middlewareRegistration(metricsMiddleware.Invoke);
            return this;
        }
    }
}