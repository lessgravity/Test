using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestNetwork.Server
{
    class NetworkServer : IDisposable
    {
        private IPEndPoint _endPoint;
        private Thread _entityThread;
        private TcpListener _listener;
        private Thread _networkThread;

        private object _clientsLock;
        private readonly IList<NetworkServerClient> _clients;

        public IList<NetworkServerClient> Clients
        {
            get
            {
                lock (_clientsLock)
                {
                    return _clients;
                }
            }
        }

        private void AcceptTcpClientAsync(IAsyncResult ar)
        {
            
        }

        public void Dispose()
        {
            if (_listener != null)
            {

            }
        }

        public NetworkServer()
        {
            _clientsLock = new object();
            _clients = new List<NetworkServerClient>();
        }

        public void Start(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }
            _endPoint = endPoint;

            _listener = new TcpListener(_endPoint);
            _listener.BeginAcceptTcpClient(AcceptTcpClientAsync, null);
        }
    }
}
