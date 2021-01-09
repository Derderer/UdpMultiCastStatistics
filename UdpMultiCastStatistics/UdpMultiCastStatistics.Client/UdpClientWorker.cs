using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UdpMultiCastStatistics.Client
{
    /// <summary>
    /// UdpClient for receiving numbers for Multicast Group
    /// </summary>
    public class UdpClientWorker
    {
        private static UdpClient _udpServerClient;
        private static IPEndPoint _ipEndPoint;

        public void StartWork()
        {
            var udpClientThread = new Thread(StartMulticastDataReceiving);
            udpClientThread.Start();
        }

        /// <summary>
        /// Start the process to receive data from Multicast Group
        /// </summary>
        private static void StartMulticastDataReceiving()
        {
            InitializeUdpClient();

            while (true)
            {
                try
                {
                    var data = _udpServerClient.Receive(ref _ipEndPoint);
                    int.TryParse(Encoding.Unicode.GetString(data), out var result);
                    Program.AvailablePackets = _udpServerClient.Available;

                    lock (Program.Data)
                    {
                        Program.AvailablePackets = _udpServerClient.Available;
                        if (Program.Data.ContainsKey(result))
                        {
                            var value = Program.Data.First(x => x.Key == result).Value;
                            Program.Data[result] = ++value;
                        }
                        else
                        {
                            Program.Data.Add(result, 1);
                        }
                    }

                    Thread.Sleep(Program.SleepTimer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Initializing UDP client for listening Multicast Group
        /// </summary>
        private static void InitializeUdpClient()
        {
            _udpServerClient = new UdpClient {ExclusiveAddressUse = false};

            _ipEndPoint = new IPEndPoint(IPAddress.Any, Program.MulticastGroupPort);

            _udpServerClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpServerClient.Client.Bind(_ipEndPoint);

            var multicastAddress = IPAddress.Parse(Program.MulticastGroupIp);
            _udpServerClient.JoinMulticastGroup(multicastAddress);
        }
    }
}