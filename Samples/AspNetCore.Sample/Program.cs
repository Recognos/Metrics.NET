using System;
using Metrics.Samples;
using Metrics.Utils;
using Microsoft.AspNetCore.Hosting;

namespace AspNetCore.Sample
{
    public class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:32132/";

            using (var scheduler = new ActionScheduler())
            {
                using (var webHost = new WebHostBuilder()
                    .UseKestrel()
                    .UseStartup<Startup>()
                    .UseUrls(url)
                    .Build())
                {
                    Console.WriteLine("AspNetCore Running at {0}", url);
                    Console.WriteLine("Press any key to exit");
                    //Process.Start(string.Format("{0}metrics/", url));

                    SampleMetrics.RunSomeRequests();

                    scheduler.Start(TimeSpan.FromMilliseconds(500), () =>
                    {
                        SetCounterSample.RunSomeRequests();
                        SetMeterSample.RunSomeRequests();
                        UserValueHistogramSample.RunSomeRequests();
                        UserValueTimerSample.RunSomeRequests();
                        SampleMetrics.RunSomeRequests();
                    });

                    HealthChecksSample.RegisterHealthChecks();

                    webHost.Run();
                    
                }
            }
        }
    }
}
