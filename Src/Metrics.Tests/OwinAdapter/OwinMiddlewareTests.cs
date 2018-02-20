using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Owin.Metrics;
using Xunit;

namespace Metrics.Tests.OwinAdapter
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    using OwinPipeline = Action<Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>>;


public class OwinMiddlewareTests
    {
        
        
        private const int timePerRequest = 100;

        private readonly TestContext context = new TestContext();
        private readonly MetricsConfig config;
        private readonly TestServer server;

        private AppFunc DoNothing(AppFunc f)
        {
            return f;
        }

        void RunPipeline(OwinPipeline pipeline)
        {
            pipeline(DoNothing);
        }
        
        public OwinMiddlewareTests()
        {
            this.config = new MetricsConfig(this.context);

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    this.config.WithOwin(m => app.UseOwin(cfg => cfg(m)));

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
        public async Task OwinMetrics_ShouldBeAbleToRecordErrors()
        {
            context.MeterValue("Owin", "Errors").Count.Should().Be(0);
            (await server.CreateRequest("http://local.test/test/error").GetAsync()).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            context.MeterValue("Owin", "Errors").Count.Should().Be(1);

            (await server.CreateRequest("http://local.test/test/error").GetAsync()).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            context.MeterValue("Owin", "Errors").Count.Should().Be(2);
        }

        [Fact]
        public async Task OwinMetrics_ShouldBeAbleToRecordActiveRequestCounts()
        {
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(0);
            (await server.CreateRequest("http://local.test/test/action").GetAsync()).StatusCode.Should().Be(HttpStatusCode.OK);
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(1);
            (await server.CreateRequest("http://local.test/test/action").GetAsync()).StatusCode.Should().Be(HttpStatusCode.OK);
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(2);
            (await server.CreateRequest("http://local.test/test/action").GetAsync()).StatusCode.Should().Be(HttpStatusCode.OK);
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(3);
            (await server.CreateRequest("http://local.test/test/action").GetAsync()).StatusCode.Should().Be(HttpStatusCode.OK);
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(4);

            var timer = context.TimerValue("Owin", "Requests");

            timer.Histogram.Min.Should().Be(timePerRequest);
            timer.Histogram.Max.Should().Be(timePerRequest);
            timer.Histogram.Mean.Should().Be(timePerRequest);
        }

        [Fact]
        public async Task OwinMetrics_ShouldRecordHistogramMetricsForPostSizeAndTimePerRequest()
        {
            const string json = "{ 'id': '1'} ";
            var postContent = new StringContent(json);
            postContent.Headers.Add("Content-Length", json.Length.ToString());
            await server.CreateRequest("http://local.test/test/post").And(r => r.Content = postContent).PostAsync();

            var histogram = context.HistogramValue("Owin", "Post & Put Request Size");

            histogram.Count.Should().Be(1);
            histogram.LastValue.Should().Be(json.Length);
        }
    }
}
