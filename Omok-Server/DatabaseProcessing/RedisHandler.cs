using CloudStructures.Structures;
using CloudStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class RedisHandler
    {
        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();
        public void RegistDbHandlerMap(Dictionary<int, Func<OmokBinaryRequestInfo, RedisDb, OmokBinaryRequestInfo>> dbWorkHandlerMap) 
        {
            dbWorkHandlerMap.Add((int)PacketId.ReqDbLogin, RequestLogin);
            dbWorkHandlerMap.Add((int)PacketId.ReqDbLeaveRoom, RequestLeaveRoom);
        }
        public OmokBinaryRequestInfo RequestLogin(OmokBinaryRequestInfo packetData, RedisDb redisDb)
        {
            var reqData = _packetMgr.GetPacketData<PKTReqDbLogin>(packetData.Data);
            var result = redisDb.CheckUserAuth(reqData.UserID, reqData.AuthToken);
            var response = _packetMgr.MakeInResDbLoginPacket(packetData.SessionID, reqData.UserID, result);
            return response;
        }

        public OmokBinaryRequestInfo RequestLeaveRoom(OmokBinaryRequestInfo packetData, RedisDb redisDb)
        {
            var reqData = _packetMgr.GetPacketData<PKTReqDbLeaveRoom>(packetData.Data);
            var result = redisDb.RemoveUserRoomNumber(reqData.UserID, reqData.RoomNumber);
            var response = _packetMgr.MakeInResDbLeaveRoom(packetData.SessionID, reqData.UserID, result); ;
            return response;
        }

    }
}
