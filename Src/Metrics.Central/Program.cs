using System;

namespace Metrics.Central
{
    class Program
    {
        static void Main(string[] args)
        {
            MetricsService ms = new MetricsService();

            ms.Start();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            ms.Stop();
        }
    }
}
