using System;
using FluentAssertions;
using Metrics.Endpoints;
using Xunit;

namespace Metrics.Tests.Endpoints
{
    public class FlotVisualizationTests
    {
        [Fact]
        public void FlotVisualization_CanReadAppFromResource()
        {
            var html = FlotWebApp.GetFlotApp();
            html.Should().NotBeEmpty();
        }
    }
}
