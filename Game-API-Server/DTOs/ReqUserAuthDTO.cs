using System.ComponentModel.DataAnnotations;

namespace Game_API_Server.DTOs
{
    public class ReqUserAuthDTO : RequestDTO
    {
        [Required]
        public string Token { get; set; } = "";
    }
}
