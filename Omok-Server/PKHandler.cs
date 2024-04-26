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

        protected UserManager _userMgr = null;
        protected PacketManager<MemoryPackBinaryPacketDataCreater> _packetMgr = new();

        public void Init(UserManager userMgr)
        {
            this._userMgr = userMgr;
        }

    }
    
}
