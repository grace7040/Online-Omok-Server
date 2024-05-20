using SuperSocket.SocketBase.Logging;

namespace OmokServer;

public class PKHandler
{
    public static Func<string, byte[], bool> SendFunc;
    public static Action<OmokBinaryRequestInfo> DIstributePacketAction;
    public static Action<string> CloseSessionAction;

    protected ILog _mainLogger;
    protected UserManager _userMgr;
    protected RoomManager _roomMgr;
    protected PacketManager<MemoryPackBinaryPacketDataCreator> _packetMgr = new();

    public void Init(ILog logger, UserManager userMgr, RoomManager roomMgr)
    {
        this._userMgr = userMgr;
        this._roomMgr = roomMgr;
        this._mainLogger = logger;
    }

}

