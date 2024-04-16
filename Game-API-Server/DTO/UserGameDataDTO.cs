using System.ComponentModel.DataAnnotations;

namespace Game_API_Server.DTO
{
    public class UserGameDataDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public int Level { get; set; }
        public int exp { get; set; }
        public int win_count {  get; set; }
        public int lose_count {  get; set; }
    }
}
