using System.ComponentModel.DataAnnotations;

namespace HiveAuthServer.DTOs;

public class ReqAccountDTO : RequestDTO
{
    [Required]
    [EmailAddress]
    [StringLength(50, ErrorMessage = "Id(이메일)은 50자까지 입력 가능합니다.")]
    public string Id { get; set; } = "";

    [Required]
    [StringLength(32, ErrorMessage = "비밀번호는 32자까지 설정 가능합니다.")]
    public string Password { get; set; } = "";
}
