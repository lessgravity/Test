using System;
using System.IO;
using System.Net.Sockets;

namespace TestNetwork.Server
{
    class NetworkServerClient : IDisposable
    {
        private Stream _networkStream;
        private TcpClient _client;
        public void Dispose()
        {

        }

        public NetworkServerClient(TcpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            _client = client;
            _networkStream = _client.GetStream();
        }
    }
}
