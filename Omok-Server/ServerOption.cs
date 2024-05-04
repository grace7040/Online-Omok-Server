﻿using System;
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

        public int Port { get; set; }

        public int MaxRequestLength { get; set; }

        public int ReceiveBufferSize { get; set; }

        public int SendBufferSize { get; set; }

        public int RoomMaxCount { get; set; } = 0;

        public int RoomMaxUserCount { get; set; } = 0;

        public int RoomStartNumber { get; set; } = 0;

        public int DBThreadCount { get; set; }

        public string RedisConnectionString { get; set; }

        public string DBConnectionString { get; set; }
        public int HeartBeatInterval { get; set; }
    }
}
