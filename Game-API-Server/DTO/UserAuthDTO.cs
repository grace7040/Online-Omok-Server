using System.ComponentModel.DataAnnotations;

namespace Game_API_Server.DTO
{
    public class UserAuthDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Token { get; set; } = "";
    }
}
