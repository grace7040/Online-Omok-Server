using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omok_Server
{
    // ::TODO:: 표기법 수정 (snake_case -> camelCase)
    public class UserGameData
    {
        public string Id { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public int Win_Count { get; set; }
        public int Lose_Count { get; set; }

    }
}
