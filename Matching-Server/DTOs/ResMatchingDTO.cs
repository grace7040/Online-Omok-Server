namespace MatchingServer.DTOs; 


public class ResMatchingDTO
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string OmokServerIP { get; set; } = "";
    public string OmokServerPort { get; set; } = "";
    public int RoomNumber { get; set; } = -1;

}


