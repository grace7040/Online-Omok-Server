namespace GameAPIServer.DTOs;

public class ResMatchingDTO  : ResponseDTO
{ 
    public string OmokServerIP { get; set; } = "";
    public string OmokServerPort { get; set; } = "";
    public int RoomNumber { get; set; } = - 1;

    public bool IsMatchSucceed { get { return RoomNumber != -1; } }
}
