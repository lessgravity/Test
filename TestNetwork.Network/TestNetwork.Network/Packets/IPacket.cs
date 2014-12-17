using LessGravity.Common;

namespace TestNetwork.Network.Packets
{
    public interface IPacket
    {
        void ReadPacket(DataStream dataStream);
        void WritePacket(DataStream dataStream);
    }
}
