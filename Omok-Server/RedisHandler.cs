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
        public void RegistDbHandler(Dictionary<int, Func<OmokBinaryRequestInfo, RedisDb, Task<OmokBinaryRequestInfo>>> dbWorkHandlerMap) 
        {
            dbWorkHandlerMap.Add((int)PacketId.ReqDbLogin, RequestLogin);
        }
        public async Task<OmokBinaryRequestInfo> RequestLogin(OmokBinaryRequestInfo packetData, RedisDb redisDb)
        {
            var reqData = _packetMgr.GetPacketData<PKTReqDbLogin>(packetData.Data);
            var result = await redisDb.CheckUserAuthAsync(reqData.UserID, reqData.AuthToken);
            var response = _packetMgr.MakeInResDbLoginPacket(packetData.SessionID, reqData.UserID, result);
            return response;
        }
    }
}
