﻿using MemoryPack;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    //패킷 직렬화 방식에 따른 패킷 관리자
    public class PacketManager<TPacketDataCreator> where TPacketDataCreator : IBinaryPacketDataCreator, new()
    {
        TPacketDataCreator _dataCreator = new();

        public OmokBinaryRequestInfo MakeInNTFConnectOrDisConnectClientPacket(bool isConnect, string sessionID)
        {
            OmokBinaryRequestInfo innerPacket = new OmokBinaryRequestInfo();

            if (isConnect) {
                innerPacket.Data = _dataCreator.PacketIdToBinary(PacketId.NtfInConnectClient);
            }
            else {
                innerPacket.Data = _dataCreator.PacketIdToBinary(PacketId.NtfInDisconnectClient);
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

            var innerPacket = new OmokBinaryRequestInfo();
            innerPacket.Data = sendData;
            innerPacket.SessionID = sessionID;
            return innerPacket;
        }

        public OmokBinaryRequestInfo MakeInReqHeartBeatPacket()
        {
            var packet = new PKTReqInHeartBeat();
            var sendData = GetBinaryPacketData(packet, PacketId.ReqInHeartBeat);

            var innerPacket = new OmokBinaryRequestInfo();
            innerPacket.Data = sendData;
            return innerPacket;
        }

        public byte[] GetBinaryPacketData<T>(T pkHeader, PacketId packetId) where T : PKHeader
        {
            byte[] sendData = _dataCreator.PacketDataToBinary(pkHeader, packetId);

            return sendData;
        }

        public T GetPacketData<T>(byte[] binaryPacketData) where T : PKHeader
        {
            return _dataCreator.BinaryToPacketData<T>(binaryPacketData);
        }
    }
}
