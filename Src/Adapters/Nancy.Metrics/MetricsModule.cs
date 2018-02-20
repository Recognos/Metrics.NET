using System;
using System.IO;
using System.Linq;
using Metrics.Endpoints;
using Metrics.Reports;

namespace Nancy.Metrics
{
    public class MetricsModule : NancyModule
    {
        private struct ModuleConfig
        {
            public readonly string ModulePath;
            public readonly Action<INancyModule> ModuleConfigAction;
            public readonly NancyMetricsEndpointHandler Handler;

            public ModuleConfig(Action<INancyModule> moduleConfig, NancyMetricsEndpointHandler handler, string metricsPath)
            {
                this.ModuleConfigAction = moduleConfig;
                this.ModulePath = metricsPath;
                this.Handler = handler;
            }
        }

        private static ModuleConfig Config;

        private static readonly object[] noCacheHeaders = {
            new { Header = "Cache-Control", Value = "no-cache, no-store, must-revalidate" },
            new { Header = "Pragma", Value = "no-cache" },
            new { Header = "Expires", Value = "0" }
        };

        internal static void Configure(Action<INancyModule> moduleConfig, MetricsEndpointReports reports, string metricsPath)
        {
            var handler = new NancyMetricsEndpointHandler(reports.Endpoints);
            MetricsModule.Config = new ModuleConfig(moduleConfig, handler, metricsPath);
        }

        public MetricsModule()
            : base(Config.ModulePath ?? "/")
        {
            if (string.IsNullOrEmpty(Config.ModulePath))
            {
                return;
            }

            Config.ModuleConfigAction?.Invoke(this);

            Get<Response>("/", _ =>
            {
                if (!this.Request.Url.Path.EndsWith("/"))
                {
                    return Response.AsRedirect(this.Request.Url.ToString() + "/");
                }
                var gzip = AcceptsGzip();
                var response = Response.FromStream(FlotWebApp.GetAppStream(!gzip), "text/html");
                if (gzip)
                {
                    response.WithHeader("Content-Encoding", "gzip");
                }
                return response;
            });

            Get<Response>("/{path*}", p =>
            {
                var path = (string)p.path;
                var endpointResponse = Config.Handler.Process(path, this.Request);
                return endpointResponse != null ? GetResponse(endpointResponse) : HttpStatusCode.NotFound;
            });
        }

        private static Response GetResponse(MetricsEndpointResponse endpointResponse)
        {
            var response = new Response();
            response.StatusCode = (HttpStatusCode)endpointResponse.StatusCode;
            response.ContentType = endpointResponse.ContentType;
            response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream, endpointResponse.Encoding))
                {
                    writer.Write(endpointResponse.Content);
                }
            };
            return response.WithHeaders(noCacheHeaders);
        }

        private bool AcceptsGzip()
        {
            return this.Request.Headers.AcceptEncoding.Any(e => e.Equals("gzip", StringComparison.OrdinalIgnoreCase));
        }
    }
}
