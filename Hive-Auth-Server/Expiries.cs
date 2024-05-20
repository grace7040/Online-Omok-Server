namespace HiveAuthServer
{
    public static class Expiries
    {
        static int loginToken = 10;
        public static TimeSpan LoginToken { get { return TimeSpan.FromMinutes(loginToken); } }
    }
}
