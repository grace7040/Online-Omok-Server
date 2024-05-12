using System.ComponentModel.DataAnnotations;

namespace Game_API_Server.DTOs
{
    public class ResMatchingDTO  : ResponseDTO
    {
        public string OmokServerIP { get; set; } = "";
        public string OmokServerPort { get; set; } = "";
        public int RoomNumber { get; set; }
    }
}
