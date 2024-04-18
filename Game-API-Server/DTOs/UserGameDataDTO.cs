using System.ComponentModel.DataAnnotations;

namespace Game_API_Server.DTO
{
    public class UserGameDataDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public int Level { get; set; }
        public int Exp { get; set; }
        public int WinCount {  get; set; }
        public int LoseCount {  get; set; }
    }
}
