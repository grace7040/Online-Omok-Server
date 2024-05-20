using System.ComponentModel.DataAnnotations;

namespace HiveAuthServer.DTOs;

public class ReqUserAuthDTO : RequestDTO
{
    [Required]
    [EmailAddress]
    public string Id { get; set; } = "";

    [Required]
    public string Token { get; set; } = "";
}
