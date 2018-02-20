using System;
using System.Text;
using FluentAssertions;
using Metrics.Endpoints;
using Xunit;

namespace Metrics.Tests.Endpoints
{
    public class MetricsEndpointTests
    {
        [Fact]
        public void MetricsEndpoint_CannotCreateWithEmptyPath()
        {
            var action1 = new Action(() => new MetricsEndpoint("", c => new MetricsEndpointResponse("test", "text/plain")));
            var action2 = new Action(() => new MetricsEndpoint(" ", c => new MetricsEndpointResponse("test", "text/plain")));
            var action3 = new Action(() => new MetricsEndpoint("/", c => new MetricsEndpointResponse("test", "text/plain")));

            action1.Should().Throw<ArgumentException>();
            action2.Should().Throw<ArgumentException>();
            action3.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void MetricsEndpoint_CanNormalizePath()
        {
            var endpoint1 = new MetricsEndpoint("test", c => new MetricsEndpointResponse("test", "text/plain"));
            endpoint1.Endpoint.Should().Be("test");

            var endpoint2 = new MetricsEndpoint("/test", c => new MetricsEndpointResponse("test", "text/plain"));
            endpoint1.Endpoint.Should().Be("test");
        }

        [Fact]
        public void MetricsEndpoint_CanProduceResponse()
        {
            Func<MetricsEndpointRequest, MetricsEndpointResponse> factory = r => new MetricsEndpointResponse("custom content", "application/custom", Encoding.ASCII, 202, "Accepted");
            var endpoint = new MetricsEndpoint("test", factory);

            var response = endpoint.ProduceResponse(null);
            response.Content.Should().Be("custom content");
            response.ContentType.Should().Be("application/custom");
            response.Encoding.Should().Be(Encoding.ASCII);
            response.StatusCode.Should().Be(202);
            response.StatusCodeDescription.Should().Be("Accepted");
        }

        [Fact]
        public void MetricsEndpoint_MatchesCorrectly()
        {
            var endpoint = new MetricsEndpoint("test", c => new MetricsEndpointResponse("test", "text/plain"));
            endpoint.IsMatch("test").Should().BeTrue();
            endpoint.IsMatch("/test").Should().BeTrue();
            endpoint.IsMatch("TEST").Should().BeTrue();
            endpoint.IsMatch("/TEST").Should().BeTrue();
            endpoint.IsMatch("text").Should().BeFalse();
        }
    }
}
