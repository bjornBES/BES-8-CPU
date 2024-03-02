using Microsoft.VisualBasic;
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
            public static List<byte> OBJString = new List<byte>();
            public static List<byte> SymbolTable = new List<byte>();

            public static void Start()
            {
                InsertBytes(ConvertToBytes("BESASMOBJ1\0BC8\016".PadRight(0x20, '.')));
            }

            public static byte[] GetOBJ()
            {
                List<byte> ret = new List<byte>();
                ret.AddRange(OBJString);
                ret.AddRange(ConvertToBytes("00FF0055AA"));
                ret.AddRange(SymbolTable);
                return ret.ToArray();
            }

            public static void AddWord(string Data)
            {
                if(Data.StartsWith(AssemblerConst.Assembler_Lable_Ident))
                {
                    InsertBytes(ConvertToBytes($"&{Data.Remove(0,2)}"));
                }
                else
                {
                    InsertBytes(ConvertToBytes($"{Data}"));
                }
            }

            public static void AddBytes(byte[] ByteStr)
            {
                InsertBytes(ByteStr);
            }

            public static void AddStr(string Data)
            {
                InsertBytes(ConvertToBytes($"{Data}"));
            }

            public static void AddOrg(string Address)
            {
                InsertBytes(ConvertToBytes($"OS:"));
                InsertBytes(ConvertToBytes($"{Address}"));
            }

            public static void AddLabel(string Name, string Addr)
            {
                InsertBytesSymbol(ConvertToBytes($"&{Name}:{Addr}"));
            }
            public static void AddInstr(params string[] args)
            {
                FormatAndPlace(args[0]);
                for (int i = 1; i < args.Length; i++)
                {
                    if (!args[i].StartsWith(AssemblerConst.Assembler_Lable_Ident))
                    {
                        FormatAndPlace(args[i]);
                    }
                    else if (args[i].StartsWith(AssemblerConst.Assembler_Lable_Ident))
                    {
                        args[i] = args[i].Remove(0, 2);
                        args[i] = args[i].Split('+')[0];
                        args[i] = "&" + args[i];
                        InsertBytes(ConvertToBytes(args[i]));
                    }
                }
            }

            static void InsertBytes(byte[] bytes)
            {
                OBJString.InsertRange(OBJString.Count, bytes);
            }
            static void InsertBytesSymbol(byte[] bytes)
            {
                SymbolTable.InsertRange(SymbolTable.Count, bytes);
            }

            static byte[] ConvertToBytes(string text)
            {
                if (text == null) return new byte[0];
                return Encoding.ASCII.GetBytes(text);
            }

            static void FormatAndPlace(string Hexvalue)
            {
                byte[] formated = Format(Hexvalue);
                for (int i = 0; i < formated.Length; i++)
                {
                    OBJString.Add(formated[i]);
                }
            }
            static void FormatAndPlaceInSymbol(string Hexvalue)
            {
                byte[] formated = Format(Hexvalue);
                for (int i = 0; i < formated.Length; i++)
                {
                    SymbolTable.Add(formated[i]);
                }
            }

            static byte[] Format(string HexValue)
            {
                HexValue = HexValue.PadLeft(6, '0');

                List<byte> Result = new List<byte>();

                for (int i = 0; i < HexValue.Length; i += 2)
                {
                    string formatString = HexValue[i].ToString() + HexValue[i + 1].ToString();
                    byte result = Convert.ToByte(formatString, 16);
                    Result.Add(result);
                }

                return Result.ToArray();
            }
        }
    }
}
