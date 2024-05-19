using System.ComponentModel.DataAnnotations;

namespace Game_API_Server.DTOs
{
    public class RequestDTO
    {
        [Required]
        [EmailAddress]
        public string Id { get; set; } = "";
    }
}


