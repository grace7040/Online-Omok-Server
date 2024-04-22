using System.ComponentModel.DataAnnotations;

namespace Game_API_Server.DTOs
{
    public class ReqUserAuthDTO : RequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Token { get; set; } = "";
    }
}
