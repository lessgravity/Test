using System;
using System.IO;
using TestNetwork.Network.Packets;

namespace TestNetwork.Network
{
    public class NetworkManager
    {
        private readonly Stream _dataStream;
        private readonly object _dataStreamLock;

        public NetworkManager(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            _dataStream = stream;
            _dataStreamLock = new object();
        }

        public IPacket ReadPacket(PacketDirection packetDirection)
        {
            lock (_dataStreamLock)
            {
                return null;
            }
        }

        public void WritePacket(IPacket packet, PacketDirection packetDirection)
        {
            lock (_dataStreamLock)
            {
                
            }
        }
    }
}
