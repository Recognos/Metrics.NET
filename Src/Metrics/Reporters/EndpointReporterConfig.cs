using System;
using System.Linq;
using System.Text;
using Metrics.Endpoints;
using Metrics.Json;
using Metrics.MetricData;
using Metrics.Reports;

namespace Metrics.Reporters
{
    public static class EndpointReporterConfig
    {
        public static MetricsEndpointReports WithTextReport(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, (d, h, c) => new MetricsEndpointResponse(StringReport.RenderMetrics(d, h), "text/plain"));
        }

        #region HealthChecks

        public static MetricsEndpointReports WithJsonHealthV1Report(this MetricsEndpointReports reports, string endpoint, bool alwaysReturnOkStatusCode = false)
        {
            return reports.WithEndpointReport(endpoint, (d, h, r) => GetHealthResponse(h, JsonHealthChecksV1.BuildJson, JsonHealthChecksV1.HealthChecksMimeType, alwaysReturnOkStatusCode));
        }

        public static MetricsEndpointReports WithJsonHealthV2Report(this MetricsEndpointReports reports, string endpoint, bool alwaysReturnOkStatusCode = false)
        {
            return reports.WithEndpointReport(endpoint, (d, h, r) => GetHealthResponse(h, JsonHealthChecksV2.BuildJson, JsonHealthChecksV2.HealthChecksMimeType, alwaysReturnOkStatusCode));
        }

        public static MetricsEndpointReports WithJsonHealthReport(this MetricsEndpointReports reports, string endpoint, bool alwaysReturnOkStatusCode = false)
        {
            return reports.WithEndpointReport(endpoint, (d, h, r) => GetHealthResponse(h, GetJsonHealthCreator(r), GetJsonHealthMimeType(r), alwaysReturnOkStatusCode));
        }

        private static MetricsEndpointResponse GetHealthResponse(Func<HealthStatus> healthStatus, Func<HealthStatus, string> jsonCreator, string healthMimeType, bool alwaysReturnOkStatusCode)
        {
            var status = healthStatus();
            var json = jsonCreator(status);

            var httpStatus = status.IsHealthy || alwaysReturnOkStatusCode ? 200 : 500;
            var httpStatusDescription = status.IsHealthy || alwaysReturnOkStatusCode ? "OK" : "Internal Server Error";

            return new MetricsEndpointResponse(json, healthMimeType, Encoding.UTF8, httpStatus, httpStatusDescription);
        }

        private static Func<HealthStatus, string> GetJsonHealthCreator(MetricsEndpointRequest request)
        {
            return IsJsonHealthV2(request)
                ? (Func<HealthStatus, string>)JsonHealthChecksV2.BuildJson
                : JsonHealthChecksV1.BuildJson;
        }

        private static string GetJsonHealthMimeType(MetricsEndpointRequest request)
        {
            return IsJsonHealthV2(request)
                ? JsonHealthChecksV2.HealthChecksMimeType
                : JsonHealthChecksV1.HealthChecksMimeType;
        }

        private static bool IsJsonHealthV2(MetricsEndpointRequest request)
        {
            string[] acceptHeader;
            if (request.Headers.TryGetValue("Accept", out acceptHeader))
            {
                return acceptHeader.Contains(JsonHealthChecksV2.HealthChecksMimeType);
            }
            return false;
        }

        #endregion HealthChecks

        #region MetricsData

        public static MetricsEndpointReports WithJsonV1Report(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, GetJsonV1Response);
        }

        private static MetricsEndpointResponse GetJsonV1Response(MetricsData data, Func<HealthStatus> healthStatus, MetricsEndpointRequest request)
        {
            var json = JsonBuilderV1.BuildJson(data);
            return new MetricsEndpointResponse(json, JsonBuilderV1.MetricsMimeType);
        }

        public static MetricsEndpointReports WithJsonV2Report(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, GetJsonV2Response);
        }

        private static MetricsEndpointResponse GetJsonV2Response(MetricsData data, Func<HealthStatus> healthStatus, MetricsEndpointRequest request)
        {
            var json = JsonBuilderV2.BuildJson(data);
            return new MetricsEndpointResponse(json, JsonBuilderV2.MetricsMimeType);
        }

        public static MetricsEndpointReports WithJsonReport(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, GetJsonResponse);
        }

        private static MetricsEndpointResponse GetJsonResponse(MetricsData data, Func<HealthStatus> healthStatus, MetricsEndpointRequest request)
        {
            string[] acceptHeader;
            if (request.Headers.TryGetValue("Accept", out acceptHeader))
            {
                return acceptHeader.Contains(JsonBuilderV2.MetricsMimeType)
                    ? GetJsonV2Response(data, healthStatus, request)
                    : GetJsonV1Response(data, healthStatus, request);
            }

            return GetJsonV1Response(data, healthStatus, request);
        }

        #endregion MetricsData

        public static MetricsEndpointReports WithPing(this MetricsEndpointReports reports)
        {
            return reports.WithEndpointReport("/ping", (d, h, r) => new MetricsEndpointResponse("pong", "text/plain"));
        }
    }
}
