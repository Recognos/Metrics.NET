using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.Endpoints;
using Xunit;

namespace Metrics.Tests.Endpoints
{
    public class MetricsHttpListenerTests
    {
        private static string Endpoint(string test)
        {
            return "http://localhost:58888/metricstest/" + test + "/";
        }
        private static Task<MetricsHttpListener> StartListener(string endpoint)
        {
            var context = new TestContext();
            return MetricsHttpListener.StartHttpListenerAsync(Endpoint(endpoint), Enumerable.Empty<MetricsEndpoint>(), CancellationToken.None);
        }

        [Fact]
        public async Task MetricsHttpListener_CanBeDisposed()
        {
            var listener = await StartListener("HttpEndpointCanBeDisposed");
            listener.Dispose();
        }

        [Fact]
        public async Task MetricsHttpListener_CanBeDoubleDisposed()
        {
            var listener = await StartListener("HttpEndpointCanBeDisposed");
            listener.Dispose();
            listener.Dispose();
        }

        [Fact]
        public async Task MetricsHttpListener_DoesNotThrowIfPortIsOccupied()
        {
            const string endpoint = "OccupiedPort";
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(Endpoint(endpoint));
                listener.Start();

                var result = await StartListener(endpoint);
                result.Should().BeNull();
            }
        }

        [Fact]
        public async Task MetricsHttpListener_LogsAnErrorIfPortIsOccupied()
        {
            const string endpoint = "OccupiedPort";
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(Endpoint(endpoint));
                listener.Start();

                var loggedAnError = false;

                using (var config = CreateConfig())
                {
                    config.WithErrorHandler((exception, s) => { loggedAnError = true; }, true);
                    config.WithErrorHandler((exception) => { loggedAnError = true; }, true);

                    await StartListener(endpoint);
                }

                Assert.True(loggedAnError);
                listener.Close();
            }
        }


        [Fact]
        public void MetricsHttpListener_MetricsConfig_SecondCallToWithHttpEndpointThrows()
        {
            using (var config = CreateConfig().WithHttpEndpoint("http://localhost:58888/metricstest/HttpListenerTests/sameendpoint/"))
            {
                var action = new Action(() => config.WithHttpEndpoint("http://localhost:58888/metricstest/HttpListenerTests/sameendpoint/"));
                action.Should().Throw<Exception>();
            }
        }

        [Fact]
        public async Task MetricsHttpListener_MetricsConfig_WaitForEndpointInitialization()
        {
            using (var config = CreateConfig().WithHttpEndpoint("http://localhost:58888/metricstest/HttpListenerTests/sameendpoint/"))
            {
                var whenInitialized = config.WhenEndpointInitialized();

                whenInitialized.IsCompleted.ShouldBeEquivalentTo(false);

                await Task.WhenAny(whenInitialized, Task.Delay(5000));

                whenInitialized.IsCompleted.ShouldBeEquivalentTo(true);
            }
        }

        private static MetricsConfig CreateConfig()
        {
            var context = new DefaultMetricsContext("http-listener-tests");
            return new MetricsConfig(context);
        }
    }
}
