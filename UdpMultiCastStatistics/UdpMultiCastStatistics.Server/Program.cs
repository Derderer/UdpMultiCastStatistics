using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpMultiCastStatistics.Server
{
    /// <summary>
    /// Main class of Server console application
    /// </summary>
    class Program
    {
        private static UdpClient _udpServerClient;
        private static IPEndPoint _ipEndPoint;

        private static readonly int MinimumValue;
        private static readonly int MaximumValue;
        private static readonly int MulticastGroupPort;
        private static readonly string MulticastGroupIp;

        static Program()
        {
            // Получаемые настройки из файла конфигураций Settings.config
            int.TryParse(ConfigurationManager.AppSettings.Get("MinimumValue"), out MinimumValue);
            int.TryParse(ConfigurationManager.AppSettings.Get("MaximumValue"), out MaximumValue);

            MulticastGroupIp = ConfigurationManager.AppSettings.Get("MulticastGroupIP");
            int.TryParse(ConfigurationManager.AppSettings.Get("MulticastGroupPort"), out MulticastGroupPort);
        }

        static void Main(string[] args)
        {
            InitializeUdpServer();
            StartSending();
        }

        /// <summary>
        /// Initializing of UDP client
        /// </summary>
        private static void InitializeUdpServer()
        {
            _udpServerClient = new UdpClient();

            var multicastAddress = IPAddress.Parse(MulticastGroupIp);
            _udpServerClient.JoinMulticastGroup(multicastAddress);
            _ipEndPoint = new IPEndPoint(multicastAddress, MulticastGroupPort);
        }

        /// <summary>
        /// Start sending data via UDP Multicast
        /// </summary>
        private static void StartSending()
        {
            var random = new Random();
            Console.WriteLine("Start generating numbers");
            while (true)
            {
                // Adding 1 to the maximum number, because maximum number is not taken when generating the random number
                var number = random.Next(MinimumValue, MaximumValue + 1);

                var buffer = Encoding.Unicode.GetBytes(number.ToString());
                _udpServerClient.Send(buffer, buffer.Length, _ipEndPoint);
            }
        }
    }
}