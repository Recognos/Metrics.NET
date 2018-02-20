using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;
using Metrics.Reports;
using Owin.Metrics.Middleware;
namespace Owin.Metrics
{
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task>, System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task>>;
    
    public class OwinMetricsConfig
    {
        public static readonly OwinMetricsConfig Disabled = new OwinMetricsConfig();

        private readonly Action<MidFunc> middlewareRegistration;
        private readonly MetricsContext context;
        private readonly Func<HealthStatus> healthStatus;

        private readonly bool isDisabled;

        public OwinMetricsConfig(Action<MidFunc> middlewareRegistration, MetricsContext context, Func<HealthStatus> healthStatus)
        {
            this.middlewareRegistration = middlewareRegistration;
            this.context = context;
            this.healthStatus = healthStatus;
        }

        private OwinMetricsConfig()
        {
            this.isDisabled = true;
        }

        /// <summary>
        /// Register all predefined metrics.
        /// </summary>
        /// <param name="ignoreRequestPathPatterns">Patterns for paths to ignore.</param>
        /// <param name="owinContext">Name of the metrics context where to register the metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public OwinMetricsConfig WithRequestMetricsConfig(Regex[] ignoreRequestPathPatterns = null, string owinContext = "Owin")
        {
            if (this.isDisabled)
            {
                return this;
            }

            return WithRequestMetricsConfig(config => config.WithAllOwinMetrics(), ignoreRequestPathPatterns, owinContext);
        }

        /// <summary>
        /// Configure which Owin metrics to be registered.
        /// </summary>
        /// <param name="config">Action used to configure Owin metrics.</param>
        /// <param name="ignoreRequestPathPatterns">Patterns for paths to ignore.</param>
        /// <param name="owinContext">Name of the metrics context where to register the metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public OwinMetricsConfig WithRequestMetricsConfig(Action<OwinRequestMetricsConfig> config,
            Regex[] ignoreRequestPathPatterns = null, string owinContext = "Owin")
        {
            if (this.isDisabled)
            {
                return this;
            }

            OwinRequestMetricsConfig requestConfig = new OwinRequestMetricsConfig(this.middlewareRegistration, this.context.Context(owinContext), ignoreRequestPathPatterns);
            config(requestConfig);
            return this;
        }

        /// <summary>
        /// Expose Owin metrics endpoint
        /// </summary>
        /// <returns>Chainable configuration object.</returns>
        public OwinMetricsConfig WithMetricsEndpoint()
        {
            if (this.isDisabled)
            {
                return this;
            }

            WithMetricsEndpoint(_ => { });
            return this;
        }

        /// <summary>
        /// Configure Owin metrics endpoint.
        /// </summary>
        /// <param name="config">Action used to configure the Owin Metrics endpoint.</param>
        /// <param name="endpointPrefix">The relative path the endpoint will be available at.</param>
        /// <returns>Chainable configuration object.</returns>
        public OwinMetricsConfig WithMetricsEndpoint(Action<MetricsEndpointReports> config, string endpointPrefix = "metrics")
        {
            if (this.isDisabled)
            {
                return this;
            }

            var endpointConfig = new MetricsEndpointReports(this.context.DataProvider, this.healthStatus);
            config(endpointConfig);
            
            this.middlewareRegistration(next => new MetricsEndpointMiddleware(next, endpointPrefix, endpointConfig).Invoke);
            return this;
        }
    }
}
