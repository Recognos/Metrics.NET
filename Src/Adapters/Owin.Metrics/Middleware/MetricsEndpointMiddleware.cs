using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Metrics.Endpoints;
using Metrics.Reports;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class MetricsEndpointMiddleware
    {
        private readonly string endpointPrefix;
        private readonly OwinMetricsEndpointHandler endpointHandler;
        private AppFunc next;

        public MetricsEndpointMiddleware(AppFunc next, string endpointPrefix, MetricsEndpointReports endpointConfig)
            : this(endpointPrefix, endpointConfig)
        {
            this.next = next;
        }
        
        public MetricsEndpointMiddleware(string endpointPrefix, MetricsEndpointReports endpointConfig)
        {
            this.endpointPrefix = NormalizePrefix(endpointPrefix);
            this.endpointHandler = new OwinMetricsEndpointHandler(endpointConfig.Endpoints);
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var requestPath = environment["owin.RequestPath"] as string;
            if (requestPath.StartsWith(this.endpointPrefix, StringComparison.OrdinalIgnoreCase))
            {
                requestPath = requestPath.Substring(this.endpointPrefix.Length);

                if (requestPath == "/")
                {
                    return GetFlotWebApp(environment);
                }

                var response = this.endpointHandler.Process(requestPath, environment);
                if (response != null)
                {
                    return WriteResponse(response, environment);
                }
            }

            return this.next(environment);
        }

        private static string NormalizePrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix) || prefix == "/")
            {
                return string.Empty;
            }

            if (prefix.StartsWith("/"))
            {
                return prefix;
            }

            return $"/{prefix}";
        }

        private static Task GetFlotWebApp(IDictionary<string, object> environment)
        {
            var content = FlotWebApp.GetFlotApp();
            return WriteResponse(environment, content, "text/html");
        }

        private static async Task WriteResponse(IDictionary<string, object> environment, string content, string contentType, HttpStatusCode code = HttpStatusCode.OK)
        {
            var response = environment["owin.ResponseBody"] as Stream;
            var headers = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;

            var contentBytes = Encoding.UTF8.GetBytes(content);

            headers["Content-Type"] = new[] { contentType };
            headers["Cache-Control"] = new[] { "no-cache, no-store, must-revalidate" };
            headers["Pragma"] = new[] { "no-cache" };
            headers["Expires"] = new[] { "0" };

            environment["owin.ResponseStatusCode"] = (int)code;

            await response.WriteAsync(contentBytes, 0, contentBytes.Length);
        }

        private static async Task WriteResponse(MetricsEndpointResponse response, IDictionary<string, object> environment)
        {
            var responseStream = environment["owin.ResponseBody"] as Stream;
            var headers = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;

            var contentBytes = response.Encoding.GetBytes(response.Content);

            headers["Content-Type"] = new[] { response.ContentType };
            headers["Cache-Control"] = new[] { "no-cache, no-store, must-revalidate" };
            headers["Pragma"] = new[] { "no-cache" };
            headers["Expires"] = new[] { "0" };

            environment["owin.ResponseStatusCode"] = (int)response.StatusCode;

            await responseStream.WriteAsync(contentBytes, 0, contentBytes.Length);
        }
    }
}
