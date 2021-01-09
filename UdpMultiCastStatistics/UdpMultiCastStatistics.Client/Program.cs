using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace UdpMultiCastStatistics.Client
{
    /// <summary>
    /// Main class of Client console application
    /// </summary>
    class Program
    {
        /// <summary>
        /// Sorted dictionaty, where:
        /// TKey - received number from UdpClient stream
        /// TValue - the number of times the given number was encountered
        /// </summary>
        public static SortedDictionary<int, long> Data = new SortedDictionary<int, long>();

        public static int SleepTimer { get; }
        public static int MulticastGroupPort { get; }
        public static string MulticastGroupIp { get; }

        public static int AvailablePackets { get; set; }
        public static decimal Mean { get; set; }
        public static IEnumerable<int> Mode { get; set; } = new List<int>();
        public static decimal Median { get; set; }
        public static decimal StandardDeviation { get; set; }

        static Program()
        {
            MulticastGroupIp = ConfigurationManager.AppSettings.Get("MulticastGroupIP");
            int.TryParse(ConfigurationManager.AppSettings.Get("MulticastGroupPort"), out var multicastGroupPort);
            int.TryParse(ConfigurationManager.AppSettings.Get("SleepTimer"), out var sleepTimer);
            if (sleepTimer < 0)
                sleepTimer = 0;

            MulticastGroupPort = multicastGroupPort;
            SleepTimer = sleepTimer;
        }

        private static void Main(string[] args)
        {
            Start();
        }

        /// <summary>
        /// Running threads and endless loop
        /// </summary>
        private static void Start()
        {
            var udpWorker = new UdpClientWorker();
            var statWorker = new StatisticsWorker();

            udpWorker.StartWork();
            statWorker.StartWork();

            while (true)
            {
                if (Console.ReadKey(true).Key != ConsoleKey.Enter)
                    continue;

                var printStatisticsThread = new Thread(PrintStatisticsData);
                printStatisticsThread.Start();
            }
        }

        /// <summary>
        /// Output data to console
        /// </summary>
        private static void PrintStatisticsData()
        {
            try
            {
                lock (Data)
                {
                    if (Data.Count == 0)
                    {
                        Console.WriteLine("No statistics available. The data array is empty.");
                        return;
                    }
                }

                Console.WriteLine("==================================================");
                Console.WriteLine("Statistical data:");
                Console.WriteLine($"Available packets: {AvailablePackets}");
                Console.WriteLine($"Mean: {Mean}");
                Console.WriteLine($"Mode: {string.Join(", ", Mode)}");
                Console.WriteLine($"Median: {Median}");
                Console.WriteLine($"Standard deviation: {StandardDeviation}");
                Console.WriteLine("==================================================");
                Console.WriteLine();
            }
            catch
            {
                // ignored
            }
        }
    }
}