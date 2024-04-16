using System.ComponentModel.DataAnnotations;

namespace Hive_Auth_Server.DTO
{
    public class AccountDTO
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(50, ErrorMessage = "이메일은 50자까지 입력 가능합니다.")]
        public string Email { get; set; }

        [Required]
        [StringLength(32, ErrorMessage = "비밀번호는 32자까지 설정 가능합니다.")]
        public string Password { get; set; }
    }
}
