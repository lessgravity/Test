using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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

        private DateTime _nextPlayerUpdate;

        private readonly ConcurrentQueue<IPacket> _packetQueue;
        private readonly IDictionary<Type, Action<NetworkClient, IPacket>> _packetHandlers;

        public bool IsSpawned { get; private set; }

        public void Connect(IPEndPoint endPoint)
        {
            _endPoint = endPoint;

            if (_client != null)
            {
                throw new InvalidOperationException("Client already connected");
            }
            _client = new TcpClient();
            _client.Connect(_endPoint);
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

        private void HandlePacket(IPacket packet)
        {
            var packetType = packet.GetType();
            Action<NetworkClient, IPacket> packetHandler;
            if (_packetHandlers.TryGetValue(packetType, out packetHandler))
            {
                Debug.WriteLine("Handle Packet: {0}", packetType.Name);
                packetHandler(this, packet);
            }
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

                if (IsSpawned && _nextPlayerUpdate < DateTime.Now)
                {
                    /*
                    lock (_positionLock)
                    {
                        SendPacket(new PlayerPacket(true));
                        if (_positionChanged)
                        {
                            SendPacket(new PlayerPositionPacket(XYZ));
                            _positionChanged = false;
                        }
                    }
                     * */
                }
                while (_packetQueue.Count > 0)
                {
                    IPacket packet;
                    if (!_packetQueue.TryDequeue(out packet))
                    {
                        continue;
                    }
                    try
                    {
                        _networkManager.WritePacket(packet, PacketDirection.ToServer);
                        if (packet is DisconnectPacket)
                        {
                            return;
                        }
                    }
                    catch
                    {
                    }
                }
                var readTimeOut = DateTime.Now.AddMilliseconds(20);
                while (_networkStream.DataAvailable && DateTime.Now < readTimeOut)
                {
                    try
                    {
                        var packet = _networkManager.ReadPacket(PacketDirection.ToClient);
                        HandlePacket(packet);
                        if (packet is DisconnectPacket)
                        {
                            Debug.WriteLine(((DisconnectPacket) packet).Reason);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                _networkResetEvent.Set();
                _networkResetEvent.Reset();
                Thread.Sleep(1);
            }
        }

        public void SendPacket(IPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet");
            }
            _packetQueue.Enqueue(packet);
        }
    }
}
