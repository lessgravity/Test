using LessGravity.Common;

namespace TestNetwork.Network.Packets
{
    public struct KeepAlivePacket : IPacket
    {
        public int KeepAlive;

        public KeepAlivePacket(int keepAlive)
        {
            KeepAlive = keepAlive;
        }

        public void ReadPacket(DataStream stream)
        {
            KeepAlive = stream.ReadInt32();
        }

        public void WritePacket(DataStream stream)
        {
            stream.WriteInt32(KeepAlive);
        }
    }
}