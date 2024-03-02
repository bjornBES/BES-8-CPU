using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
                //Console.WriteLine("TO " + Base + " from " + FromBase + " value " + value);
                value = value.Trim();
                return Convert.ToString(Convert.ToUInt32(value, FromBase), Base).PadLeft(5, '0');
            }
            public static string ConvFrom(ref string value, bool InAssembler = false)
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
                    value = CheakAddr(value, InAssembler);
                    return value;
                }
                else
                {
                    value = ConvTo32(16, value, 10).PadLeft(5, '0');
                    return value;
                }
            }
            static string OrValue = "0";
            static string AndValue = "0";
            static string PlusValue = "0";
            static string MultValue = "0";
            static string DiviValue = "0";
            static string ModValue = "0";
            static string XorValue = "0";
            static string NotValue = "0";
            public static string CheakAddr(string arg, bool InAssembler = true)
            {
                arg = arg.Replace("[", "");
                arg = arg.Replace("]", "");

                PlusValue = AndValue = OrValue = "0";

                ParseExpression(arg);

                int Plus = int.Parse(PlusValue);
                int And = int.Parse(AndValue);
                int Or = int.Parse(OrValue);
                int Mult = int.Parse(MultValue);
                int Divi = int.Parse(DiviValue);
                int Mod = int.Parse(ModValue);
                int Xor = int.Parse(XorValue);
                int Not = int.Parse(NotValue);

                arg = arg.Split(' ', 2)[0];

                // [REG]        REG
                if (AssemblerLists.Registers.TryGetValue(arg, out ushort reg))
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
                        int NONstrvalue = Convert.ToInt32(ConvFrom(ref arg), 16);
                        if (Plus != 0)
                            NONstrvalue += Plus;
                        if (And != 0)
                            NONstrvalue &= And;
                        if (Or != 0)
                            NONstrvalue |= Or;
                        if (Mult != 0)
                            NONstrvalue *= Mult;
                        if (Divi != 0)
                            NONstrvalue /= Divi;
                        if (Mod != 0)
                            NONstrvalue %= Mod;
                        if (Xor != 0)
                            NONstrvalue ^= Xor;
                        if (Not != 0)
                            NONstrvalue = ~Not;
                        string Value = Convert.ToString(NONstrvalue, 16).PadLeft(8, '0').Substring(3,5);

                        AssemblerLists.MCcode[PC] = Value.PadLeft(5, '0');
                        PC++;
                        arg = arg.TrimEnd('h', 'b');
                        int BASE = 16;
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
                        uint VariableValue = UseVariables(arg, out string NEWAM) + (uint)Plus;
                        VariableValue &= (uint)And;
                        VariableValue |= (uint)Or;
                        string Addr = Convert.ToString(VariableValue, 16) + "h";

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
                        char OpChar;
                        if (Plus < 0) OpChar = '-';
                        else if (Plus >= 0) OpChar = '+';
                        else OpChar = '+';
                        AssemblerLists.MCcode[PC] = AssemblerConst.Assembler_Lable_Ident + arg + OpChar.ToString() + Plus.ToString();
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
                        return AssemblerConst.Assembler_Lable_Ident + arg;
                    }
                }
                return "";
            }

            private static void ParseExpression(string arg)
            {
                if (arg.StartsWith('%')) return;

                List<string> Tokens = new List<string>();
                for (int i = 0; i < arg.Length; i++)
                {
                    if (char.IsDigit(arg[i]))
                    {
                        string buf = arg[i].ToString();
                        i++;
                        while (i < arg.Length && char.IsDigit(arg[i]))
                        {
                            buf += arg[i].ToString();
                            i++;
                        }
                        if (i < arg.Length && arg[i] == 'h')
                        {
                            buf = ConvTo(10, buf, 16);
                        }
                        else if (i < arg.Length && arg[i] == 'b')
                        {
                            buf = ConvTo(10, buf, 2);
                        }
                        else
                        {
                            if (i < arg.Length)
                            {
                                i--;
                            }
                        }
                        Tokens.Add(buf);
                    }
                    else if (char.IsSeparator(arg[i]))
                    {
                        continue;
                    }
                    else if (char.IsLetter(arg[i]))
                    {
                        string buf = arg[i].ToString();
                        i++;
                        while (i < arg.Length && char.IsLetter(arg[i]))
                        {
                            buf += arg[i].ToString();
                            i++;
                        }
                        Tokens.Add(buf);
                        i--;
                    }
                    else
                    {
                        Tokens.Add(arg[i].ToString());
                        if (arg[i] == '&' && arg[i + 1] == '&')
                        {
                            i++;
                        }
                    }
                }
                //10+1
                string[] CopyTokens = new string[Tokens.Count];
                Tokens.CopyTo(CopyTokens);
                for (int i = 0; i < Tokens.Count; i++)
                {
                    string token = Tokens[i];
                    if (AssemblerLists.Registers.TryGetValue(Tokens[i], out _))
                    {
                        return;
                    }
                    switch (token)
                    {
                        case "+":
                            PlusValue = Convert.ToString(int.Parse(Tokens[i + 1]));
                            break;
                        case "-":
                            PlusValue = Convert.ToString(-int.Parse(Tokens[i + 1]));
                            break;
                        case "&":
                            AndValue = Convert.ToString(int.Parse(Tokens[i + 1]));
                            break;
                        case "|":
                            OrValue = Convert.ToString(int.Parse(Tokens[i + 1]));
                            break;
                        case "*":
                            MultValue = Convert.ToString(int.Parse(Tokens[i + 1]));
                            break;
                        case "/":
                            DiviValue = Convert.ToString(-int.Parse(Tokens[i + 1]));
                            break;
                        case "%":
                            ModValue = Convert.ToString(int.Parse(Tokens[i + 1]));
                            break;
                        case "^":
                            XorValue = Convert.ToString(int.Parse(Tokens[i + 1]));
                            break;
                        case "~":
                            NotValue = Convert.ToString(int.Parse(Tokens[i - 1]));
                            break;
                    }
                }
            }

            public static bool IsNotNumbers(string txt)
            {
                int good = 0;
                for (int i = 0; i < txt.Length; i++)
                {
                    if (char.IsLetter(txt[i]) || txt[i] == '_') good++;
                }
                if (good == txt.Length) return true;
                return false;
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