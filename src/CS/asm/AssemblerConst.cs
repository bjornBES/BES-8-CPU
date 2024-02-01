using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asm
{
    public class AssemblerConst : AssemblerErrors
    {
        public const string Assembler_Keyword_CodeSection = "TEXT";
        public const string Assembler_Keyword_DataSection = "DATA";

        public const string Assembler_Keyword_long = "long";
        public const string Assembler_Keyword_far = "far";

        public const uint far_Offset = 0x10000;  
        public const uint long_Offset = 0x20000;  
    }
}
