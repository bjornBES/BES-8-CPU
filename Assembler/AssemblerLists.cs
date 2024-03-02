using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public partial class Assembler
    {
        public static class AssemblerLists
        {
            public static List<Macros> Macros = new(1000);
            public static List<Lable> lables = new();
            public static List<Lable> GlobalLables = new();
            public static List<Variables> Variables = new();
            public static List<Variables> ImmVariables = new();
            public static List<Variables> PointerVariables = new();
            public static string[] MCcode = new string[0xFFFFF + 1]; // 0xFFFFF + 1
            public static List<string> Tokens = new();
            public static Dictionary<string, string> keyPattens = new();
            public static Dictionary<string, ushort> Registers = new();
            public static Dictionary<string, string> ArgumentIdentifier = new Dictionary<string, string>();

            public static void Start()
            {
                Array.Fill(MCcode, "00000", 0x0,MCcode.Length);

                ArgumentIdentifier.Add("imm", "0");             // #number
                ArgumentIdentifier.Add("addr", "1");            // [addr]
                ArgumentIdentifier.Add("reg", "2");             // reg  
                ArgumentIdentifier.Add("RegAddr", "3");         // [reg]
                ArgumentIdentifier.Add("IndexReg", "4");        // [addr]&reg
                ArgumentIdentifier.Add("IndexImm", "5");        // [addr]&number
                ArgumentIdentifier.Add("IndexRegAddrReg", "6"); // [reg]&reg
                ArgumentIdentifier.Add("IndexRegAddrImm", "7"); // [reg]&imm

                Registers.Add("AX", 0x00000); Registers.Add("AL", 0x08000); Registers.Add("AH", 0x04000);
                Registers.Add("BX", 0x00001); Registers.Add("BL", 0x08001); Registers.Add("BH", 0x04001);
                Registers.Add("CX", 0x00002); Registers.Add("CL", 0x08002); Registers.Add("CH", 0x04002);
                Registers.Add("DX", 0x00003); Registers.Add("DL", 0x08003); Registers.Add("DH", 0x04003);
                Registers.Add("ZX", 0x00004); Registers.Add("ZL", 0x08004); Registers.Add("ZH", 0x04004);
                Registers.Add("PC", 0x00005);
                Registers.Add("SP", 0x00006);
                Registers.Add("MB", 0x00007);

                Registers.Add("X", 0x00008); Registers.Add("XL", 0x08008); Registers.Add("XH", 0x04008);
                Registers.Add("Y", 0x00009); Registers.Add("YL", 0x08009); Registers.Add("YH", 0x04009);

                Registers.Add("F", 0x0000F);

                Registers.Add("R1", 0x00010); Registers.Add("R1L", 0x08010); Registers.Add("R1H", 0x04010);
                Registers.Add("R2", 0x00011); Registers.Add("R2L", 0x08011); Registers.Add("R2H", 0x04011);
                Registers.Add("R3", 0x00012); Registers.Add("R3L", 0x08012); Registers.Add("R3H", 0x04012);
                Registers.Add("R4", 0x00013); Registers.Add("R4L", 0x08013); Registers.Add("R4H", 0x04013);

                keyPattens.Add("NULL", "#00");
                keyPattens.Add("KW_ENT", "#Dh");
                keyPattens.Add("KW_ESC", "#1Bh");
                keyPattens.Add("KW_BS", "#8h");
                keyPattens.Add("KW_SP", "#20h");
            }
        }
    }
}