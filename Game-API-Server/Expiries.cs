﻿namespace Hive_Auth_Server
{
    public static class Expiries
    {
        static int loginToken = 6;
        public static TimeSpan LoginToken { get { return TimeSpan.FromHours(loginToken); } }
    }
}
