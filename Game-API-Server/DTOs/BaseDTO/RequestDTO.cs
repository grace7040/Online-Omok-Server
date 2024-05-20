using System.ComponentModel.DataAnnotations;

namespace GameAPIServer.DTOs;

public class RequestDTO
{
    [Required]
    [EmailAddress]
    public string Id { get; set; } = "";
}


