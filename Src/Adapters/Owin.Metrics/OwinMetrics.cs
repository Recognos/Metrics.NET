using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metrics;

namespace Owin.Metrics
{
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task>, System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task>>;
    using OwinPipeline = Action<Action<System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task>, System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task>>>>;
    /// <summary>
    /// Helper class to register OWIN Metrics
    /// </summary>
    public static class OwinMetrics
    {

        /// <summary>
        /// Add Metrics Middleware to the Owin pipeline.
        /// Sample: Metric.Config.WithOwin( m => app.Use(m)) 
        /// Where app is the IAppBuilder
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="middlewareRegistration">Action used to register middleware. This should generally be app.Use(middleware)</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithOwin(this MetricsConfig config, Action<MidFunc> middlewareRegistration)
        {
            return config.WithOwin(middlewareRegistration, owin =>
                                owin.WithRequestMetricsConfig()
                                    .WithMetricsEndpoint());
        }

        /// <summary>
        /// Add Metrics Middleware to the Owin pipeline.
        /// Sample: Metric.Config.WithOwin( m => app.Use(m)) 
        /// Where app is the IAppBuilder
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="middlewareRegistration">Action used to register middleware. This should generally be app.Use(middleware)</param>
        /// <param name="owinConfig">Action used to configure Owin metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithOwin(this MetricsConfig config, Action<MidFunc> middlewareRegistration, Action<OwinMetricsConfig> owinConfig)
        {
            var owin = config.WithConfigExtension((ctx, hs) => new OwinMetricsConfig(middlewareRegistration, ctx, hs), () => OwinMetricsConfig.Disabled);
            owinConfig(owin);
            return config;
        }
    }
}
