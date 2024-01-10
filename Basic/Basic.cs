using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic
{
    public class BasicInt
    {
        public List<string> Asm = new List<string>(0x2FFFF + 1);
        public List<string> Consts = new List<string>();
        public required string DebugFile;
        public Variables[] Variables = new Variables[0xC8FE9 + 1];
        const int VariablesOffset = 0x37010;
        int Line = 0;
        string src = "";
        public void Build(string[] Src)
        {
            Variables.Initialize();
            for (int i = 0; i < Variables.Length; i++)
            {
                Variables[i] = new Variables();
            }
            for (Line = 0; Line < Src.Length; Line++)
            {
                if (string.IsNullOrEmpty(Src[Line])) continue;

                string Number = Src[Line].Split(' ', 2)[0];
                string Instr = Src[Line].Split(' ', 2)[1];
                string Args = "";
                Asm.Add("\r\nLINE_" + Number + ":");
                if(Instr.Split(" ").Length > 1)
                {
                    Args = Instr.Split(" ", 2)[1];
                }
                Instr = Src[Line].Split(' ', 3)[1];

                if (Instr.ToUpper().StartsWith("HOME"))
                {
                    Asm.Add("OUTB #1 #8000h ; clearing the scrren " + Src[Line]);
                    src += Number.PadLeft(4, '0') + ": OUTB #1 #1000000000000000b ; clearing the screen" + "\r\n";
                }
                else if (Instr.ToUpper().StartsWith("LET"))
                {
                    string[] arg = Args.Split(' ');
                    string Name = arg[0].ToUpper();

                    if (arg[1] == "=")
                    {
                        NewVariable(Name, Convert.ToUInt16(arg[2]));
                        src += Number.PadLeft(4, '0') + ": Var " + Name + " " + arg[2] + "\r\n";
                    }
                }
                else if (Instr.ToUpper().StartsWith("PRINT"))
                {
                    string[] arg = Args.Split(',', ';');
                    for (int v = 0; v < arg.Length; v++)
                    {
                        if (arg[v].Contains('"'))
                        {
                            arg[v] = arg[v].TrimEnd('"');
                            arg[v] = arg[v].TrimStart('"');
                            Consts.Add("Constr_Message_" + Line + ":");
                            Consts.Add(".str \"" + arg[v] + "\"");
                            Asm.Add("PRINT_LOOP_" + Line + ":");
                            Asm.Add("MOV AX [Constr_Message_" + Line + "], X");
                            Asm.Add("JMZ [EXIT_PRINT_LOOP_" + Line + "]");
                            Asm.Add("OUTB #1 AX");
                            Asm.Add("INC X");
                            Asm.Add("JMP [EXIT_PRINT_LOOP_" + Line + "]");
                            Asm.Add("EXIT_PRINT_LOOP_" + Line + ":");
                        }
                        else if (GetVariableAddr(arg[v]) != -1)
                        {
                            byte[] str = Encoding.ASCII.GetBytes(GetVariableData(arg[v]).ToString());

                            Array.Reverse(str);
                            Asm.Add("PUSH #0");

                            for (int s = 0; s < str.Length; s++)
                            {
                                Asm.Add("PUSH #" + str[s]);
                            }

                            Asm.Add("PRINT_LOOP_" + Line + ":");
                            Asm.Add("POP AX");
                            Asm.Add("JMZ [EXIT_PRINT_LOOP_" + Line + "]");
                            Asm.Add("OUTB #1 AX");
                            Asm.Add("JMP [EXIT_PRINT_LOOP_" + Line + "]");
                            Asm.Add("EXIT_PRINT_LOOP_" + Line + ":");
                        }
                        else
                        {

                        }
                    }
                    src += Number.PadLeft(4, '0') + ": PRINT " + Args + "\r\n";
                }
                else if (Instr.ToUpper().StartsWith("GOSUB"))
                {
                    src += Number.PadLeft(4, '0') + ": CALL " + Args + "\r\n";
                    Asm.Add("CALL [LINE_" + Args + "]");
                }
                else if (Instr.ToUpper().StartsWith("GOTO"))
                {
                    src += Number.PadLeft(4, '0') + ": JMP " + Args + "\r\n";
                    Asm.Add("JMP [LINE_" + Args + "]");
                }
                else if (Instr.ToUpper().StartsWith("RETURN"))
                {
                    src += Number.PadLeft(4, '0') + ": RET " + "\r\n";
                    Asm.Add("RTS");
                }
                else if (Instr.ToUpper().StartsWith("REM")) continue;
                else if (GetVariableAddr(Instr) != -1)
                {
                    string[] arg = Args.Split(' ');
                    uint value = Convert.ToUInt16(arg[1]);
                    string Name = Instr;
                    ChangeVariable(Name, value);
                }
            }
            Asm.InsertRange(Asm.Count, Consts);
            File.WriteAllText(DebugFile, src);
        }
        int NewVariable(string Name, uint value)
        {
            for (int i = 0; i < Variables.Length; i++)
            {
                if (Variables[i].Name == Name)
                {
                    // todo Erorr Same Name
                    Console.WriteLine("Error Same Name at " + Line);
                    Environment.Exit(1);
                    return -1;
                }
                if (Variables[i].Name == "")
                {
                    Variables[i].Name = Name;
                    Variables[i].Addr = (uint)i;
                    Variables[i].Data = value;
                    Asm.Add("MOV &" + Convert.ToString(i + VariablesOffset, 16) + "h #" + Convert.ToString(value, 16) + "h");
                    return i;
                }
            }
            return -1;
        }
        uint GetVariableData(string Name)
        {
            for (int i = 0; i < Variables.Length; i++)
            {
                if (Variables[i].Name == Name)
                {
                    return Variables[i].Data;
                }
            }
            return 0x8000;
        }
        int GetVariableAddr(string Name)
        {
            for (int i = 0; i < Variables.Length; i++)
            {
                if (Variables[i].Name == Name)
                {
                    return i;
                }
            }
            return -1;
        }
        void ChangeVariable(string Name, uint To)
        {
            for (int i = 0; i < Variables.Length; i++)
            {
                if (Variables[i].Name == Name)
                {
                    Variables[i].Data = To;
                    Asm.Add("MOV &" + Convert.ToString(i + VariablesOffset, 16) + "h #" + Convert.ToString(To, 16) + "h");
                    return;
                }
            }
        }
        void FreeVariable(string Name)
        {
            for (int i = 0; i < Variables.Length; i++)
            {
                if (Variables[i].Name == Name)
                {
                    Asm.Add("MOV &" + Convert.ToString(i + VariablesOffset, 16) + "h #0h");
                    Variables[i].Name = "";
                    return;
                }
            }
        }
    }
}
