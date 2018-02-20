using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.Metrics;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using AspNetCore.Metrics;
using Xunit;

namespace Metrics.Tests.AspNetCoreAdapter
{
    public class AspNetCoreMiddlewareTests
    {
        private const int timePerRequest = 100;

        private readonly TestContext context = new TestContext();
        private readonly MetricsConfig config;
        private readonly TestServer server;

        public AspNetCoreMiddlewareTests()
        {
            this.config = new MetricsConfig(this.context);

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    this.config.WithAspNetCore(m => app.Use(m));

                    app.Run(ctx =>
                    {
                        this.context.Clock.Advance(TimeUnit.Milliseconds, timePerRequest);
                        if (ctx.Request.Path.ToString() == "/test/action")
                        {
                            return ctx.Response.WriteAsync("response");
                        }

                        if (ctx.Request.Path.ToString() == "/test/error")
                        {
                            ctx.Response.StatusCode = 500;
                            return ctx.Response.WriteAsync("response");
                        }

                        if (ctx.Request.Path.ToString() == "/test/size")
                        {
                            return ctx.Response.WriteAsync("response");
                        }

                        if (ctx.Request.Path.ToString() == "/test/post")
                        {
                            return ctx.Response.WriteAsync("response");
                        }

                        ctx.Response.StatusCode = 404;
                        return ctx.Response.WriteAsync("not found");
                    });

                });
            
            this.server = new TestServer(builder);
        }


        [Fact]
        public async Task AspNetCoreMetrics_ShouldBeAbleToRecordErrors()
        {
            this.context.MeterValue("AspNetCore", "Errors").Count.Should().Be(0);
            (await this.server.CreateRequest("http://local.test/test/error").GetAsync()).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            this.context.MeterValue("AspNetCore", "Errors").Count.Should().Be(1);

            (await this.server.CreateRequest("http://local.test/test/error").GetAsync()).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            this.context.MeterValue("AspNetCore", "Errors").Count.Should().Be(2);
        }

        [Fact]
        public async Task AspNetCoreMetrics_ShouldBeAbleToRecordActiveRequestCounts()
        {
            this.context.TimerValue("AspNetCore", "Requests").Rate.Count.Should().Be(0);
            (await this.server.CreateRequest("http://local.test/test/action").GetAsync()).StatusCode.Should().Be(HttpStatusCode.OK);
            this.context.TimerValue("AspNetCore", "Requests").Rate.Count.Should().Be(1);
            (await this.server.CreateRequest("http://local.test/test/action").GetAsync()).StatusCode.Should().Be(HttpStatusCode.OK);
            this.context.TimerValue("AspNetCore", "Requests").Rate.Count.Should().Be(2);
            (await this.server.CreateRequest("http://local.test/test/action").GetAsync()).StatusCode.Should().Be(HttpStatusCode.OK);
            this.context.TimerValue("AspNetCore", "Requests").Rate.Count.Should().Be(3);
            (await this.server.CreateRequest("http://local.test/test/action").GetAsync()).StatusCode.Should().Be(HttpStatusCode.OK);
            this.context.TimerValue("AspNetCore", "Requests").Rate.Count.Should().Be(4);

            var timer = this.context.TimerValue("AspNetCore", "Requests");

            timer.Histogram.Min.Should().Be(timePerRequest);
            timer.Histogram.Max.Should().Be(timePerRequest);
            timer.Histogram.Mean.Should().Be(timePerRequest);
        }

        [Fact]
        public async Task AspNetCoreMetrics_ShouldRecordHistogramMetricsForPostSizeAndTimePerRequest()
        {
            const string json = "{ 'id': '1'} ";
            var postContent = new StringContent(json);
            postContent.Headers.Add("Content-Length", json.Length.ToString());
            await this.server.CreateRequest("http://local.test/test/post").And(r => r.Content = postContent).PostAsync();

            var histogram = this.context.HistogramValue("AspNetCore", "Post & Put Request Size");

            histogram.Count.Should().Be(1);
            histogram.LastValue.Should().Be(json.Length);
        }
    }
}
