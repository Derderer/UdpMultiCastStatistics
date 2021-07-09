using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UdpMultiCastStatistics.Client
{
    /// <summary>
    /// Class for calculating statistics data
    /// </summary>
    public class StatisticsWorker
    {
        public decimal Mean { get; set; }
        public IEnumerable<int> Mode { get; set; } = new List<int>();
        public decimal Median { get; set; }
        public decimal StandardDeviation { get; set; }

        /// <summary>
        /// Starting a new thread for calculations
        /// </summary>
        public void StartWork()
        {
            var statisticsThread = new Thread(GetStatisticsData);
            statisticsThread.Start();
        }

        /// <summary>
        /// Starting the calculation of statistics data
        /// </summary>
        private void GetStatisticsData()
        {
            while (true)
            {
                try
                {
                    SortedDictionary<int, long> data;

                    lock (Program.Data)
                    {
                        if (Program.Data.Count == 0)
                            continue;

                        data = new SortedDictionary<int, long>(Program.Data);
                    }

                    var calculateData = CalculateData(data);

                    Mean = calculateData.TotalSum / calculateData.Count;
                    Median = CalculateMedian(data, calculateData.Count);
                    StandardDeviation = CalculateStandardDeviation(data, Mean, calculateData.Count);

                    var modeValue = data
                        .OrderByDescending(x => x.Value)
                        .Select(x => x.Value)
                        .FirstOrDefault();

                    Mode = data.Where(x => x.Value == modeValue).Select(x => x.Key);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Calculating total sum and counting all numbers
        /// </summary>
        /// <param name="data">Sorted dictionary with elements</param>
        private DataResult CalculateData(SortedDictionary<int, long> data)
        {
            decimal totalSum = 0;
            long totalCount = 0;

            foreach (var pair in data)
            {
                var key = pair.Key;
                var value = pair.Value;

                totalCount += value;
                totalSum += key * (decimal) value;
            }

            return new DataResult {TotalSum = totalSum, Count = totalCount};
        }

        /// <summary>
        /// Calculating standard deviation of received data
        /// </summary>
        /// <param name="data">Sorted dictionary</param>
        /// <param name="mean">Mean value</param>
        /// <param name="totalCount">Total count number</param>
        /// <returns>Стандартное отклонение коллекции</returns>
        private decimal CalculateStandardDeviation(SortedDictionary<int, long> data, decimal mean, long totalCount)
        {
            double deviation = 0;
            if (data.Count <= 1)
                return (decimal)deviation;

            double sum = 0;

            foreach (var pair in data)
            {
                for (long i = 0; i < pair.Value; i++)
                {
                    sum += Math.Pow(pair.Key - (double)mean, 2);
                }
            }

            deviation = Math.Sqrt(sum / (totalCount - 1));
            return (decimal)deviation;
        }

        /// <summary>
        /// Calculating median of received data
        /// </summary>
        /// <param name="data">Sorted dictionary</param>
        /// <param name="totalCount">Total count of received numbers</param>
        /// <returns>Median</returns>
        /// <remarks>
        /// P.S. I personally do not really like this solution,
        /// but this is the solution that I was able to come up with at the current time.
        /// I could not find other ideas yet
        /// </remarks>
        private decimal CalculateMedian(SortedDictionary<int, long> data, long totalCount)
        {
            var isOdd = totalCount % 2 == 1;
            var index = totalCount / 2;

            decimal result = 0;
            int? lastKey = null;

            foreach (var pair in data)
            {
                if (index - pair.Value >= 0)
                    lastKey = pair.Key;

                index -= pair.Value;
                if (isOdd)
                {
                    if (index >= 0)
                        continue;

                    result = pair.Key;
                    break;
                }

                if (index >= 0)
                {
                    continue;
                }

                result = lastKey.HasValue && index == -1
                    ? (pair.Key + (decimal) lastKey) / 2
                    : pair.Key;
                break;
            }

            return result;
        }

        /// <summary>
        /// Structure for the result of calculating the total sum and number of elements
        /// </summary>
        private struct DataResult
        {
            public decimal TotalSum { get; set; }

            public long Count { get; set; }
        }
    }
}