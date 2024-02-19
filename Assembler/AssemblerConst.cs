using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public partial class Assembler
    {
        public static class AssemblerConst
        {
            public const string Assembler_Lable_Ident = "L:";

            public const string Assembler_Keyword_long = "long";
            public const string Assembler_Keyword_far = "far";

            public const uint far_Offset = 0x10000;
            public const uint long_Offset = 0x20000;
        }
    }
}