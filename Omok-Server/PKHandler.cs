using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    public class PKHandler
    {
        public static Func<string, byte[], bool> SendFunc;
        public static Action<OmokBinaryRequestInfo> DIstributePacketAction;

        protected UserManager _userMgr;
        protected PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();
        protected RoomManager _roomMgr;

        public void Init(UserManager userMgr, RoomManager roomMgr)
        {
            this._userMgr = userMgr;
            this._roomMgr = roomMgr;
        }

    }
    
}
