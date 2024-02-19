using System.Text;
namespace assembler
{
    public partial class Assembler
    {
        public static ushort Bits = 16;
        public static int PC = 0x0000;
        public static int MaxPC = 0x0000;
        public static string[] OrgSrc = Array.Empty<string>();
        public static string[] OutSrc = Array.Empty<string>();
        public static string CurrentFile;
        public static int LineIndex = 0;
        public static int LastNewFile = 0;
        public static bool HasError = false;

        public static void Build(string[] src)
        {
            AssemblerDirectives.Start();
            AssemblerObj.Start();
            AssemblerLists.Start();
            OrgSrc = (string[])src.Clone();
            OutSrc = (string[])src.Clone();

            for (LineIndex = 0; LineIndex < src.Length; LineIndex++)
            {
                //if (!(LineIndex < OrgSrc.Length)) break;
                OutSrc[LineIndex] = OutSrc[LineIndex].Trim(' ');
                OutSrc[LineIndex] = OutSrc[LineIndex].TrimEnd(' ');
                OutSrc[LineIndex] = OutSrc[LineIndex].TrimStart(' ');
                OutSrc[LineIndex] = OutSrc[LineIndex].Trim('\t');

                OrgSrc[LineIndex] = OrgSrc[LineIndex].Trim(' ');
                OrgSrc[LineIndex] = OrgSrc[LineIndex].TrimEnd(' ');
                OrgSrc[LineIndex] = OrgSrc[LineIndex].TrimStart(' ');
                OrgSrc[LineIndex] = OrgSrc[LineIndex].Trim('\t');

                for (int i = 0; i < OrgSrc.Length; i++)
                {
                    if (OrgSrc[LineIndex].Contains(';'))
                    {
                        if (!OrgSrc[LineIndex].StartsWith(';'))
                        {
                            OutSrc[LineIndex] = OutSrc[LineIndex].Split(';')[0];
                            OrgSrc[LineIndex] = OrgSrc[LineIndex].Split(';')[0];
                        }
                        else
                        {
                            OutSrc[LineIndex] = OutSrc[LineIndex].Remove(0);
                            OrgSrc[LineIndex] = OrgSrc[LineIndex].Remove(0);
                            continue;
                        }
                    }
                }
                OutSrc[LineIndex] = OutSrc[LineIndex].Replace("\t", "");
                OrgSrc[LineIndex] = OrgSrc[LineIndex].Replace("\t", "");


                if (string.IsNullOrEmpty(OrgSrc[LineIndex]))
                {
                    continue;
                }


                string[] line = OrgSrc[LineIndex].Split(' ');
                string Formatet = "";
                for (int i = 0; i < line.Length; i++)
                {
                    if (string.IsNullOrEmpty(line[i]))
                    {
                        continue;
                    }
                    Formatet += line[i] + " ";
                }

                OrgSrc[LineIndex] = Formatet;

                if (OrgSrc[LineIndex].StartsWith('@'))
                {
                    // TODO MakeMacro();
                    continue;
                }

                string[] arg;
                OrgSrc[LineIndex] = OrgSrc[LineIndex].Trim();
                if (OrgSrc[LineIndex].Split(' ').Length > 1)
                {
                    arg = OrgSrc[LineIndex].Split(' ', 2)[1].Split(',');
                }
                else
                {
                    arg = Array.Empty<string>();
                }

                bool HasArgs = OrgSrc[LineIndex].Split(' ').Length > 1;
                string instr = OrgSrc[LineIndex].Split(' ')[0];
                if (HasArgs)
                {
                    for (int a = 0; a < arg.Length; a++)
                    {
                        if (AssemblerLists.keyPattens.TryGetValue(arg[a].Trim().TrimStart('#'), out string Value))
                        {
                            arg[a] = Value;
                        }
                    }
                }

                Formatet = instr + " ";
                for (int i = 0; i < arg.Length; i++)
                {
                    Formatet += arg[i].Trim() + " ";
                }

                Loop(instr, arg, HasArgs);
            }
            Console.WriteLine("Done Assembling");
            LineIndex = 0;
            AssemblerLists.Tokens.Add("\r\n");
            PC = 0;
            for (int i = 0; i < AssemblerLists.MCcode.Length; i++)
            {
                PC = (ushort)i;
                if (MaxPC < PC) MaxPC = PC;
                if (AssemblerLists.MCcode[i].StartsWith(AssemblerConst.Assembler_Lable_Ident))
                {
                    uint Add = 0;
                    uint Sub = 0;
                    if (AssemblerLists.MCcode[i].Contains('+'))
                    {
                        Add = uint.Parse(AssemblerLists.MCcode[i].Split('+')[1]);
                    }
                    else if (AssemblerLists.MCcode[i].Contains('-'))
                    {
                        Sub = uint.Parse(AssemblerLists.MCcode[i].Split('-')[1]);
                    }
                    string Name = AssemblerLists.MCcode[i].Split(':', '+')[1];
                    string addr = Convert.ToString((AssemblerLables.UseLable(Name, true) + Add) - Sub, 16) + "h";
                    AssemblerLists.MCcode[i] = AssemblerMarcos.ConvFrom(ref addr);
                }
            }
            PC = 0;
            Console.WriteLine("BUILD");
        }
        static void Loop(string instr, string[] arg, bool HasArgs)
        {
            if (string.IsNullOrEmpty(instr)) return;

            OutSrc[LineIndex] = Convert.ToString(PC, 16).PadLeft(5, '0') + " " + OutSrc[LineIndex];
            if (instr.Contains('$'))
            {
                NewVariable(instr, arg, 0);
                return;
            }

            if (instr.StartsWith('.'))
            {
                AssemblerDirectives.AssemblerInstruction(instr, arg);
                return;
            }

            if (!instr.Contains(':'))
            {
                AssemblerInstructions.Doinstr(instr, arg, HasArgs);
            }
            else
            {
                if (OrgSrc[LineIndex].StartsWith('.'))
                {
                    OutSrc[LineIndex] = OrgSrc[LineIndex].TrimStart('.');
                    if (OrgSrc[LineIndex].StartsWith("global"))
                    {
                        AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | Global Lable " + OrgSrc[LineIndex]);
                        //OBJBuffer += ":" + OrgSrc[LineIndex].Split(' ')[1].TrimEnd(':') + "|" + Convert.ToString(PC, 16) + "#";
                        AssemblerLists.GlobalLables.Add(new Lable()
                        {
                            Name = OrgSrc[LineIndex].Split(' ')[1].TrimEnd(':'),
                            Addr = PC
                        });
                        return;
                    }
                    else
                    {
                        AssemblerErrors.ErrorDirInstructionNotFound(OrgSrc[LineIndex]);
                    }
                }
                AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | Lable " + OrgSrc[LineIndex]);
                //OBJBuffer += ":" + OrgSrc[LineIndex].TrimEnd(':') + "|" + Convert.ToString(PC, 16) + "*";
                AssemblerLists.lables.Add(new Lable()
                {
                    Name = OrgSrc[LineIndex].TrimEnd(':'),
                    Addr = PC
                });
            }
            if(PC > MaxPC)
            {
                MaxPC = PC;
            }
        }
        static void NewVariable(string instr, string[] arg, int Q)
        {
            arg = arg[0].Split(' ');
            if (arg[1].Contains('#') || arg[1].Contains('&'))
            {
                Console.WriteLine("Error Var " + "BE201");
                //Environment.Exit(1);
            }

            if (arg[0] == "=")
            {
                if (Q == 1)
                    AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | Var " + instr + " " + arg[1]);
                AssemblerMarcos.ConvFrom(ref arg[1]);
                AssemblerVariables.AddVariable(instr.TrimStart('$'), Convert.ToUInt32(arg[1], 16), false, false);
            }
            else if (arg[0] == "*=")
            {
                AssemblerMarcos.ConvFrom(ref arg[1]);
                if (Q == 1)
                    AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + "Var Pointer " + instr + " 0000" + " at " + Convert.ToString(Convert.ToUInt32(arg[1], 16), 16));
                AssemblerLists.MCcode[Convert.ToInt32(arg[1], 16)] = "FFFFF";
                AssemblerVariables.AddVariable(instr.TrimStart('$'), Convert.ToUInt32(arg[1], 16), true, false);
            }
            else if (arg[0] == "#=")
            {
                AssemblerMarcos.ConvFrom(ref arg[1]);
                if (Q == 1)
                    AssemblerLists.Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + "Imm Var " + instr + " 0000" + " at " + Convert.ToString(Convert.ToUInt32(arg[1], 16), 16));
                AssemblerMarcos.ConvFrom(ref arg[1]);
                AssemblerVariables.AddVariable(instr.TrimStart('$'), Convert.ToUInt32(arg[1], 16), false, true);
            }
        }
    }
}
