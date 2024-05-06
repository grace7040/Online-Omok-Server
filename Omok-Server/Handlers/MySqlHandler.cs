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
            var userData = await mySqlDb.GetUserGameDataAsync(userId);
            var response = _packetMgr.MakeInResDbLoadUserGameDataPacket(packetData.SessionID, 
                                                                        userId, 
                                                                        ErrorCode.None, 
                                                                        userData.Win_Count, 
                                                                        userData.Lose_Count, 
                                                                        userData.Level, 
                                                                        userData.Exp); 
            return response;
        }

        public async Task<OmokBinaryRequestInfo> RequestSaveUserGameData(OmokBinaryRequestInfo packetData, MySqlDb mySqlDb)
        {
            var userData = _packetMgr.GetPacketData<PKTReqDbSaveUserGameData>(packetData.Data);
            var result = await mySqlDb.UpdateUserGameDataAsync(userData.UserID,
                                                                userData.WinCount,
                                                                userData.LoseCount,
                                                                userData.Level,
                                                                userData.Exp);
            var response = _packetMgr.MakeInResDbSaveUserGameDataPacket(packetData.SessionID, result);
            return response;
        }
    }
}
