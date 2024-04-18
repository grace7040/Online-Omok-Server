using Hive_Auth_Server.DTO;
using System.ComponentModel.DataAnnotations;

namespace Hive_Auth_Server.DTOs
{
    public class ResUserAuthDTO : ResponseDTO
    {
        public string Token { get; set; } = "";
    }
}
