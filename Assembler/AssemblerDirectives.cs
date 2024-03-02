using assembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public partial class Assembler
    {
        public static class AssemblerDirectives
        {
            static readonly Dictionary<string, Func<string[], bool>> AssemblerDirective = new();

            public static void Start()
            {
                AssemblerDirective.Add("byte", Word);
                AssemblerDirective.Add("db", Word);

                AssemblerDirective.Add("word", Word);
                AssemblerDirective.Add("dw", Word);

                AssemblerDirective.Add("repeat", Repeat);
                AssemblerDirective.Add("strz", Strz);
                AssemblerDirective.Add("str", Str);
                AssemblerDirective.Add("org", Org);
                AssemblerDirective.Add("newfile", Newfile);
                AssemblerDirective.Add("out", Out);
                AssemblerDirective.Add("include", Include);
                AssemblerDirective.Add("bits", Bits);

                AssemblerDirective.Add("global", Global);
            }

            static bool Word(string[] arg)
            {
                    AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | Word " + arg.Length);
                for (int i = 0; i < arg.Length; i++)
                {
                    if (string.IsNullOrEmpty(arg[i])) break;
                    if (arg[i].Contains('#') || arg[i].Contains('&'))
                    {
                        Console.WriteLine("Error Var " + "BE201");
                        //Environment.Exit(1);
                    }
                    arg[i] = arg[i].TrimStart();
                    if (arg[i].Contains('$') == false)
                    {
                        AssemblerMarcos.ConvFrom(ref arg[i], false);
                        AssemblerObj.AddWord(arg[i]);
                        AssemblerLists.MCcode[PC] = arg[i];
                    }
                    else
                    {
                        string addr = Convert.ToString(PC, 16) + "h";
                        AssemblerObj.AddWord(Convert.ToString(PC, 16));
                        AssemblerLists.MCcode[PC] = AssemblerMarcos.ConvFrom(ref addr);
                    }
                    PC++;
                }
                return true;
            }
            static bool Repeat(string[] arg)
            {
                string[] args = arg[0].Split(' ',2);
                int Count = Convert.ToInt32(AssemblerMarcos.ConvFrom(ref args[0]), 16);

                string command = args[1];

                for (int i = 0; i < Count; i++)
                {
                    string instr = command.Split(' ', 2).First();
                    args = command.Split(' ', 2).Last().Split(' ');
                    bool HasArgs = args.Length >= 1;
                    Loop(instr, args, HasArgs);
                }
                return true;
            }
            static bool Strz(string[] arg)
            {
                if (arg[0].Contains('#') || arg[0].Contains('&'))
                {
                    Console.WriteLine("Error Var " + "BE201");
                    //Environment.Exit(1);
                }

                string FullString = OrgSrc[LineIndex].Split(' ', 2)[1];
                AssemblerObj.AddStr(FullString.TrimStart('"').TrimEnd('"') + "\0");
                AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | strz " + FullString);

                if (FullString.StartsWith('"') && FullString.EndsWith('"'))
                {
                    FullString = FullString.TrimStart('"');
                    FullString = FullString.TrimEnd('"');

                    List<byte> bytes = Encoding.ASCII.GetBytes(FullString).ToList();
                    bytes.Add(0);

                    for (int b = 0; b < bytes.Count; b++)
                    {
                        AssemblerLists.MCcode[PC] = Convert.ToString(bytes[b], 16).PadLeft(5, '0');
                        PC++;
                    }

                    PC++;
                }
                else
                {
                    //todo error
                    AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | ERROR B3032");
                }
                return true;
            }
            static bool Str(string[] arg)
            {
                if (arg[0].Contains('#') || arg[0].Contains('&'))
                {
                    Console.WriteLine("Error Var " + "BE201");
                    //Environment.Exit(1);
                }

                string FullString = OrgSrc[LineIndex].Split(' ', 2)[1];
                AssemblerObj.AddStr(FullString.TrimStart('"').TrimEnd('"'));
                AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | str " + FullString);

                if (FullString.StartsWith('"') && FullString.EndsWith('"'))
                {
                    FullString = FullString.TrimStart('"');
                    FullString = FullString.TrimEnd('"');

                    List<byte> bytes = Encoding.ASCII.GetBytes(FullString).ToList();

                    for (int b = 0; b < bytes.Count; b++)
                    {
                        AssemblerLists.MCcode[PC] = Convert.ToString(bytes[b], 16).PadLeft(5, '0');
                        PC++;
                    }

                    PC++;
                }
                else
                {
                    //todo error
                    AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | ERROR B3032");
                }
                return true;
            }
            static bool Org(string[] arg)
            {
                if (arg[0].Contains('#') || arg[0].Contains('&'))
                {
                    Console.WriteLine("Error Var " + "BE200");
                    //Environment.Exit(1);
                }
                if (arg[0].Contains('$') == false)
                {
                    AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | org to " + arg[0]);
                    AssemblerMarcos.ConvFrom(ref arg[0]);
                    PC = Convert.ToInt32(arg[0], 16);
                    AssemblerObj.AddOrg(arg[0]);
                    if (MaxPC < PC) MaxPC = PC;
                }
                else
                {
                    int CopyPC = PC;

                    if (arg[0].Contains('+'))
                    {
                        int add = int.Parse(arg[0].Split('+')[1]);
                        CopyPC += add;
                    }

                    AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | org to " + Convert.ToString(CopyPC, 16));

                    PC = CopyPC;
                    if (MaxPC < PC) MaxPC = PC;
                }
                return true;
            }
            static bool Newfile(string[] arg)
            {
                if (AssemblerLists.lables.Count != 0)
                {
                    for (int i = LastNewFile; i < AssemblerLists.MCcode.Length; i++)
                    {
                        if (AssemblerLists.MCcode[i].StartsWith(AssemblerConst.Assembler_Lable_Ident))
                        {
                            uint Add = 0;
                            if (AssemblerLists.MCcode[i].Contains('+'))
                            {
                                Add = uint.Parse(AssemblerLists.MCcode[i].Split('+')[1]);
                            }
                            string Name = AssemblerLists.MCcode[i].Split(':', '+')[1];
                            string addr = Convert.ToString(AssemblerLables.UseLable(Name) + Add, 16) + "h";
                            if (addr != "8000h")
                                AssemblerLists.MCcode[i] = AssemblerMarcos.ConvFrom(ref addr);
                        }
                    }
                }
                AssemblerObj.AddStr("NEWFILE");
                LineNumber = 0;
                AssemblerLists.lables.Clear();
                AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | New File " + arg[0]);
                CurrentFile = arg[0];
                LastNewFile = LineIndex;
                return true;
            }
            static bool Out(string[] arg)
            {
                return true;
            }
            static bool Include(string[] arg)
            {
                return true;
            }
            static bool Bits(string[] arg)
            {
                if (arg[0].Contains('#') || arg[0].Contains('&'))
                {
                    Console.WriteLine("Error Var " + "BE200");
                    //Environment.Exit(1);
                }
                AssemblerMarcos.ConvFrom(ref arg[0]);
                if (arg[0] == "8" || arg[0] == "16")
                    Assembler.Bits = Convert.ToUInt16(arg[0], 16);
                else
                {
                    //todo Error
                }
                return true;
            }
            static bool Global(string[] arg)
            {
                if (arg[0].Contains('#') || arg[0].Contains('&'))
                {
                    Console.WriteLine("Error Var " + "BE200");
                    //Environment.Exit(1);
                }

                AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | Global Lable " + OrgSrc[LineIndex]);
                //OBJBuffer += ":" + OrgSrc[LineIndex].Split(' ')[1].TrimEnd(':') + "|" + Convert.ToString(PC, 16) + "#";
                string Name = OrgSrc[LineIndex].Split(' ')[1].TrimEnd(':');
                AssemblerObj.AddLabel(Name, Convert.ToString(PC));
                AssemblerLists.GlobalLables.Add(new Lable()
                {
                    Name = Name,
                    Addr = PC
                });

                return true;
            }

            public static void AssemblerInstruction(string instr, string[] arg)
            {
                instr = instr.TrimStart('.');

                if (AssemblerDirective.TryGetValue(instr, out Func<string[], bool> output))
                {
                    output(arg);
                }
                else
                {
                    AssemblerErrors.ErrorDirInstructionNotFound(instr);
                }
            }
        }
    }
}