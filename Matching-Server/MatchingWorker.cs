﻿using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace MatchingServer;

public interface IMatchingWorker : IDisposable
{
    public void AddUser(string userID);

    public (bool, MatchingData) GetMatchingData(string userID);
    public void RemoveUserFromMatchingDict(string userID, out bool result);
}

public class MatchingWorker : IMatchingWorker
{
    System.Threading.Thread _reqWorker = null;
    System.Threading.Thread _completeWorker = null;
    
    ConcurrentDictionary<string, MatchingData> _matchingDict = new(); // key는 유저ID
    ConcurrentQueue<string> _waitingQueue = new();
    ConcurrentQueue<string> _matchingQueue = new();

    string _requestMatchingKey;
    string _checkMatchingKey;
    RedisConnection _redisConnection;

    public MatchingWorker(IOptions<MatchingConfig> matchingConfig)
    {        
        var redisConnectString = matchingConfig.Value.RedisConnectionString;
        var redisConfig = new RedisConfig("MatchingRedis", redisConnectString!);
        _redisConnection = new RedisConnection(redisConfig);

        _requestMatchingKey = matchingConfig.Value.RequestMatchingKey;
        _checkMatchingKey = matchingConfig.Value.CheckMatchingKey;

        InitWorkers();
    }

    void InitWorkers()
    {
        _reqWorker = new System.Threading.Thread(this.RunMatching);
        _reqWorker.Start();

        _completeWorker = new System.Threading.Thread(this.RunMatchingComplete);
        _completeWorker.Start();
    }
    
    public void AddUser(string userID)
    {
        if (!_matchingDict.ContainsKey(userID))
        {
            var userData = new MatchingData
            {
                OmokServerIP = "",
                OmokServerPort = "",
                RoomNumber = -1,
                State = MatchingState.Waiting
            };
            _waitingQueue.Enqueue(userID);
            _matchingDict.TryAdd(userID, userData);
        }
        
    }

    public (bool, MatchingData) GetMatchingData(string userID)
    {
        if(_matchingDict.TryGetValue(userID, out var data))
        {
            if(data.State == MatchingState.Complete)
            {
                return (true, data);
            }
        }

        return (false, null);
    }

    public void RemoveUserFromMatchingDict(string userID, out bool result)
    {
        result = _matchingDict.TryRemove(userID, out var data);
    }

    //매칭을 기다리는 유저가 2명 이상인 경우 redis에 매칭 요청을 추가하는 스레드
    void RunMatching()
    {
        while (true)
        {
            try
            {
                if (_waitingQueue.Count < 2)
                {
                    System.Threading.Thread.Sleep(1);
                    continue;
                }

                var canMatching = GetWaitingTwoUsers(out var user1, out var user2);
                if (!canMatching)
                {
                    continue;
                }

                StartMatching(user1, user2);
                PushMatchingRequestToRedis();
            }
            catch (Exception ex)
            {
                // ::TODO:: 예외처리
            }
        }
    }

    bool GetWaitingTwoUsers(out string u1, out string u2)
    {
        u1 = "";
        u2 = "";
        _waitingQueue.TryDequeue(out var user1);
        if (!_matchingDict.ContainsKey(user1))
        {
            return false;
        }

        _waitingQueue.TryDequeue(out var user2);
        if (!_matchingDict.ContainsKey(user2))
        {
            _waitingQueue.Enqueue(user1);
            return false;
        }

        u1 = user1;
        u2 = user2;
        return true;
    }

    void StartMatching(string user1, string user2)
    {
        _matchingQueue.Enqueue(user1);
        _matchingDict[user1].State = MatchingState.Matching;

        _matchingQueue.Enqueue(user2);
        _matchingDict[user2].State = MatchingState.Matching;
    }

    //redis로부터 방배정 결과를 받아와서 실제 매칭을 진행하는 스레드
    void RunMatchingComplete()
    {
        while (true)
        {
            try
            {
                var (isEmptyRoomExist, completeMatchingData) = TryGetEmptyRoomInfoFromRedis();
                if (isEmptyRoomExist)
                {
                    var ip = completeMatchingData.OmokServerIP;
                    var port = completeMatchingData.OmokServerPort;
                    var roomNumber = completeMatchingData.RoomNumber;

                    MakeCompleteMatching(ip, port, roomNumber);
                }
                else
                {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }
            }
            catch (Exception ex)
            {
                // ::TODO:: 예외처리
            }
        }        
    }

    void MakeCompleteMatching(string ip, string port, int roomNumber)
    {
        _matchingQueue.TryDequeue(out var user1);
        _matchingQueue.TryDequeue(out var user2);

        if(user1 == null || user2 == null)
        {
            // ::TODO:: TryDequeue 실패 처리
            return;
        }

        UpdateMatchingDict(user1, MatchingState.Complete, ip, port, roomNumber);
        UpdateMatchingDict(user2, MatchingState.Complete, ip, port, roomNumber);

        return;
    }

    void UpdateMatchingDict(string user, MatchingState state, string ip, string port, int roomNumber)
    {
        _matchingDict[user].State = state;
        _matchingDict[user].OmokServerIP = ip;
        _matchingDict[user].OmokServerPort = port;
        _matchingDict[user].RoomNumber = roomNumber;
    }

    public void Dispose()
    {
        // ::TODO:: MatchWorker Dispose
    }

    void PushMatchingRequestToRedis()
    {
        var query = new RedisList<int>(_redisConnection, _requestMatchingKey, null);
        var result = query.RightPushAsync(1).Result;
        if(result != 1)
        {
            // ::TODO:: ErrorCode.AddUserToMatchingQueueFailException
        }
    }

    (bool, EmptyRoomInfo) TryGetEmptyRoomInfoFromRedis()
    {
        var query = new RedisList<EmptyRoomInfo>(_redisConnection, _checkMatchingKey, null);
        var result = query.RightPopAsync().Result;
        if (result.HasValue)
        {
            return (true, result.Value);
        }

        return (false, null);
    }
}