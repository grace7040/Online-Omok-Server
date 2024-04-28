﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//PacketType
public enum PacketId : int
{
    REQ_RES_TEST_ECHO = 101,
    

    // 클라이언트
    CS_BEGIN = 1001,

    REQ_LOGIN = 1002,
    RES_LOGIN = 1003,
    NTF_MUST_CLOSE = 1005,

    REQ_ROOM_ENTER = 1015,
    RES_ROOM_ENTER = 1016,
    NTF_ROOM_USER_LIST = 1017,
    NTF_ROOM_NEW_USER = 1018,

    REQ_ROOM_LEAVE = 1021,
    RES_ROOM_LEAVE = 1022,
    NTF_ROOM_LEAVE_USER = 1023,

    REQ_ROOM_CHAT = 1026,
    NTF_ROOM_CHAT = 1027,

    REQ_ROOM_READY = 1031,
    RES_GAME_READY = 1032,
    NTF_GAME_START = 1033,

    NTF_GAME_TURN = 1036,

    REQ_GAME_PUT_STONE = 1041,
    RES_GAME_PUT_STONE = 1042,

    REQ_ROOM_DEV_ALL_ROOM_START_GAME = 1091,
    RES_ROOM_DEV_ALL_ROOM_START_GAME = 1092,

    REQ_ROOM_DEV_ALL_ROOM_END_GAME = 1093,
    RES_ROOM_DEV_ALL_ROOM_END_GAME = 1094,

    CS_END = 1100,


    // 시스템, 서버 - 서버
    SS_START = 8001,

    INNTF_CONNECT_CLIENT = 8011,
    INNTF_DISCONNECT_CLIENT = 8012,

    REQ_SS_SERVERINFO = 8021,
    RES_SS_SERVERINFO = 8023,

    REQ_IN_ROOM_ENTER = 8031,
    RES_IN_ROOM_ENTER = 8032,

    NTF_IN_ROOM_LEAVE = 8036,


    // DB 8101 ~ 9000
    REQ_DB_LOGIN = 8101,
    RES_DB_LOGIN = 8102,
}
