using System.ComponentModel.DataAnnotations;

namespace Hive_Auth_Server.DTO
{
    public class UserAuthDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
