using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public class Token
    {
        public TokenType Type;
        public string Value = "NULL";
        public int Line;
        public string File;
    }
}
