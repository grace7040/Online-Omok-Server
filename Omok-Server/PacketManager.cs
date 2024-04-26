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
    public class PacketManager<TPacketDataCreater> where TPacketDataCreater : IBinaryPacketDataCreater, new()
    {
        TPacketDataCreater _dataCreater = new TPacketDataCreater();

        public OmokBinaryRequestInfo MakeInNTFConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
        {
            OmokBinaryRequestInfo innerPacket = new OmokBinaryRequestInfo();

            if (isConnect) {
                innerPacket.Data = _dataCreater.PacketIdToBinary(PacketId.NTF_CONNECT_CLIENT);
            }
            else {
                innerPacket.Data = _dataCreater.PacketIdToBinary(PacketId.NTF_DISCONNECT_CLIENT);
            }

            innerPacket.SessionID = sessionID;

            return innerPacket;
        }

        public OmokBinaryRequestInfo MakeInNTFRoomLeavePacket(string sessionID, int roomNum, string iD)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBinaryPacketData(PKHeader pkHeader, PacketId packetId)
        {
            byte[] sendData = [];
            _dataCreater.PacketDataToBinary(ref sendData, pkHeader, packetId);

            return sendData;
        }

        public T GetPacketData<T>(byte[] binaryPacketData) where T : PKHeader
        {
            return _dataCreater.BinaryToPacketData<T>(binaryPacketData);
        }
    }
}
