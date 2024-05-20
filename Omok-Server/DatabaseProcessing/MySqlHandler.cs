namespace OmokServer;
public class MySqlHandler
{
    PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();
    public void RegistDbHandler(Dictionary<int, Func<OmokBinaryRequestInfo, MySqlDb, OmokBinaryRequestInfo>> dbWorkHandlerMap)
    {
        dbWorkHandlerMap.Add((int)PacketId.ReqDbLoadUserGameData, RequestLoadUserGameData);
        dbWorkHandlerMap.Add((int)PacketId.ReqDbSaveUserGameData, RequestSaveUserGameData);
    }

    public OmokBinaryRequestInfo RequestLoadUserGameData(OmokBinaryRequestInfo packetData, MySqlDb mySqlDb)
    {
        var userId = _packetMgr.GetPacketData<PKTReqDbLoadUserGameData>(packetData.Data).UserID;
        var userData = mySqlDb.GetUserGameData(userId);
        var response = _packetMgr.MakeInResDbLoadUserGameDataPacket(packetData.SessionID, 
                                                                    userId, 
                                                                    ErrorCode.None, 
                                                                    userData.Win_Count, 
                                                                    userData.Lose_Count, 
                                                                    userData.Level, 
                                                                    userData.Exp); 
        return response;
    }

    public OmokBinaryRequestInfo RequestSaveUserGameData(OmokBinaryRequestInfo packetData, MySqlDb mySqlDb)
    {
        var userData = _packetMgr.GetPacketData<PKTReqDbSaveUserGameData>(packetData.Data);
        var result = mySqlDb.UpdateUserGameData(userData.UserID,
                                                            userData.WinCount,
                                                            userData.LoseCount,
                                                            userData.Level,
                                                            userData.Exp);
        var response = _packetMgr.MakeInResDbSaveUserGameDataPacket(packetData.SessionID, result);
        return response;
    }
}
