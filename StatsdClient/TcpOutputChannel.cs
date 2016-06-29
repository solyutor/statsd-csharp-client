using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace StatsdClient
{
    internal sealed class TcpOutputChannel : IOutputChannel
    {
        private readonly TcpClient _tcpClient;
        private readonly string _host;
        private readonly int _port;
        private readonly int _retryAttempts;

        public TcpOutputChannel(string host, int port, byte retryAttempts = 3)
        {
            _host = host;
            _port = port;
            _retryAttempts = retryAttempts;
            _tcpClient = new TcpClient();
        }

        public void Send(byte[] buffer, int length)
        {
            int attempt = -1;
            do
            {
                attempt++;
            } while (!TrySend(buffer, length) && attempt < _retryAttempts);
        }

        private bool TrySend(byte[] buffer, int length)
        {
            try
            {
                if (!_tcpClient.Connected)
                {
                    _tcpClient.Connect(_host, _port);
                }
                _tcpClient.GetStream().Write(buffer, 0, length);
                return true;
            }
            catch (IOException ex)
            {
                // No more attempts left, so log it and continue
                Trace.TraceWarning("Sending metrics via TCP failed with an IOException: {0}", ex.Message);
            }
            catch (SocketException ex)
            {
                // No more attempts left, so log it and continue
                Trace.TraceWarning("Sending metrics via TCP failed with a SocketException: {0}, code: {1}", ex.Message, ex.SocketErrorCode.ToString());
            }
            return false;
        }
    }
}