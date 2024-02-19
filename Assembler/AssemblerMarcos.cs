using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public partial class Assembler
    {
        public static class AssemblerMarcos
        {
            public static string ConvTo(int Base, string value, int FromBase)
            {
                value = value.Trim();
                return Convert.ToString(Convert.ToUInt16(value, FromBase), Base).PadLeft(5, '0');
            }
            public static string ConvTo32(int Base, string value, int FromBase)
            {
                Console.WriteLine("TO " + Base + " from " + FromBase + " value " + value);
                value = value.Trim();
                return Convert.ToString(Convert.ToUInt32(value, FromBase), Base).PadLeft(5, '0');
            }
            public static string ConvFrom(ref string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    AssemblerErrors.ErrorArgIsNull();
                }
                value = value.Trim();
                if (value.Last() == 'h')
                {
                    value = value.TrimEnd('h').PadLeft(5, '0');
                    return value;
                }
                else if (value.Last() == 'b')
                {
                    value = value.TrimEnd('b');
                    value = ConvTo32(16, value, 2).PadLeft(5, '0');
                    return value;
                }
                else if (value.StartsWith('['))
                {
                    value = CheakAddr(value, false);
                }
                else
                {
                    value = ConvTo32(16, value, 10).PadLeft(5, '0');
                    return value;
                }
                value = null;
                return null;
            }
            public static string CheakAddr(string arg, bool InAssembler = true)
            {
                arg = arg.Replace("[", "");
                arg = arg.Replace("]", "");

                // [REG]        REG
                if (AssemblerLists.Registers.TryGetValue(arg, out byte reg))
                {
                    string RegValue = ConvTo(16, reg.ToString().PadLeft(6, '0'), 10).PadLeft(4, '0');
                    if (InAssembler)
                    {
                        OutSrc[LineIndex] += RegValue + " ";
                        AssemblerLists.MCcode[PC] = RegValue;
                        if (AssemblerInstructions.AM == "F")
                            AssemblerInstructions.AM = AssemblerLists.ArgumentIdentifier["RegAddr"];
                        else
                            AssemblerInstructions.AM1 = AssemblerLists.ArgumentIdentifier["RegAddr"];
                        PC++;
                        AssemblerInstructions.offset++;
                        return "1";
                    }
                    else
                    {
                        return RegValue;
                    }
                }
                // [NUMBERh]    0-9 A-F
                // [NUMBER]     0-9
                // [NUMBERb]    0-1
                else if (IsHex(arg) || IsBin(arg) || IsDec(arg))
                {
                    if (InAssembler)
                    {
                        if (AssemblerInstructions.AM == "F")
                            AssemblerInstructions.AM = AssemblerLists.ArgumentIdentifier["addr"];
                        else
                            AssemblerInstructions.AM1 = AssemblerLists.ArgumentIdentifier["addr"];

                        char EndChar = arg[^1];
                        string Value = ConvFrom(ref arg);

                        AssemblerLists.MCcode[PC] = Value;
                        //TODO OBJBuffer += Value;
                        PC++;
                        arg = arg.TrimEnd('h', 'b');
                        int BASE = 10;
                        if (EndChar == 'h') BASE = 16;
                        if (EndChar == 'b') BASE = 2;
                        OutSrc[LineIndex] += ConvTo32(16, arg, BASE).PadLeft(5, '0') + " ";
                        AssemblerInstructions.offset++;
                        return "1";
                    }
                    else
                    {
                        char EndChar = arg[^1];
                        string Value = ConvFrom(ref arg);

                        AssemblerLists.MCcode[PC] = Value;
                        //TODO OBJBuffer += Value;
                        PC++;
                        arg = arg.TrimEnd('h', 'b');
                        int BASE = 10;
                        if (EndChar == 'h') BASE = 16;
                        if (EndChar == 'b') BASE = 2;
                        return ConvTo32(16, arg, BASE).PadLeft(5, '0');
                    }
                }
                else if (arg.StartsWith('%'))
                {
                    if (InAssembler)
                    {
                        string Addr = Convert.ToString(UseVariables(arg, out string NEWAM), 16) + "h";

                        if (AssemblerInstructions.AM == "F")
                            AssemblerInstructions.AM = NEWAM;
                        else
                            AssemblerInstructions.AM1 = NEWAM;

                        string AddrValue = ConvFrom(ref Addr);

                        AssemblerLists.MCcode[PC] = AddrValue;
                        //OBJBuffer += AddrValue;
                        PC++;

                        Addr = Addr.TrimEnd('h', 'b');
                        int BASE = 16;

                        OutSrc[LineIndex] += ConvTo32(16, Addr, BASE).PadLeft(5, '0') + " ";
                        AssemblerInstructions.offset++;
                        AssemblerInstructions.TokenBuffer += " Var " + arg;
                        return "1";
                    }
                }
                // [LABEL]      any
                else if (IsNotNumbers(arg))
                {
                    if (InAssembler)
                    {
                        //OBJBuffer += arg[a];
                        AssemblerLists.MCcode[PC] = AssemblerConst.Assembler_Lable_Ident + arg;
                        OutSrc[LineIndex] += arg + " ";
                        if (AssemblerInstructions.AM == "F")
                            AssemblerInstructions.AM = AssemblerLists.ArgumentIdentifier["addr"];
                        else
                            AssemblerInstructions.AM1 = AssemblerLists.ArgumentIdentifier["addr"];
                        PC++;
                        AssemblerInstructions.offset++;
                        return "1";
                    }
                    else
                    {
                        AssemblerLists.MCcode[PC] = AssemblerConst.Assembler_Lable_Ident + arg;
                    }
                }
                return "";
            }

            public static bool IsNotNumbers(string txt)
            {
                for (int i = 0; i < txt.Length; i++)
                {
                    if (!char.IsLetter(txt[i]) || txt[i] == '_') return false;
                }
                return true;
            }
            public static bool IsNumbers(string txt)
            {
                for (int i = 0; i < txt.Length; i++)
                {
                    if (!char.IsNumber(txt[i])) return false;
                }
                return true;
            }
            public static bool IsHex(string txt)
            {
                if (txt.Last() != 'h') return false;
                txt = txt.TrimEnd('h');
                for (int i = 0; i < txt.Length; i++)
                {
                    if (!char.IsAsciiHexDigit(txt[i])) return false;
                }
                return true;
            }
            public static bool IsBin(string txt)
            {
                if (txt.Last() != 'b') return false;
                txt = txt.TrimEnd('b');
                for (int i = 0; i < txt.Length; i++)
                {
                    if (!char.IsBetween(txt[i], '0', '1')) return false;
                }
                return true;
            }
            public static bool IsDec(string txt)
            {
                if (txt.Last() == 'b' && txt.Last() == 'h') return false;
                for (int i = 0; i < txt.Length; i++)
                {
                    if (!char.IsDigit(txt[i])) return false;
                }
                return true;
            }
        }
    }
}