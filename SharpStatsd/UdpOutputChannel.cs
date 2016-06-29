using System;
using System.Net.Sockets;

namespace SharpStatsd
{
    internal sealed class UdpOutputChannel : IOutputChannel
    {
        private readonly UdpClient _udpClient;

        public UdpOutputChannel(string host, int port)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException($"Expected valid hostname or ip address but was empty string", nameof(port));
            }

            if (port < 1 || port > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(port), $"Expected a value between 1 and {ushort.MaxValue} but was {port}");
            }

            _udpClient = new UdpClient();
            _udpClient.Connect(host, port);
        }

        public void Send(byte[] buffer, int length)
        {
            _udpClient.Send(buffer, buffer.Length);
        }
    }
}