using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using TestNetwork.Network;
using TestNetwork.Network.Packets;

namespace TestNetwork.Server
{
    class NetworkServerClient : IDisposable
    {
        private readonly Stream _networkStream;
        private readonly TcpClient _client;
        private readonly ConcurrentQueue<IPacket> _packetQueue;
        private readonly NetworkManager _networkManager;
        private readonly NetworkServer _server;


        public bool IsDisconnectPending { get; private set; }

        public bool IsLoggedIn { get; private set; }
        protected internal DateTime LastKeepAlive { get; set; }
        protected internal DateTime LastKeepAliveSent { get; set; }

        private void Disconnect(string reason = null)
        {
            if (!_server.Clients.Contains(this))
            {
                throw new InvalidOperationException("The server is not aware of this client.");
            }

            if (!string.IsNullOrEmpty(reason))
            {
                try
                {

                }
                catch { }
            }
            try
            {

            }
            catch { }
            if (IsLoggedIn)
            {
                //
                // tell entitymanager to remove the client's entity
                //
            }
            _server.Clients.Remove(this);
            if (IsLoggedIn)
            {
                
            }
            Dispose();
        }

        public void Dispose()
        {
            if (_client != null)
            {
                if (_client.Client.Connected)
                {
                    _client.Close();
                }
            }
        }

        private void HandlePacket(IPacket packet)
        {
            var packetType = packet.GetType();
            _server.PacketHandlers[packetType](this, _server);
        }

        public NetworkServerClient(TcpClient client, NetworkServer server)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            _server = server;
            _client = client;
            _networkStream = _client.GetStream();
            _networkManager = new NetworkManager(_networkStream);
            _packetQueue = new ConcurrentQueue<IPacket>();
        }

        public void SendPacket(IPacket packet)
        {
            _packetQueue.Enqueue(packet);
        }

        public void SendPackets()
        {
            IsDisconnectPending = false;
            //
            // 1. sending out all pending packets
            //
            while (_packetQueue.Count > 0)
            {
                IPacket packet;
                if (!_packetQueue.TryDequeue(out packet))
                {
                    continue;
                }
                try
                {
                    _networkManager.WritePacket(packet, PacketDirection.ToClient);
                }
                catch (IOException)
                {
                    IsDisconnectPending = true;
                    continue;
                }
                if (packet is DisconnectPacket)
                {
                    IsDisconnectPending = true;
                }
            }

            if (IsDisconnectPending)
            {
                //
                // Disconnect
                //
            }

            //
            // 2. read in all pending packets
            //
            var timeout = DateTime.Now.AddMilliseconds(20);
            while (_client.Available > 0 && DateTime.Now < timeout)
            {
                try
                {
                    var packet = _networkManager.ReadPacket(PacketDirection.ToServer);
                    if (packet is DisconnectPacket)
                    {
                        
                    }
                    HandlePacket(packet);
                }
                catch (SocketException)
                {
                    //
                    // Disconnect
                    //
                }
            }
            if (IsLoggedIn)
            {
                Update();
            }
        }

        private void Update()
        {
            if (LastKeepAliveSent.AddSeconds(20) < DateTime.Now)
            {
                SendPacket(new KeepAlivePacket(new Random().Next()));
                LastKeepAliveSent = DateTime.Now;
            }
        }
    }
}
