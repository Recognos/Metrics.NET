using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Metrics.Core;
using Metrics.Utils;

namespace Metrics.Json
{
    public class JsonHealthChecksV2
    {
        public const int Version = 2;
        public const string HealthChecksMimeType = "application/vnd.metrics.net.v2.health+json";

        private readonly List<JsonProperty> root = new List<JsonProperty>();

        public static string BuildJson(HealthStatus status) { return BuildJson(status, Clock.Default, indented: false); }
        public static string BuildJson(HealthStatus status, Clock clock, bool indented = true)
        {
            return new JsonHealthChecksV2()
               .AddVersion(Version)
               .AddTimestamp(Clock.Default)
               .AddObject(status)
               .GetJson(indented);
        }

        public JsonHealthChecksV2 AddVersion(int version)
        {
            root.Add(new JsonProperty("Version", version.ToString(CultureInfo.InvariantCulture)));
            return this;
        }

        public JsonHealthChecksV2 AddTimestamp(Clock clock)
        {
            root.Add(new JsonProperty("Timestamp", Clock.FormatTimestamp(clock.UTCDateTime)));
            return this;
        }

        public JsonHealthChecksV2 AddObject(HealthStatus status)
        {
            var properties = new List<JsonProperty>() { new JsonProperty("IsHealthy", status.IsHealthy) };
            var unhealty = status.Results.Where(r => !r.Check.IsHealthy);
            properties.Add(new JsonProperty("Unhealthy", CreateHealthJsonObject(unhealty)));
            var healthy = status.Results.Where(r => r.Check.IsHealthy);
            properties.Add(new JsonProperty("Healthy", CreateHealthJsonObject(healthy)));
            this.root.AddRange(properties);
            return this;
        }

        private IEnumerable<JsonObject> CreateHealthJsonObject(IEnumerable<HealthCheck.Result> results)
        {
            return results.Select(r => new JsonObject(new List<JsonProperty>()
            {
                new JsonProperty("Name", r.Name),
                new JsonProperty("Message", r.Check.Message),
                new JsonProperty("Tags", r.Tags.Tags)
            }));
        }

        public string GetJson(bool indented = true)
        {
            return new JsonObject(root).AsJson(indented);
        }
    }
}
