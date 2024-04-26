using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Omok_Server
{
    public class Room
    {
        public const int InvalidRoomNumber = -1;
        public int Number { get; private set; }

        int _maxUserCount = 0;

        static Func<string, byte[], bool> SendFunc;

        public void Init(int number, int maxUserCount)
        {
            Number = number;
            _maxUserCount = maxUserCount;
        }
        public static void SetSendFunc(Func<string, byte[], bool> func)
        {
            SendFunc = func;
        }
    }
}
