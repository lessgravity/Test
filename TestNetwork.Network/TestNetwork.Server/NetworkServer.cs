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
        private readonly object _networkLock;
        private Thread _networkThread;
        private readonly ManualResetEvent _clientConnectedEvent;

        private readonly object _clientsLock;
        private readonly IList<NetworkServerClient> _clients;

        public IDictionary<Type, Action<NetworkServerClient, NetworkServer>> PacketHandlers { get; private set; }

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
            lock (_clientsLock)
            {
                var listener = ar.AsyncState as TcpListener;
                //
                // if listener is stopped right before the last accept started, return
                //
                if (listener == null)
                {
                    return;
                }
                try
                {
                    var client = new NetworkServerClient(listener.EndAcceptTcpClient(ar), this);
                    _clients.Add(client);
                }
                catch (ObjectDisposedException)
                {
                    //
                    // handle that somehow
                    //
                    return;
                }

                //
                // keep accepting incoming connections
                //
                listener.BeginAcceptTcpClient(AcceptTcpClientAsync, null);

                _clientConnectedEvent.Set();
            }
        }

        private void DisconnectClients()
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

            _networkLock = new object();
            PacketHandlers = new Dictionary<Type, Action<NetworkServerClient, NetworkServer>>();
            _clientConnectedEvent = new ManualResetEvent(false);
        }

        private void NetworkThreadProc(object state)
        {
            while (true)
            {
                lock (_networkLock)
                {
                    //
                    // 1. send out all queued client packets per client
                    //
                    foreach (var client in _clients)
                    {
                        client.SendPackets();
                    }
                }
                Thread.Sleep(10);
            }
        }

        public void Start(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }
            _endPoint = endPoint;

            _listener = new TcpListener(_endPoint);
            _listener.Start();
            _listener.BeginAcceptTcpClient(AcceptTcpClientAsync, _listener);

            _networkThread = new Thread(NetworkThreadProc);
            _networkThread.Start();
        }

        public void Stop()
        {
            _listener.Stop();

            DisconnectClients();

            if (_networkThread != null)
            {
                _networkThread.Abort();
                _networkThread = null;
            }
        }
    }
}
