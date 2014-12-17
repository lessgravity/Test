using LessGravity.Common;

namespace TestNetwork.Network.Packets
{
    public struct DisconnectPacket : IPacket
    {
        public string Reason;

        public DisconnectPacket(string reason)
        {
            Reason = reason;
        }

        public void ReadPacket(DataStream dataStream)
        {
            Reason = dataStream.ReadString();
        }

        public void WritePacket(DataStream dataStream)
        {
            dataStream.WriteString(Reason);
        }
    }
}
