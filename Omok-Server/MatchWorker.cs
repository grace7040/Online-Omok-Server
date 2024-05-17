using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class MatchWorker
    {
        string _requestMatchingKey;
        string _checkMatchingKey;
        string _serverIP;
        int _serverPort;
        RedisConnection _redisConnection;

        System.Threading.Thread _checkMatchingQueueWorker = null;

        Queue<int> _emptyRoomQueue = new();
        public MatchWorker(string redisConnectString, string requestMatchingKey, string checkMatchingKey, string serverIP, int serverPort)
        {
            Console.WriteLine("MatchWoker 생성자 호출");

            var redisConfig = new RedisConfig("MatchingRedis", redisConnectString!);
            _redisConnection = new RedisConnection(redisConfig);

            _requestMatchingKey = requestMatchingKey;
            _checkMatchingKey = checkMatchingKey;

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
                    Thread.Sleep(1);
                    continue;
                }

                if (!IsMatchingRequestReceived())
                {
                    Thread.Sleep(1);
                    continue;
                }

                var roomNumber = _emptyRoomQueue.Dequeue();
                var roomInfo = new EmptyRoomInfo
                {
                    OmokServerIP = _serverIP,
                    OmokServerPort = _serverPort.ToString(),
                    RoomNumber = roomNumber
                };

                var query = new RedisList<EmptyRoomInfo>(_redisConnection, _checkMatchingKey, null);
                var result = query.RightPushAsync(roomInfo).Result;

                if (result != 1)
                {
                    // ::TODO:: redis Push 실패
                    continue;
                }
            }
        }

        bool IsMatchingRequestReceived()
        {
            var query = new RedisList<int>(_redisConnection, _requestMatchingKey, null);
            var result = query.RightPopAsync().Result;

            return result.HasValue;
        }
    }
    public class EmptyRoomInfo
    {
        public string OmokServerIP { get; set; } = "";
        public string OmokServerPort { get; set; } = "";
        public int RoomNumber { get; set; }
    }
}
