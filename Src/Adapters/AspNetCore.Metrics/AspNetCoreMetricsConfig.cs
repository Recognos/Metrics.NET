using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspNetCore.Metrics.Middleware;
using Metrics;
using Metrics.Reports;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics
{
    public class AspNetCoreMetricsConfig
    {
        public static readonly AspNetCoreMetricsConfig Disabled = new AspNetCoreMetricsConfig();

        private readonly Action<Func<HttpContext, Func<Task>, Task>> middlewareRegistration;
        private readonly MetricsContext context;
        private readonly Func<HealthStatus> healthStatus;

        private readonly bool isDisabled;

        public AspNetCoreMetricsConfig(Action<Func<HttpContext, Func<Task>, Task>> middlewareRegistration, MetricsContext context, Func<HealthStatus> healthStatus)
        {
            this.middlewareRegistration = middlewareRegistration;
            this.context = context;
            this.healthStatus = healthStatus;
        }

        private AspNetCoreMetricsConfig()
        {
            this.isDisabled = true;
        }

        /// <summary>
        /// Register all predefined AspNetCore metrics.
        /// </summary>
        /// <param name="ignoreRequestPathPatterns">Patterns for paths to ignore.</param>
        /// <param name="aspNetCoreContext">Name of the metrics context where to register the metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public AspNetCoreMetricsConfig WithRequestMetricsConfig(Regex[] ignoreRequestPathPatterns = null, string aspNetCoreContext = "AspNetCore")
        {
            if (this.isDisabled)
            {
                return this;
            }

            return WithRequestMetricsConfig(config => config.WithAllAspNetCoreMetrics(), ignoreRequestPathPatterns, aspNetCoreContext);
        }

        /// <summary>
        /// Configure which AspNetCore metrics to be registered.
        /// </summary>
        /// <param name="config">Action used to configure AspNetCore metrics.</param>
        /// <param name="ignoreRequestPathPatterns">Patterns for paths to ignore.</param>
        /// <param name="aspNetCoreContext">Name of the metrics context where to register the metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public AspNetCoreMetricsConfig WithRequestMetricsConfig(Action<AspNetCoreRequestMetricsConfig> config,
            Regex[] ignoreRequestPathPatterns = null, string aspNetCoreContext = "AspNetCore")
        {
            if (this.isDisabled)
            {
                return this;
            }

            AspNetCoreRequestMetricsConfig requestConfig = new AspNetCoreRequestMetricsConfig(this.middlewareRegistration, this.context.Context(aspNetCoreContext), ignoreRequestPathPatterns);
            config(requestConfig);
            return this;
        }

        /// <summary>
        /// Expose AspNetCore metrics endpoint
        /// </summary>
        /// <returns>Chainable configuration object.</returns>
        public AspNetCoreMetricsConfig WithMetricsEndpoint()
        {
            if (this.isDisabled)
            {
                return this;
            }

            WithMetricsEndpoint(_ => { });
            return this;
        }

        /// <summary>
        /// Configure AspNetCore metrics endpoint.
        /// </summary>
        /// <param name="config">Action used to configure the Owin Metrics endpoint.</param>
        /// <param name="endpointPrefix">The relative path the endpoint will be available at.</param>
        /// <returns>Chainable configuration object.</returns>
        public AspNetCoreMetricsConfig WithMetricsEndpoint(Action<MetricsEndpointReports> config, string endpointPrefix = "metrics")
        {
            if (this.isDisabled)
            {
                return this;
            }

            var endpointConfig = new MetricsEndpointReports(this.context.DataProvider, this.healthStatus);
            config(endpointConfig);
            
            var metricsEndpointMiddleware = new AspNetCoreMetricsEndpointMiddleware(endpointPrefix, endpointConfig);
            this.middlewareRegistration(metricsEndpointMiddleware.Invoke);
            return this;
        }
    }
}