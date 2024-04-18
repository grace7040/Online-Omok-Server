using System.ComponentModel.DataAnnotations;

namespace Hive_Auth_Server.DTO
{
    public class ReqUserAuthDTO : RequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string Token { get; set; } = "";
    }
}
