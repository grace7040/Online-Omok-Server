using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public interface IBinaryPacketDataCreater
    {
        byte[] PacketDataToBinary<T>(ref byte[] packetData, T pkHeader, PacketId packetId);
        byte[] PacketIdToBinary(PacketId packetId);

        public T BinaryToPacketData<T>(byte[] binaryPacketData);
    }

    public class MemoryPackBinaryPacketDataCreater : IBinaryPacketDataCreater
    {
        public byte[] PacketDataToBinary<T>(ref byte[] packetData, T pkHeader, PacketId packetId)
        {
            packetData = MemoryPackSerializer.Serialize(pkHeader);
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
