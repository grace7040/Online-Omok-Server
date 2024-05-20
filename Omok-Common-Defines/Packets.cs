using MemoryPack;

[MemoryPackable]
public partial class PKHeader
{
    public UInt16 TotalSize { get; set; } = 0;
    public UInt16 Id { get; set; } = 0;
    public byte Type { get; set; } = 0;
}

[MemoryPackable]
public partial class PKTResConnect : PKHeader
{
    public short Result { get; set; }
}

// 로그인 요청
[MemoryPackable]
public partial class PKTReqLogin : PKHeader
{
    public string UserID { get; set; }
    public string AuthToken { get; set; }
}

[MemoryPackable]
public partial class PKTReqDbLogin : PKHeader
{
    public string UserID { get; set; }
    public string AuthToken { get; set; }
}


[MemoryPackable]
public partial class PKTResLogin : PKHeader
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTResInLogin : PKHeader
{
    public string UserID { get; set; }
    public short Result { get; set; }
    
}



// 로그아웃 요청
[MemoryPackable]
public partial class PKNtfMustClose : PKHeader
{
    public short Result { get; set; }
}



[MemoryPackable]
public partial class PKTReqRoomEnter : PKHeader
{
    public int RoomNumber { get; set; }
}

[MemoryPackable]
public partial class PKTResRoomEnter : PKHeader
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTNtfRoomUserList : PKHeader
{
    public List<string> UserIDList { get; set; } = new List<string>();
}

[MemoryPackable]
public partial class PKTNtfRoomNewUser : PKHeader
{
    public string UserID { get; set; }
}


[MemoryPackable]
public partial class PKTReqRoomLeave : PKHeader
{
    public int RoomNumber { get; set; }
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTReqDbLeaveRoom : PKHeader
{
    public int RoomNumber { get; set; }
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTResRoomLeave : PKHeader
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTResDbLeaveRoom : PKHeader
{
    public int RoomNumber { get; set; }
    public string UserID { get; set; }
    public short Result { get; set; }
}


[MemoryPackable]
public partial class PKTNtfRoomLeaveUser : PKHeader
{
    public string UserID { get; set; }
}


[MemoryPackable]
public partial class PKTReqRoomChat : PKHeader
{
    public string ChatMessage { get; set; }
}


[MemoryPackable]
public partial class PKTNtfRoomChat : PKHeader
{
    public string UserID { get; set; }

    public string ChatMessage { get; set; }
}

[MemoryPackable]
public partial class PKTReqGameReady : PKHeader
{

}

[MemoryPackable]
public partial class PKTResGameReady : PKHeader
{
    public RoomUserState State { get; set; }
}

[MemoryPackable]
public partial class PKTNtfGameStart : PKHeader
{
    public StoneColor MyStoneColor { get; set; }
}

[MemoryPackable]
public partial class PKTNtfPutStone : PKHeader
{
    public Tuple<int, int>? Position { get; set; }
}

[MemoryPackable]
public partial class PKTReqPutStone : PKHeader
{
    public Tuple<int, int> Position { get; set; }

}

[MemoryPackable]
public partial class PKTResPutStone : PKHeader
{
    public short Result;

}

[MemoryPackable]
public partial class PKTNtfTurnOver : PKHeader
{

}

[MemoryPackable]
public partial class PKTNtfGameEnd : PKHeader
{
    public StoneColor WinnerColor { get; set; }

}

[MemoryPackable]
public partial class PKTReqHeartBeat : PKHeader
{

}

[MemoryPackable]
public partial class PKTResHeartBeat : PKHeader
{

}


[MemoryPackable]
public partial class PKTReqDbLoadUserGameData : PKHeader
{
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTResDbLoadUserGameData : PKHeader
{
    public string UserID { get; set; }
    public short Result { get; set; }
    public int WinCount { get; set; }
    public int LoseCount { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
}

[MemoryPackable]
public partial class PKTResLoadUserGameData : PKHeader
{
    public short Result { get; set; }
    public int WinCount { get; set; }
    public int LoseCount { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
}

[MemoryPackable]
public partial class PKTReqDbSaveUserGameData : PKHeader
{
    public string UserID { get; set; }
    public int WinCount { get; set; }
    public int LoseCount { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
}


[MemoryPackable]
public partial class PKTResDbSaveUserGameData : PKHeader
{
    public string UserID { get; set; }
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTResSaveUserGameData : PKHeader
{
    public short Result { get; set; }
}

[MemoryPackable]
public partial class PKTReqDbCancelMatching : PKHeader
{
    public string UserID { get; set; }
}

[MemoryPackable]
public partial class PKTResDbCancelMatching : PKHeader
{
    public string UserID { get; set; }
    public short Result { get; set; }
}
