using MemoryPack;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    //패킷 직렬화 방식에 따른 패킷 관리자
    public class PacketManager<TPacketDataCreator> where TPacketDataCreator : IBinaryPacketDataCreator, new()
    {
        TPacketDataCreator _dataCreator = new TPacketDataCreator();

        public OmokBinaryRequestInfo MakeInNTFConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
        {
            OmokBinaryRequestInfo innerPacket = new OmokBinaryRequestInfo();

            if (isConnect) {
                innerPacket.Data = _dataCreator.PacketIdToBinary(PacketId.INNTF_CONNECT_CLIENT);
            }
            else {
                innerPacket.Data = _dataCreator.PacketIdToBinary(PacketId.INNTF_DISCONNECT_CLIENT);
            }

            innerPacket.SessionID = sessionID;

            return innerPacket;
        }

        public OmokBinaryRequestInfo MakeReqRoomLeavePacket(string sessionID, int roomNumber, string userID)
        {
            var packet = new PKTReqRoomLeave()
            {
                RoomNumber = roomNumber,
                UserID = userID,
            };

            var sendData = GetBinaryPacketData(packet, PacketId.NTF_IN_ROOM_LEAVE);

            var memoryPakcPacket = new OmokBinaryRequestInfo();
            memoryPakcPacket.Data = sendData;
            memoryPakcPacket.SessionID = sessionID;
            return memoryPakcPacket;
        }

        public byte[] GetBinaryPacketData<T>(T pkHeader, PacketId packetId) where T : PKHeader
        {
            byte[] sendData = _dataCreator.PacketDataToBinary(pkHeader, packetId);

            return sendData;
        }

        public T GetPacketData<T>(byte[] binaryPacketData) where T : PKHeader
        {
            return _dataCreator.BinaryToPacketData<T>(binaryPacketData);
        }
    }
}
