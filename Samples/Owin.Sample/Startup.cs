using System;
using System.Text.RegularExpressions;
using Metrics;
using Metrics.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin.Metrics;
using Formatting = Newtonsoft.Json.Formatting;

namespace Owin.Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }


        public void Configure(IApplicationBuilder app)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            app.UseCors(c => c.AllowAnyOrigin());
            
            Metric.Config
                //.WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(30)))
                .WithOwin(
                    middleware => app.UseOwin(x => x(middleware)), 
                    config => config
                        .WithRequestMetricsConfig(c => c.WithAllOwinMetrics(), new[]
                        {
                            new Regex("(?i)^sampleignore"),
                            new Regex("(?i)^metrics"),
                            new Regex("(?i)^health"),
                            new Regex("(?i)^json")
                         })
                        .WithMetricsEndpoint(conf => conf
                            .WithEndpointReport("/test", (d, h, r) => new MetricsEndpointResponse("test", "text/plain")))
                );

            app.UseMvc();
        }

    }
}
