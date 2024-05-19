using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class ServerOption
    {
        public int ServerUniqueID { get; set; }

        public string Name { get; set; } = "";

        public int MaxConnectionNumber { get; set; }

        public int CheckUserCount { get; set; }
        
        public string IP { get; set; } = "";
        public int Port { get; set; }

        public int MaxRequestLength { get; set; }

        public int ReceiveBufferSize { get; set; }

        public int SendBufferSize { get; set; }

        public int RoomMaxCount { get; set; } = 0;

        public int RoomMaxUserCount { get; set; } = 0;

        public int HeartBeatInterval { get; set; }
        public int RoomCheckInterval { get; set; }
        public int CheckRoomCount { get; set; }
        public int TurnTimeOut { get; set; }
        public int MaxGameTime { get; set; }
        public int MaxTurnOverCnt { get; set; }

        public int RoomStartNumber { get; set; } = 0;

        

        //public int DbThreadCount { get; set; }

        //public string DbConnectionString { get; set; }
        

        //public int RedisThreadCount { get; set; }

        //public string RedisConnectionString { get; set; }
        //public string UserRoomKey { get; set; } = "";
        //public string RequestMatchingKey { get; set; } = "";
        //public string CheckMatchingKey { get; set; } = "";

    }

    public class DbOption
    {
        public string DbConnectionString { get; set; }
        public int DbThreadCount { get; set; }

        
    }

    public class RedisOption
    {
        public string RedisConnectionString { get; set; }
        public int RedisThreadCount { get; set; }

        public string UserRoomKey { get; set; } = "";
        public string RequestMatchingKey { get; set; } = "";
        public string CheckMatchingKey { get; set; } = "";
    }
}
