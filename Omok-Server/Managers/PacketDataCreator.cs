using MemoryPack;

namespace Omok_Server
{
    public interface IBinaryPacketDataCreator
    {
        byte[] PacketDataToBinary<T>(T pkHeader, PacketId packetId);
        byte[] PacketIdToBinary(PacketId packetId);

        public T BinaryToPacketData<T>(byte[] binaryPacketData);
    }

    public class MemoryPackBinaryPacketDataCreator : IBinaryPacketDataCreator
    {
        public byte[] PacketDataToBinary<T>(T pkHeader, PacketId packetId)
        {
            var packetData = MemoryPackSerializer.Serialize(pkHeader);
            MemoryPackPacketHeadInfo.Write(packetData, packetId);
            return packetData;
        }

        public byte[] PacketIdToBinary(PacketId packetId)
        {
            byte[] packetData = new byte[MemoryPackPacketHeadInfo.HeadSize];
            MemoryPackPacketHeadInfo.WritePacketId(packetData, (UInt16)packetId);
            return packetData;
        }

        public T BinaryToPacketData<T>(byte[] binaryPacketData) 
        {
            return MemoryPackSerializer.Deserialize<T>(binaryPacketData);
        }
    }
}
