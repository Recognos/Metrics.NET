using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests.Utils
{
    public class ActionSchedulerTests
    {
        [Fact]
        public void ActionScheduler_ExecutesScheduledFunction()
        {
            using (ActionScheduler scheduler = new ActionScheduler())
            {
                var tcs = new TaskCompletionSource<bool>();
                int data = 0;

                Func<CancellationToken, Task> function = (t) => Task.Factory.StartNew(() =>
                    {
                        data++;
                        tcs.SetResult(true);
                    });

                scheduler.Start(TimeSpan.FromMilliseconds(10), function);
                tcs.Task.Wait();
                scheduler.Stop();

                data.Should().Be(1);
            }
        }

        [Fact]
        public void ActionScheduler_ExecutesScheduledAction()
        {
            using (ActionScheduler scheduler = new ActionScheduler())
            {
                var tcs = new TaskCompletionSource<bool>();
                int data = 0;

                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    data++;
                    tcs.SetResult(true);
                });

                tcs.Task.Wait();
                scheduler.Stop();

                data.Should().Be(1);
            }
        }

        [Fact]
        public void ActionScheduler_ExecutesScheduledActionWithToken()
        {
            using (ActionScheduler scheduler = new ActionScheduler())
            {
                int data = 0;
                var tcs = new TaskCompletionSource<bool>();

                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    data++;
                    tcs.SetResult(true);
                });

                tcs.Task.Wait();
                scheduler.Stop();
                data.Should().Be(1);
            }
        }

        [Fact]
        public void ActionScheduler_ExecutesScheduledActionMultipleTimes()
        {
            using (ActionScheduler scheduler = new ActionScheduler())
            {
                int data = 0;
                var tcs = new TaskCompletionSource<bool>();

                scheduler.Start(TimeSpan.FromMilliseconds(10), () =>
                {
                    data++;
                    tcs.SetResult(true);
                });

                tcs.Task.Wait();
                data.Should().Be(1);

                tcs = new TaskCompletionSource<bool>();
                tcs.Task.Wait();
                data.Should().Be(2);

                scheduler.Stop();
            }
        }

        [Fact]
        public void ActionScheduler_ReportsExceptionWithGlobalMetricHandler()
        {
            Exception x = null;
            var tcs = new TaskCompletionSource<bool>();

            Metric.Config.WithErrorHandler(e =>
            {
                x = e;
                tcs.SetResult(true);
            });

            using (ActionScheduler scheduler = new ActionScheduler())
            {
                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    throw new InvalidOperationException("boom");
                });

                tcs.Task.Wait(1000);
                scheduler.Stop();
            }

            x.Should().NotBeNull();
        }

        [Fact]
        public void ActionScheduler_CannotCreateWithInvalidParameter()
        {
            var action = new Action(() => new ActionScheduler(-2));
            action.Should().Throw<Exception>();
        }

        [Fact]
        public void ActionScheduler_DefaultDoesNotTolerateFailures()
        {
            using (var scheduler = new ActionScheduler())
            {
                Test_NoTolerance(scheduler);
            }
        }

        [Fact]
        public void ActionScheduler_DoesNotTolerateFailures()
        {
            using (var scheduler = new ActionScheduler(0))
            {
                Test_NoTolerance(scheduler);
            }
        }

        private static void Test_NoTolerance(ActionScheduler scheduler)
        {
            var actionCounter = 0;

            scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
            {
                actionCounter++;
                throw new Exception();
            });

            Thread.Sleep(200);
            scheduler.Stop();

            actionCounter.Should().Be(1);
        }

        [Fact]
        public void ActionScheduler_ToleratesFailures()
        {
            using (var scheduler = new ActionScheduler(3))
            {
                var actionCounter = 0;
                var tcs = new TaskCompletionSource<bool>();

                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    if (actionCounter < 3)
                    {
                        actionCounter++;
                        throw new Exception();
                    }
                    else
                    {
                        actionCounter++;
                        tcs.SetResult(true);
                    }
                });

                tcs.Task.Wait();
                scheduler.Stop();

                actionCounter.Should().Be(4);
            }
        }

        [Fact]
        public void ActionScheduler_ReportsErrorInCaseOfToleratedFailures()
        {
            using (var scheduler = new ActionScheduler(3))
            {
                var actionCounter = 0;
                var tcs = new TaskCompletionSource<bool>();
                var errorCounter = 0;

                Metric.Config.WithErrorHandler(e =>
                {
                    if (e.Message == "reports_error")
                    {
                        errorCounter++;
                    }
                });

                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    if (actionCounter < 3)
                    {
                        actionCounter++;
                        throw new Exception("reports_error");
                    }
                    else
                    {
                        actionCounter++;
                        tcs.SetResult(true);
                    }
                });

                tcs.Task.Wait();
                scheduler.Stop();

                actionCounter.Should().Be(4);
                errorCounter.Should().Be(3);
            }
        }

        [Fact]
        public void ActionScheduler_ToleratesUnlimitedFailures()
        {
            using (var scheduler = new ActionScheduler(-1))
            {
                var actionCounter = 0;
                var tcs = new TaskCompletionSource<bool>();

                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    if (actionCounter < 10)
                    {
                        actionCounter++;
                        throw new Exception();
                    }
                    else
                    {
                        actionCounter++;
                        tcs.SetResult(true);
                    }
                });

                tcs.Task.Wait();
                scheduler.Stop();

                actionCounter.Should().Be(11);
            }
        }
    }
}
