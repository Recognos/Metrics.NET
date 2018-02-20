using System;
using System.Text.RegularExpressions;
using Metrics;
using Owin.Metrics.Middleware;

namespace Owin.Metrics
{   
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task>, System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task>>;
    
    public class OwinRequestMetricsConfig
    {
        private readonly MetricsContext metricsContext;
        private readonly Action<MidFunc> middlewareRegistration;
        private readonly Regex[] ignoreRequestPathPatterns;

        public OwinRequestMetricsConfig(Action<MidFunc> middlewareRegistration, MetricsContext metricsContext, Regex[] ignoreRequestPathPatterns)
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
        public OwinRequestMetricsConfig WithAllOwinMetrics()
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
        public OwinRequestMetricsConfig WithRequestTimer(string metricName = "Requests")
        {
            middlewareRegistration(next => new RequestTimerMiddleware(next, this.metricsContext, metricName, this.ignoreRequestPathPatterns).Invoke);
            return this;
        }

        /// <summary>
        /// Registers a Counter metric named "Owin.ActiveRequests" that shows the current number of active requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithActiveRequestCounter(string metricName = "Active Requests")
        {
            middlewareRegistration(next => new ActiveRequestCounterMiddleware(next, this.metricsContext, metricName, this.ignoreRequestPathPatterns).Invoke);
            return this;
        }

        /// <summary>
        /// Register a Histogram metric named "Owin.PostAndPutRequestsSize" on the size of the POST and PUT requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithPostAndPutRequestSizeHistogram(string metricName = "Post & Put Request Size")
        {
            middlewareRegistration(next => new PostAndPutRequestSizeHistogramMiddleware(next, this.metricsContext, metricName, this.ignoreRequestPathPatterns).Invoke);
            return this;
        }

        /// <summary>
        /// Registers a timer for each request.
        /// Timer is created based on route and will be named:
        /// Owin.{HTTP_METHOD_NAME} [{ROUTE_PATH}]
        /// </summary>
        public OwinRequestMetricsConfig WithTimerForEachRequest()
        {
            middlewareRegistration(next => new TimerForEachRequestMiddleware(next, this.metricsContext, this.ignoreRequestPathPatterns).Invoke);
            return this;
        }

        /// <summary>
        /// Registers a Meter metric named "Owin.Errors" that records the rate at which unhanded errors occurred while 
        /// processing Nancy requests.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithErrorsMeter(string metricName = "Errors")
        {
            middlewareRegistration(next => new ErrorMeterMiddleware(next, this.metricsContext, metricName, this.ignoreRequestPathPatterns).Invoke);
            return this;
        }

        /// <summary>
        /// Registers a Meter metric named "Owin.HttpStatusCodes" that records the rate at which given HTTP stats codes 
        /// are returned.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithHttpStatusCodeMeter(string metricName = "HttpStatusCodes")
        {
            middlewareRegistration(next => new HttpStatusCodeMeterMiddleware(next, this.metricsContext, metricName, this.ignoreRequestPathPatterns).Invoke);
            return this;
        }
    }
}