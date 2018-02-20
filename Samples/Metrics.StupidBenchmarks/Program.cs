using System;
using CommandLine;
using CommandLine.Text;
using Metrics.Core;
using Metrics.Sampling;
using Metrics.Utils;
namespace Metrics.StupidBenchmarks
{
    class CommonOptions
    {
        [Value(0)]
        public string Target { get; set; }
        
        [Option('c', HelpText = "Max Threads", Default = 32)]
        public int MaxThreads { get; set; }

        [Option('s', HelpText = "Seconds", Default = 5)]
        public int Seconds { get; set; }

        [Option('d', HelpText = "Number of threads to decrement each step", Default = 4)]
        public int Decrement { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CommonOptions>(args)
                .MapResult(
                    (CommonOptions opts) => RunCommitAndReturnExitCode(opts),
                    errs => -1);
        }

        private static int RunCommitAndReturnExitCode(CommonOptions opts)
        {
            
            BenchmarkRunner.DefaultTotalSeconds = opts.Seconds;
            BenchmarkRunner.DefaultMaxThreads = opts.MaxThreads;

            //Metric.Config.WithHttpEndpoint("http://localhost:1234/");

            switch (opts.Target)
            {
                case "noop":
                    BenchmarkRunner.Run("Noop", () => { });
                    break;
                case "counter":
                    var counter = new CounterMetric();
                    BenchmarkRunner.Run("Counter", () => counter.Increment());
                    break;
                case "meter":
                    var meter = new MeterMetric();
                    BenchmarkRunner.Run("Meter", () => meter.Mark());
                    break;
                case "histogram":
                    var histogram = new HistogramMetric();
                    BenchmarkRunner.Run("Histogram", () => histogram.Update(137));
                    break;
                case "timer":
                    var timer = new TimerMetric();
                    BenchmarkRunner.Run("Timer", () => timer.Record(1, TimeUnit.Milliseconds));
                    break;
                case "hdrtimer":
                    var hdrTimer = new TimerMetric(new HdrHistogramReservoir());
                    BenchmarkRunner.Run("HDR Timer", () => hdrTimer.Record(1, TimeUnit.Milliseconds));
                    break;
                case "ewma":
                    var ewma = EWMA.OneMinuteEWMA();
                    BenchmarkRunner.Run("EWMA", () => ewma.Update(1));
                    break;
                case "edr":
                    var edr = new ExponentiallyDecayingReservoir();
                    BenchmarkRunner.Run("EDR", () => edr.Update(1));
                    break;
                case "hdr":
                    var hdrReservoir = new HdrHistogramReservoir();
                    BenchmarkRunner.Run("HDR Recorder", () => hdrReservoir.Update(1));
                    break;
                case "uniform":
                    var uniform = new UniformReservoir();
                    BenchmarkRunner.Run("Uniform", () => uniform.Update(1));
                    break;
                case "sliding":
                    var sliding = new SlidingWindowReservoir();
                    BenchmarkRunner.Run("Sliding", () => sliding.Update(1));
                    break;
                case "timerimpact":
                    var load = new WorkLoad();
                    BenchmarkRunner.Run("WorkWithoutTimer", () => load.DoSomeWork(), iterationsChunk: 10);
                    BenchmarkRunner.Run("WorkWithTimer", () => load.DoSomeWorkWithATimer(), iterationsChunk: 10);
                    break;
            }
            return 0;
        }
    }
}
