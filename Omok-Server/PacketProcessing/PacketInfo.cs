using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class OmokBinaryRequestInfo : BinaryRequestInfo
    {
        public string SessionID { get; set; } = "";
        public byte[] Data { get; set; }

        public const int PACKET_HEADER_MEMORYPACK_START_POS = 1;
        public const int HEADERE_SIZE = 5 + PACKET_HEADER_MEMORYPACK_START_POS;

        public OmokBinaryRequestInfo()
            : base(null, null)
        {
            Data = [];
        }
        public OmokBinaryRequestInfo(byte[] packetData)
        : base(null, packetData)
        {
            Data = packetData;
        }

        public void SetPacketData(byte[] packetData)
        {
            Data = packetData;
        }

    }
}
