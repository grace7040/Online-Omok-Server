using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//PacketType
public enum PacketId : int
{
    ReqResTestEcho = 101,

    ReqHeartBeat = 102,
    ResHeartBeat = 103,

    // 클라이언트
    CsBegin = 1001,

    ReqLogin = 1002,
    ResLogin = 1003,
    NtfMustClose = 1005,

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
    NtfGameENd = 1047,

    ReqRoomDevAllRoomStartGame = 1091,
    ResRoomDevAllRoomStartGame = 1092,

    ReqRoomDevAllRoomEndGame = 1093,
    ResRoomDevAllRoomEndGame = 1094,

    CsEnd = 1100,

    // 시스템, 서버 - 서버
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

    


    // DB 8101 ~ 9000
    ReqDbLogin = 8101,
    ResDbLogin = 8102,
}
