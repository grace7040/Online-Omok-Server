﻿namespace OmokServer;

//패킷 직렬화 방식에 따른 패킷 관리자
public class PacketManager<TPacketDataCreator> where TPacketDataCreator : IBinaryPacketDataCreator, new()
{
    TPacketDataCreator _dataCreator = new();

    public byte[] GetBinaryPacketData<T>(T pkHeader, PacketId packetId) where T : PKHeader
    {
        var sendData = _dataCreator.PacketDataToBinary(pkHeader, packetId);

        return sendData;
    }

    public T GetPacketData<T>(byte[] binaryPacketData) where T : PKHeader
    {
        return _dataCreator.BinaryToPacketData<T>(binaryPacketData);
    }

    public OmokBinaryRequestInfo MakeInNTFConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
    {
        var innerPacket = new OmokBinaryRequestInfo();

        if (isConnect) {
            innerPacket.Data = _dataCreator.PacketIdToBinary(PacketId.NtfInConnectClient);
        }
        else {
            innerPacket.Data = _dataCreator.PacketIdToBinary(PacketId.NtfInDisConnectClient);
        }

        innerPacket.SessionID = sessionID;

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInNTFRoomLeavePacket(string sessionID, int roomNumber, string userID)
    {
        var packet = new PKTReqRoomLeave()
        {
            RoomNumber = roomNumber,
            UserID = userID,
        };

        var sendData = GetBinaryPacketData(packet, PacketId.ReqRoomLeave);

        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInReqHeartBeatPacket()
    {
        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = _dataCreator.PacketIdToBinary(PacketId.ReqInHeartBeat)
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInReqDisConnectUserPacket(string sessionID)
    {
        var innerPacket = new OmokBinaryRequestInfo()
        {
            Data = _dataCreator.PacketIdToBinary(PacketId.ReqInDisConnectUser),
            SessionID = sessionID
        };
        
        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInReqRoomCheckPacket()
    {
        var innerPacket = new OmokBinaryRequestInfo()
        {
            Data = _dataCreator.PacketIdToBinary(PacketId.ReqInRoomCheck)
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInReqDbLoginPacket(string sessionID, string userID, string authToken)
    {
        var responseLogin = new PKTReqDbLogin()
        {
            UserID = userID,
            AuthToken = authToken
        };

        var sendData = GetBinaryPacketData(responseLogin, PacketId.ReqDbLogin);

        var innerPacket = new OmokBinaryRequestInfo()
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }


    public OmokBinaryRequestInfo MakeInResDbLoginPacket(string sessionID, string userID, ErrorCode result)
    {
        var responseLogin = new PKTResInLogin()
        {
            Result = (short)result,
            UserID = userID,
        };

        var sendData = GetBinaryPacketData(responseLogin, PacketId.ResDbLogin);

        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInReqDbLeaveRoom(string sessionID, string userID, int roomNumber)
    {
        var responseLogin = new PKTReqDbLeaveRoom()
        {
            UserID = userID,
            RoomNumber = roomNumber
        };

        var sendData = GetBinaryPacketData(responseLogin, PacketId.ReqDbLeaveRoom);

        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInResDbLeaveRoom(string sessionID, string userID, ErrorCode result)
    {
        var responseLogin = new PKTResDbLeaveRoom()
        {
            Result = (short)result,
            UserID = userID,
        };

        var sendData = GetBinaryPacketData(responseLogin, PacketId.ResDbLeaveRoom);

        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInReqDbLoadUserGameDataPacket(string sessionID, string userID)
    {
        var reqData = new PKTReqDbLoadUserGameData()
        {
            UserID = userID
        };

        var sendData = GetBinaryPacketData(reqData, PacketId.ReqDbLoadUserGameData);

        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInResDbLoadUserGameDataPacket(string sessionID, string userID, ErrorCode error, int winCount, int loseCount, int level, int exp)
    {
        var resData = new PKTResDbLoadUserGameData()
        {
            Result = (short)error,
            UserID = userID,
            WinCount = winCount,
            LoseCount = loseCount,
            Level = level,
            Exp = exp
        };

        var sendData = GetBinaryPacketData(resData, PacketId.ResDbLoadUserGameData);

        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInReqDbSaveUserGameDataPacket(string sessionID, string userID, int winCount, int loseCount, int level, int exp)
    {
        var reqData = new PKTReqDbSaveUserGameData()
        {
            UserID = userID,
            WinCount = winCount,
            LoseCount = loseCount,
            Level = level,
            Exp = exp
        };

        var sendData = GetBinaryPacketData(reqData, PacketId.ReqDbSaveUserGameData);

        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }

    public OmokBinaryRequestInfo MakeInResDbSaveUserGameDataPacket(string sessionID, ErrorCode result)
    {
        var resData = new PKTResDbSaveUserGameData()
        {
            Result = (short)result
        };

        var sendData = GetBinaryPacketData(resData, PacketId.ResDbSaveUserGameData);

        var innerPacket = new OmokBinaryRequestInfo
        {
            Data = sendData,
            SessionID = sessionID
        };

        return innerPacket;
    }
}
