﻿using CloudStructures;
using CloudStructures.Structures;

namespace OmokServer;

public class MatchWorker
{
    string _requestMatchingKey;
    string _checkMatchingKey;
    string _serverIP;
    int _serverPort;
    RedisConnection _redisConnection;

    System.Threading.Thread _checkMatchingQueueWorker = null;

    Queue<int> _emptyRoomQueue = new();
    public MatchWorker(RedisOption redisOption, string serverIP, int serverPort)
    {
        var redisConnectString = redisOption.RedisConnectionString;
        var redisConfig = new RedisConfig("MatchingRedis", redisConnectString!);
        _redisConnection = new RedisConnection(redisConfig);

        _requestMatchingKey = redisOption.RequestMatchingKey;
        _checkMatchingKey = redisOption.CheckMatchingKey;

        _checkMatchingQueueWorker = new System.Threading.Thread(this.CheckMatchingQueue);
        _checkMatchingQueueWorker.Start();
        _serverIP = serverIP;
        _serverPort = serverPort;
    }

    public void AddEmptyRoom(int roomNumber)
    {
        _emptyRoomQueue.Enqueue(roomNumber);
    }

    void CheckMatchingQueue()
    {
        while (true)
        {
            if (_emptyRoomQueue.Count < 1)
            {
                Thread.Sleep(1000);
                continue;
            }

            if (!IsMatchingRequestReceived())
            {
                Thread.Sleep(500);
                continue;
            }

            GetEmptyRoomInfo(out var roomInfo);
            PushEmptyRoomInfoToRedis(roomInfo);
        }
    }

    bool IsMatchingRequestReceived()
    {
        var query = new RedisList<int>(_redisConnection, _requestMatchingKey, null);
        var result = query.RightPopAsync().Result;

        return result.HasValue;
    }
    
    void GetEmptyRoomInfo(out EmptyRoomInfo roomInfo)
    {
        var roomNumber = _emptyRoomQueue.Dequeue();
        roomInfo = new EmptyRoomInfo
        {
            OmokServerIP = _serverIP,
            OmokServerPort = _serverPort.ToString(),
            RoomNumber = roomNumber
        };
    }

    void PushEmptyRoomInfoToRedis(EmptyRoomInfo roomInfo)
    {
        var query = new RedisList<EmptyRoomInfo>(_redisConnection, _checkMatchingKey, null);
        var result = query.RightPushAsync(roomInfo).Result;

        if (result != 1)
        {
            // ::TODO:: redis Push 실패
            return;
        }
    }
}
public class EmptyRoomInfo
{
    public string OmokServerIP { get; set; } = "";
    public string OmokServerPort { get; set; } = "";
    public int RoomNumber { get; set; }
}
