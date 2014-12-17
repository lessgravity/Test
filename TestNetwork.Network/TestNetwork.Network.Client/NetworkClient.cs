using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TestNetwork.Network.Packets;

namespace TestNetwork.Network.Client
{
    class NetworkClient : IDisposable
    {
        private IPEndPoint _endPoint;
        private TcpClient _client;
        private object _networkLock;
        private NetworkManager _networkManager;
        private ManualResetEvent _networkResetEvent;
        private readonly NetworkSession _networkSession;
        private NetworkStream _networkStream;
        private Thread _networkThread;

        private readonly ConcurrentQueue<IPacket> _packetQueue;
        private readonly IDictionary<Type, Action<NetworkClient, IPacket>> _packetHandlers; 

        public void Connect(IPEndPoint endPoint)
        {
            _endPoint = endPoint;

            if (_client != null)
            {
                throw new InvalidOperationException("Client already connected");
            }
            _client = new TcpClient(_endPoint);
            _networkStream = _client.GetStream();
            _networkManager = new NetworkManager(_networkStream);

            _networkResetEvent = new ManualResetEvent(true);
            _networkThread = new Thread(NetworkThreadProc);
            _networkThread.Start();
        }

        public void Disconnect(string reason)
        {
            if (_networkThread != null)
            {
                _networkThread.Abort();
                _networkThread = null;
            }
            if (_client.Connected)
            {
                try
                {
                    _networkManager.WritePacket(new DisconnectPacket(reason), PacketDirection.ToServer);
                }
                catch
                {
                }
                finally
                {
                    _client.Close();
                }
            }
        }

        public void Dispose()
        {
        }

        public NetworkClient(NetworkSession networkSession)
        {
            _networkLock = new object();
            _networkSession = networkSession;
            _packetHandlers = new Dictionary<Type, Action<NetworkClient, IPacket>>();
            _packetQueue = new ConcurrentQueue<IPacket>();
        }

        private void NetworkThreadProc(object state)
        {
            while (true)
            {
                _networkResetEvent.Set();
                _networkResetEvent.Reset();
                Thread.Sleep(1);
            }
        }
    }
}
