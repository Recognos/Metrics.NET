using System;
using System.Text;
using FluentAssertions;
using Metrics.Endpoints;
using Xunit;

namespace Metrics.Tests.Endpoints
{
    public class MetricsEndpointResponseTests
    {
        [Fact]
        public void MetricsEndpointResponse_HasSaneDefaults()
        {
            var response = new MetricsEndpointResponse("content", "content-type");
            response.Content.Should().Be("content");
            response.ContentType.Should().Be("content-type");
            response.Encoding.Should().Be(Encoding.UTF8);
            response.StatusCode.Should().Be(200);
            response.StatusCodeDescription.Should().Be("OK");

            response = new MetricsEndpointResponse("content", "content-type", Encoding.ASCII);
            response.Content.Should().Be("content");
            response.ContentType.Should().Be("content-type");
            response.Encoding.Should().Be(Encoding.ASCII);
            response.StatusCode.Should().Be(200);
            response.StatusCodeDescription.Should().Be("OK");
        }

        [Fact]
        public void MetricsEndpointResponse_CannotCreateWithoutContent()
        {
            var action = new Action(() => new MetricsEndpointResponse(null, "content-type", Encoding.ASCII));
            action.Should().Throw<Exception>();
        }

        [Fact]
        public void MetricsEndpointResponse_CannotCreateWithInvalidContentType()
        {
            var action1 = new Action(() => new MetricsEndpointResponse("content", null, Encoding.ASCII));
            var action2 = new Action(() => new MetricsEndpointResponse("content", string.Empty, Encoding.ASCII));
            var action3 = new Action(() => new MetricsEndpointResponse("content", " ", Encoding.ASCII));
            action1.Should().Throw<Exception>();
            action2.Should().Throw<Exception>();
            action3.Should().Throw<Exception>();
        }

        [Fact]
        public void MetricsEndpointResponse_CannotCreateWithoutEncoding()
        {
            var action = new Action(() => new MetricsEndpointResponse("content", "content-type", null));
            action.Should().Throw<Exception>();
        }

        [Fact]
        public void MetricsEndpointResponse_CannotCreateWithoutStatusCodeDescription()
        {
            var action = new Action(() => new MetricsEndpointResponse("content", "content-type", Encoding.ASCII, 200, null));
            action.Should().Throw<Exception>();
        }
    }
}
