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

        /// <summary>
        /// Sleep timer in milliseconds
        /// </summary>
        public static int SleepTimer { get; }

        /// <summary>
        /// Multicast Group, where data is received
        /// </summary>
        public static int MulticastGroupPort { get; }

        /// <summary>
        /// Multicast Group IP
        /// </summary>
        public static string MulticastGroupIp { get; }

        private static readonly StatisticsWorker StatisticsWorker;
        private static readonly UdpClientWorker UdpWorker;

        static Program()
        {
            StatisticsWorker = new StatisticsWorker();
            UdpWorker = new UdpClientWorker();
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
            UdpWorker.StartWork();
            StatisticsWorker.StartWork();

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
                Console.WriteLine("             Statistical data");
                Console.WriteLine($"Available pckets: {UdpWorker.AvailablePackets}");
                Console.WriteLine($"Mean: {StatisticsWorker.Mean}");
                Console.WriteLine($"Mode: {string.Join(", ", StatisticsWorker.Mode)}");
                Console.WriteLine($"Median: {StatisticsWorker.Median}");
                Console.WriteLine($"Standard deviation: {StatisticsWorker.StandardDeviation}");
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