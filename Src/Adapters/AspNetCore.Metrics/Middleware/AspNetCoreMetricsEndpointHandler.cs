using System.Collections.Generic;
using System.Linq;
using Metrics.Endpoints;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics.Middleware
{
    internal class AspNetCoreMetricsEndpointHandler : AbstractMetricsEndpointHandler<HttpContext>
    {
        public AspNetCoreMetricsEndpointHandler(IEnumerable<MetricsEndpoint> endpoints) : base(endpoints) { }

        protected override MetricsEndpointRequest CreateRequest(HttpContext context)
        {
            IDictionary<string, string[]> header = context.Request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
            return new MetricsEndpointRequest(header);
        }
    }
}
