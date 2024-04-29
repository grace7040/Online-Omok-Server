using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    
    public class ReceiveFilter : FixedHeaderReceiveFilter<OmokBinaryRequestInfo> 
    {
        public ReceiveFilter() : base(OmokBinaryRequestInfo.HEADERE_SIZE)
        {
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(header, offset, 2);
            }

            var totalSize = BitConverter.ToUInt16(header, offset + OmokBinaryRequestInfo.PACKET_HEADER_MEMORYPACK_START_POS);
            return totalSize - OmokBinaryRequestInfo.HEADERE_SIZE;
        }

        // offset: header 데이터까지 있는 readBuffer 에서 body가 시작되는 위치를 가리킨다
        // length: body 데이터의 크기 
        protected override OmokBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] readBuffer, int offset, int length)
        {
            OmokBinaryRequestInfo packet = new();

            // body 데이터가 있는 경우
            if (length > 0)
            {
                if (offset >= OmokBinaryRequestInfo.HEADERE_SIZE)
                {
                    var packetStartPos = offset - OmokBinaryRequestInfo.HEADERE_SIZE;
                    var packetSize = length + OmokBinaryRequestInfo.HEADERE_SIZE;

                    packet.SetPacketData(readBuffer.CloneRange(packetStartPos, packetSize));
                    return packet;
                }
                else
                {
                    //offset 이 헤더 크기보다 작으므로 헤더와 보디를 직접 합쳐야 한다.
                    var packetData = new Byte[length + OmokBinaryRequestInfo.HEADERE_SIZE];
                    header.CopyTo(packetData, 0);
                    Array.Copy(readBuffer, offset, packetData, OmokBinaryRequestInfo.HEADERE_SIZE, length);

                    packet.SetPacketData(packetData);
                    return packet;
                }
            }

            // body 데이터가 없는 경우
            packet.SetPacketData(header.CloneRange(header.Offset, header.Count));
            return packet;
        }

    }
}
