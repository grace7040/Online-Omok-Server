using SuperSocket.Common;
using SuperSocket.SocketEngine.Protocol;

namespace OmokServer;
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

    protected override OmokBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] readBuffer, int offset, int length)
    {
        OmokBinaryRequestInfo packet = new();

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
                //offset이 헤더보다 작으므로 헤더와 보디를 직접 합쳐야 한다. (버퍼 한바퀴 돌아서 처음으로 간 것)
                var packetData = new Byte[length + OmokBinaryRequestInfo.HEADERE_SIZE];
                header.CopyTo(packetData, 0);
                Array.Copy(readBuffer, offset, packetData, OmokBinaryRequestInfo.HEADERE_SIZE, length);

                packet.SetPacketData(packetData);
                return packet;
            }
        }

        packet.SetPacketData(header.CloneRange(header.Offset, header.Count));
        return packet;
    }

}
