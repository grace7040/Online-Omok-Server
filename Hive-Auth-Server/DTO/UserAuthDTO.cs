using System.ComponentModel.DataAnnotations;

namespace Hive_Auth_Server.DTO
{
    public class UserAuthDTO
    {
        [Required]
        [EmailAddress]
        [StringLength(50, ErrorMessage = "이메일은 50자까지 입력 가능합니다.")]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
