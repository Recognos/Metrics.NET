using System.Text.RegularExpressions;
using AspNetCore.Metrics;
using Metrics;
using Metrics.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace AspNetCore.Sample
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
                .WithAspNetCore(
                    middleware => app.Use(middleware), 
                    config => config
                        .WithRequestMetricsConfig(c => c.WithAllAspNetCoreMetrics(), new[]
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
