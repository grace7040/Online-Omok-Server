using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class MySqlHandler
    {
        PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();
        public void RegistDbHandler(Dictionary<int, Func<OmokBinaryRequestInfo, MySqlDb, Task<OmokBinaryRequestInfo>>> dbWorkHandlerMap)
        {
            dbWorkHandlerMap.Add((int)PacketId.ReqDbLoadUserGameData, RequestLoadUserGameData);
            dbWorkHandlerMap.Add((int)PacketId.ReqDbSaveUserGameData, RequestSaveUserGameData);
        }

        public async Task<OmokBinaryRequestInfo> RequestLoadUserGameData(OmokBinaryRequestInfo packetData, MySqlDb mySqlDb)
        {
            var userId = _packetMgr.GetPacketData<PKTReqDbLoadUserGameData>(packetData.Data).UserID;
            var userDatas = await mySqlDb.LoadUserGameDataAsync(userId);
            var response = _packetMgr.MakeInResDbLoadUserGameDataPacket(packetData.SessionID, userId, ErrorCode.None, userDatas.WinCount, userDatas.LoseCount, userDatas.Level, userDatas.Exp); 
            return response;
        }

        public async Task<OmokBinaryRequestInfo> RequestSaveUserGameData(OmokBinaryRequestInfo packetData, MySqlDb mySqlDb)
        {
            return new OmokBinaryRequestInfo();
        }
    }
}
