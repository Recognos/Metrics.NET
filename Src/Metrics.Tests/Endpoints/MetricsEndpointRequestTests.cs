using System;
using FluentAssertions;
using Metrics.Endpoints;
using Xunit;

namespace Metrics.Tests.Endpoints
{
    public class MetricsEndpointRequestTests
    {
        [Fact]
        public void MetricsEndpointRequest_CannotCreateWithNullHeaders()
        {
            var action = new Action(() => new MetricsEndpointRequest(null));
            action.Should().Throw<Exception>();
        }
    }
}
