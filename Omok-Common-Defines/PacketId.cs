using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//PacketType
public enum PacketId : int
{
    // 클라이언트와 통신 1001 ~ 1100
    CsBegin = 1001,

    ReqLogin = 1002,
    ResLogin = 1003,

    NtfMustClose = 1005,

    ResLoadUserGameData = 1007,

    ReqHeartBeat = 1009,
    ResHeartBeat = 1010,

    ReqRoomEnter = 1015,
    ResRoomEnter = 1016,
    NtfRoomUserList = 1017,
    NtfRoomNewUser = 1018,

    ReqRoomLeave = 1021,
    ResRoomLeave = 1022,
    NtfRoomLeaveUser = 1023,

    ReqRoomChat = 1026,
    NtfRoomChat = 1027,

    ReqGameReady = 1031,
    ResGameReady = 1032,
    NtfGameStart = 1033,

    ReqPutStone = 1041,
    ResPutStone = 1042,
    NtfPutStone = 1043,

    NtfTurnOver = 1046,
    NtfGameEnd = 1047,


    CsEnd = 1100,



    // 서버 간 통신 or 서버 내부 패킷 8001 ~ 8200
    SsStart = 8001,

    NtfInConnectClient = 8011,
    NtfInDisConnectClient = 8012,
    ReqInDisConnectUser = 8013,

    ReqSsServerInfo = 8021,
    ResSsServerInfo = 8023,

    ReqInRoomEnter = 8031,
    ResInRoomEnter = 8032,

    NtfInRoomLeave = 8036,

    ReqInHeartBeat = 8041,

    ReqInRoomCheck = 8046,

    // DB 요청 8101 ~ 8200
    ReqDbLogin = 8101,
    ResDbLogin = 8102,

    ReqDbLoadUserGameData = 8106,
    ResDbLoadUserGameData = 8107,
    
    ReqDbSaveUserGameData = 8110,
    ResDbSaveUserGameData = 8111,
    ResDbLeaveRoom = 8112,
    ReqDbLeaveRoom = 8113,

    SsEnd = 8200,
}
