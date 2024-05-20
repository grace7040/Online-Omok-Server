using System.ComponentModel.DataAnnotations;

namespace GameAPIServer.DTOs;

public class ReqUserAuthDTO : RequestDTO
{
    [Required]
    public string Token { get; set; } = "";
}
