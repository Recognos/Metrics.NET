using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics
{
    /// <summary>
    /// Helper class to register OWIN Metrics
    /// </summary>
    public static class AspNetCoreMetrics
    {
        /// <summary>
        /// Add Metrics Middleware to the AspNetCore pipeline.
        /// Sample: Metric.Config.WithAspNetCore( m => app.Use(m)) 
        /// Where app is the IAppBuilder
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="middlewareRegistration">Action used to register middleware. This should generally be app.Use(middleware)</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithAspNetCore(this MetricsConfig config, Action<Func<HttpContext, Func<Task>, Task>> middlewareRegistration)
        {
            return config.WithAspNetCore(middlewareRegistration, owin =>
                owin.WithRequestMetricsConfig());
        }

        /// <summary>
        /// Add Metrics Middleware to the AspNetCore pipeline.
        /// Sample: Metric.Config.WithAspNetCore( m => app.Use(m)) 
        /// Where app is the IAppBuilder
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="middlewareRegistration">Action used to register middleware. This should generally be app.Use(middleware)</param>
        /// <param name="aspNetCoreConfig">Action used to configure AspNetCore metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithAspNetCore(this MetricsConfig config, Action<Func<HttpContext, Func<Task>, Task>> middlewareRegistration, Action<AspNetCoreMetricsConfig> aspNetCoreConfig)
        {
            var aspNetCore = config.WithConfigExtension((ctx, hs) => 
                new AspNetCoreMetricsConfig(middlewareRegistration, ctx, hs), () => AspNetCoreMetricsConfig.Disabled);
            aspNetCoreConfig(aspNetCore);
            return config;
        }
    }

}