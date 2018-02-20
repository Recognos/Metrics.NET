using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Metrics.Endpoints;
using Metrics.Reports;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics.Middleware
{
    public class AspNetCoreMetricsEndpointMiddleware
    {
        private readonly string endpointPrefix;
        private readonly AspNetCoreMetricsEndpointHandler endpointHandler;

        public AspNetCoreMetricsEndpointMiddleware(string endpointPrefix, MetricsEndpointReports endpointConfig)
        {
            this.endpointPrefix = NormalizePrefix(endpointPrefix);
            this.endpointHandler = new AspNetCoreMetricsEndpointHandler(endpointConfig.Endpoints);
        }

        public Task Invoke(HttpContext context, Func<Task> next)
        {
            var requestPath = context.Request.Path.ToString();
            if (requestPath.StartsWith(this.endpointPrefix, StringComparison.OrdinalIgnoreCase))
            {
                requestPath = requestPath.Substring(this.endpointPrefix.Length);

                if (requestPath == "/")
                {
                    return GetFlotWebApp(context);
                }

                var response = this.endpointHandler.Process(requestPath, context);
                if (response != null)
                {
                    return WriteResponse(response, context);
                }
            }

            return next();
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

        private static Task GetFlotWebApp(HttpContext context)
        {
            var content = FlotWebApp.GetFlotApp();
            return WriteResponse(context, content, "text/html");
        }

        private static async Task WriteResponse(HttpContext context, string content, string contentType, HttpStatusCode code = HttpStatusCode.OK)
        {
            var response = context.Response.Body;
            var headers = context.Response.Headers;

            var contentBytes = Encoding.UTF8.GetBytes(content);

            headers["Content-Type"] = new[] { contentType };
            headers["Cache-Control"] = new[] { "no-cache, no-store, must-revalidate" };
            headers["Pragma"] = new[] { "no-cache" };
            headers["Expires"] = new[] { "0" };

            context.Response.StatusCode = (int)code;

            await response.WriteAsync(contentBytes, 0, contentBytes.Length);
        }

        private static async Task WriteResponse(MetricsEndpointResponse response, HttpContext context)
        {
            var responseStream = context.Response.Body;
            var headers = context.Response.Headers;

            var contentBytes = response.Encoding.GetBytes(response.Content);

            headers["Content-Type"] = new[] { response.ContentType };
            headers["Cache-Control"] = new[] { "no-cache, no-store, must-revalidate" };
            headers["Pragma"] = new[] { "no-cache" };
            headers["Expires"] = new[] { "0" };

            context.Response.StatusCode = (int)response.StatusCode;

            await responseStream.WriteAsync(contentBytes, 0, contentBytes.Length);
        }
    }
}
