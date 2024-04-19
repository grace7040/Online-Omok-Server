using System.ComponentModel.DataAnnotations;

namespace Game_API_Server.DTOs
{
    public class ReqUserAuthDTO : RequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string Token { get; set; } = "";
    }
}
