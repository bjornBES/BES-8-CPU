using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public partial class Assembler
    {
        public class AssemblerObj
        {
            public static List<string> OBJString = new List<string>();

            public static void Start()
            {
                OBJString.Add("BESASMOBJ1".PadRight(0x10, '\0'));
                AddInstr("00000", "02455", "0FFFF");
                AddInstr("00000", "&TEST", "0FFFF");
                AddLabel("TEST", "0FF00");
            }

            public static void AddLabel(string Name, string Addr)
            {
                OBJString.Add($"{Name}::{Addr}&{CurrentFile}");
            }
            public static void AddInstr(string ident, params string[] args)
            {
                string FullArgs = "";
                for (int i = 0; i < args.Length; i++)
                {
                    FullArgs += args[i];
                }
                OBJString.Add($"{ident}{FullArgs}");
            }
        }
    }
}
