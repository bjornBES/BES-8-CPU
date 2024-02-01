using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
namespace asm
{
    public class Assembler : AssemblerMarcos
    {
        List<Macros> Macros = new(1000);
        List<Lable> lables = new();
        List<Lable> GlobalLables = new();
        List<Variables> Variables = new();
        List<Variables> ImmVariables = new();
        List<Variables> PointerVariables = new();
        public string[] MCcode = new string[0xFFFFF + 1];
        public string OBJBuffer = "";
        public ushort Bits = 16;
        public List<string> Tokens = new();
        Dictionary<string, string> keyPattens = new();
        public Dictionary<string, byte> Registers = new();
        public int PC = 0x0000;
        public int MaxPC = 0x0000;
        public string[] OrgSrc = Array.Empty<string>();
        public string[] Src;
        public string CurrentFile;
        public int LineNumber = 0;
        public int LineIndex = 0;
        public bool UseSections = true;
        int LastNewFile = 0;

        string InSectionName = "";

        public bool HasError = false;
        public void Build(string[] src)
        {
            Src = (string[])src.Clone();
            OrgSrc = src;
            Assembler = this;
            MCcode.Initialize();
            Array.Fill(MCcode, "0000", 0x0000, 0xFFFFF + 1);

            Registers.Add("AX", 0b00_0000); Registers.Add("AL", 0b10_0000); Registers.Add("AH", 0b01_0000);
            Registers.Add("BX", 0b00_0001); Registers.Add("BL", 0b10_0001); Registers.Add("BH", 0b01_0001);
            Registers.Add("CX", 0b00_0010); Registers.Add("CL", 0b10_0010); Registers.Add("CH", 0b01_0010);
            Registers.Add("DX", 0b00_0011); Registers.Add("DL", 0b10_0011); Registers.Add("DH", 0b01_0011);
            Registers.Add("ZX", 0b00_0100); Registers.Add("ZL", 0b10_0100); Registers.Add("ZH", 0b01_0100);
            Registers.Add("PC", 0b00_0101);
            Registers.Add("SP", 0b00_0110);
            Registers.Add("MB", 0b00_0111);

            Registers.Add("X", 0b00_1000); Registers.Add("XL", 0b10_1000); Registers.Add("XH", 0b01_1000);
            Registers.Add("Y", 0b00_1001); Registers.Add("YL", 0b10_1001); Registers.Add("YH", 0b01_1001);

            Registers.Add("EAX", 0b11_1010);
            Registers.Add("EBX", 0b11_1011);
            Registers.Add("F", 0b00_1111);

            keyPattens.Add("NULL", "#00");
            keyPattens.Add("KW_ENT", "#Dh");
            keyPattens.Add("KW_ESC", "#1Bh");
            keyPattens.Add("KW_BS", "#8h");
            keyPattens.Add("KW_SP", "#20h");

            string[] instrs = { "mov BL %ri", "mov AX #0", "int #10h" };
            List<string> args = new List<string>
            {
                "ri"
            };
            Macros.Add(new Macros()
            {
                Name = "Print",
                Args = args,
                Instrs = instrs,
                PCOffset = 3

            });
            //Marcos[0].InsertMarco(ref OrgSrc, ref PC);
            //Environment.Exit(0);
            Console.WriteLine("DONE");

            Loop();
        }
        void Loop()
        {
            for (int Q = 0; Q < 2 && HasError == false; Q++)
            {
                OBJBuffer = "BESASMOBJ";
                LineNumber = 0;
                OrgSrc = (string[])Src.Clone();
                PC = 0;
                for (LineIndex = 0; LineIndex < Src.Length; LineIndex++)
                {
                    LineNumber++;


                    for (int i = 0; i < OrgSrc.Length; i++)
                    {
                        if (OrgSrc[LineIndex].Contains(';'))
                        {
                            if (!OrgSrc[LineIndex].StartsWith(';'))
                            {
                                OrgSrc[LineIndex] = OrgSrc[LineIndex].Split(';')[0];
                            }
                            else
                            {
                                OrgSrc[LineIndex] = OrgSrc[LineIndex].Remove(0);
                                continue;
                            }
                        }
                    }

                    if (OrgSrc[LineIndex].StartsWith('@'))
                    {
                        MakeMacro();
                        continue;
                    }

                    OrgSrc[LineIndex] = OrgSrc[LineIndex].Trim();
                    if (string.IsNullOrEmpty(OrgSrc[LineIndex]))
                    {
                        continue;
                    }
                    OrgSrc[LineIndex] = OrgSrc[LineIndex].TrimEnd(' ').TrimEnd('\t').TrimEnd(' ').TrimEnd('\t');
                    bool FarAddr = false, LongAddr = false;
                    if (OrgSrc[LineIndex].EndsWith(':') == false)
                    {
                        if (OrgSrc[LineIndex].StartsWith('&'))
                        {
                            RunMacro();
                            continue;
                        }

                        bool HasArgs = OrgSrc[LineIndex].Split(' ').Length > 1;
                        string[] arg;
                        if (OrgSrc[LineIndex].Split(' ').Length > 1)
                        {
                            if (OrgSrc[LineIndex].Contains(Assembler_Keyword_long))
                            {
                                LongAddr = true;
                                arg = OrgSrc[LineIndex].Split(' ', 3)[2].Split(' ');
                            }
                            else if (OrgSrc[LineIndex].Contains(Assembler_Keyword_far))
                            {
                                FarAddr = true;
                                arg = OrgSrc[LineIndex].Split(' ', 3)[2].Split(' ');
                            }
                            else
                            {
                                arg = OrgSrc[LineIndex].Split(' ', 2)[1].Split(' ');
                            }
                        }
                        else
                        {
                            arg = new string[0];
                        }
                        string instr = OrgSrc[LineIndex].Split(' ')[0];
                        if (HasArgs)
                        {
                            for (int a = 0; a < arg.Length; a++)
                            {
                                if (keyPattens.TryGetValue(arg[a].TrimStart('#'), out string Value))
                                {
                                    arg[a] = Value;
                                }
                            }
                        }

                        Assembler = this;
                        if (instr.Contains('$'))
                        {
                            NewVariable(instr, arg, Q);
                            Assembler = this;
                            continue;
                        }
                        if (instr.StartsWith('.'))
                        {
                            AssemblerInstruction(instr, arg, Q);
                            continue;
                        }
                        if (UseSections == false || InSectionName == Assembler_Keyword_DataSection)
                        {
                            string InstrCode = GetInstrCode(instr, out Instructions instruction).PadRight(2, '0');
                            MCcode[PC] = InstrCode;


                            InstructionHasLength(instruction, arg, instr);

                            OrgSrc[LineIndex] = instr.PadRight(4, ' ') + "\t\t";
                            OrgSrc[LineIndex] += MCcode[PC].PadLeft(2, '0') + "/|F ";
                            MCcode[PC] += "/|F";
                            OBJBuffer += MCcode[PC];
                            string TokenBuffer = Convert.ToString(PC, 16).PadLeft(5, '0') + " | " + instr;
                            PC++;
                            string AM = "F";
                            string AM1 = "F";
                            int offset = 0;
                            if (HasArgs)
                            {
                                for (int a = 0; a < arg.Length; a++)
                                {
                                    Assembler = this;
                                    if (arg[a].StartsWith("#\'") && arg[a].EndsWith('\''))
                                    {
                                        arg[a] = arg[a].TrimStart('#');
                                        arg[a] = arg[a].TrimEnd('\'');
                                        arg[a] = arg[a].TrimStart('\'');
                                        TokenBuffer += " ImmChar " + arg[a];

                                        byte CByte = Encoding.ASCII.GetBytes(arg[a].ToCharArray())[0];
                                        string CByteHex = Convert.ToString(CByte, 16);

                                        MCcode[PC] = CByteHex;
                                        OBJBuffer += CByteHex;
                                        PC++;
                                        arg[a] = arg[a].TrimEnd('h', 'b');

                                        OrgSrc[LineIndex] += ConvTo32(16, CByteHex, 16).PadLeft(5, '0') + " ";
                                        offset++;

                                        if (AM == "F")
                                            AM = "0";
                                        else
                                            AM1 = "0";
                                        continue;
                                    }
                                    else if (arg[a].StartsWith('#'))
                                    {
                                        arg[a] = arg[a].TrimStart('#');

                                        TokenBuffer += " Imm " + arg[a];

                                        string Value = ConvFrom(ref arg[a]);

                                        MCcode[PC] = Value;
                                        OBJBuffer += Value;
                                        PC++;
                                        arg[a] = arg[a].TrimEnd('h', 'b');
                                        int BASE = 16;
                                        OrgSrc[LineIndex] += ConvTo32(16, arg[a], BASE).PadLeft(5, '0') + " ";
                                        offset++;

                                        if (AM == "F")
                                            AM = "0";
                                        else
                                            AM1 = "0";
                                        continue;
                                    }
                                    else if (arg[a].StartsWith('[') && arg[a].Contains(']'))
                                    {
                                        if (char.IsDigit(arg[a][1]))
                                        {

                                            if (arg[a].Length == 5)
                                            {
                                                if (AM == "F")
                                                    AM = "5";
                                                else
                                                    AM1 = "5";
                                            }
                                            else
                                            {
                                                if (AM == "F")
                                                    AM = "1";
                                                else
                                                    AM1 = "1";
                                            }
                                            arg[a] = arg[a].TrimStart('&');
                                            TokenBuffer += " addr " + arg[a];
                                            char EndChar = arg[a][arg[a].Length - 1];
                                            string Value = ConvFrom(ref arg[a]);
                                            if (FarAddr == true)
                                            {
                                                uint Addr = Convert.ToUInt16(Value, 16);
                                                Addr = Addr + far_Offset;
                                                Value = Convert.ToString(Addr, 16);
                                            }
                                            if (LongAddr == true)
                                            {
                                                uint Addr = Convert.ToUInt16(Value, 16);
                                                Addr = Addr + long_Offset;
                                                Value = Convert.ToString(Addr, 16);
                                            }
                                            MCcode[PC] = Value;
                                            OBJBuffer += Value;
                                            PC++;
                                            arg[a] = arg[a].TrimEnd('h', 'b');

                                            int BASE = 10;

                                            if (EndChar == 'h') BASE = 16;
                                            if (EndChar == 'b') BASE = 2;
                                            OrgSrc[LineIndex] += ConvTo32(16, arg[a], BASE).PadLeft(5, '0') + " ";
                                            offset++;
                                        }
                                        else
                                        {
                                            if (arg[a].Contains("[SP]"))
                                            {
                                                arg[a] = arg[a].Replace("[", "");
                                                arg[a] = arg[a].Replace("]", "");

                                                string RegValue = ConvTo(16, Registers["SP"].ToString().PadLeft(6, '0'), 10).PadLeft(4, '0');
                                                OBJBuffer += RegValue;
                                                MCcode[PC] = RegValue;

                                                TokenBuffer += " Lable " + arg[a];

                                                if (AM == "F")
                                                    AM = "2";
                                                else
                                                    AM1 = "2";
                                                PC++;
                                                offset++;
                                                continue;
                                            }
                                            if (Q == 1)
                                            {
                                                arg[a] = arg[a].Replace("[", "");
                                                arg[a] = arg[a].Replace("]", "");


                                                OBJBuffer += arg[a];
                                                MCcode[PC] = "LABLE:" + arg[a];
                                                if (FarAddr == true)
                                                {
                                                    MCcode[PC] += "+F";
                                                    OBJBuffer += "+F";
                                                }
                                                else if (LongAddr == true)
                                                {
                                                    MCcode[PC] += "+L";
                                                    OBJBuffer += "+L";
                                                }

                                                TokenBuffer += " Lable " + arg[a];

                                                if (AM == "F")
                                                    AM = "1";
                                                else
                                                    AM1 = "1";
                                                PC++;
                                                offset++;
                                                continue;
                                            }
                                        }
                                        continue;
                                    }
                                    else if (arg[a].Contains(','))
                                    {
                                        if (AM == "F")
                                            AM = "3";
                                        else
                                            AM1 = "3";
                                        string Lable = "";
                                        string Register = "";
                                        if (arg.Length - 1 == a + 1)
                                        {
                                            Lable = arg[a].TrimEnd(',');
                                            Register = arg[a + 1];
                                        }
                                        else if (arg.Length - 1 == a)
                                        {
                                            Lable = arg[a].Split(',')[0];
                                            Register = arg[a].Split(',')[1];
                                        }
                                        a++;

                                        if (Lable.StartsWith('['))
                                        {
                                            Lable = Lable.Replace("[", "");
                                            Lable = Lable.Replace("]", "");
                                            OBJBuffer += Lable;
                                            MCcode[PC] = "LABLE:" + Lable;
                                        }
                                        else if (Lable.StartsWith('&'))
                                        {
                                            Lable = Lable.TrimStart('&');

                                            string AddrValue = ConvFrom(ref Lable);

                                            MCcode[PC] = AddrValue;
                                            OBJBuffer += AddrValue;
                                            PC++;
                                            Lable = Lable.TrimEnd('h', 'b');
                                        }
                                        else
                                        {
                                            ErrorSyntax("ARGS");
                                        }
                                        TokenBuffer += " Indexed " + Lable + " " + Register;
                                        if (Registers.TryGetValue(Register, out byte Value))
                                        {
                                            offset++;
                                            PC++;
                                            string RegValue = ConvTo(16, Value.ToString().PadLeft(6, '0'), 10).PadLeft(4, '0');
                                            MCcode[PC] = RegValue;
                                            OBJBuffer = RegValue;
                                        }
                                        else if (char.IsNumber(Register[0]))
                                        {
                                            MCcode[PC] += "+" + Register;
                                            OBJBuffer = "+" + Register;
                                        }
                                        else
                                        {
                                            ErrorRegisterNotFound(Register);
                                        }
                                        PC++;
                                        offset++;
                                        continue;
                                    }
                                    else if (Registers.TryGetValue(arg[a], out byte Value))
                                    {
                                        TokenBuffer += " Reg " + arg[a];
                                        string RegValue = ConvTo(16, Value.ToString().PadLeft(6, '0'), 10).PadLeft(4, '0');
                                        OBJBuffer += RegValue;
                                        MCcode[PC] = RegValue;
                                        OrgSrc[LineIndex] += MCcode[PC].PadLeft(4, '0') + " ";
                                        if (AM == "F")
                                            AM = "2";
                                        else
                                            AM1 = "2";
                                        PC++;
                                        offset++;
                                        continue;
                                    }
                                    else if (arg[a].StartsWith('%'))
                                    {
                                        string Addr = Convert.ToString(UseVariables(arg[a], out string NEWAM), 16) + "h";

                                        if (AM == "F")
                                            AM = NEWAM;
                                        else
                                            AM1 = NEWAM;

                                        string AddrValue = ConvFrom(ref Addr);

                                        MCcode[PC] = AddrValue;
                                        OBJBuffer += AddrValue;
                                        PC++;

                                        Addr = Addr.TrimEnd('h', 'b');
                                        int BASE = 16;

                                        OrgSrc[LineIndex] += ConvTo32(16, Addr, BASE).PadLeft(5, '0') + " ";
                                        offset++;
                                        TokenBuffer += " Var " + arg[a];
                                        continue;
                                    }
                                    else if (arg[a].StartsWith('$'))
                                    {
                                        if ((PC & 0x10000) == 0x10000)
                                        {
                                            if (AM == "F")
                                                AM = "5";
                                            else
                                                AM1 = "5";
                                        }
                                        else
                                        {
                                            if (AM == "F")
                                                AM = "1";
                                            else
                                                AM1 = "1";
                                        }
                                        uint CopyPC = (uint)(PC - (offset + 1));

                                        if (arg[a].Contains('+'))
                                        {
                                            uint add = uint.Parse(arg[a].Split('+')[1]);
                                            CopyPC += add;
                                        }

                                        string addr = Convert.ToString(CopyPC, 16) + "h";
                                        TokenBuffer += " Get current Addr at " + ConvTo(16, CopyPC.ToString(), 10);

                                        string AddrValue = ConvFrom(ref addr);

                                        MCcode[PC] = AddrValue;
                                        OBJBuffer += AddrValue;
                                        PC++;
                                        addr = addr.TrimEnd('h', 'b');
                                        OrgSrc[LineIndex] += ConvTo(16, CopyPC.ToString(), 10).PadLeft(4, '0') + " ";
                                        offset++;
                                        continue;
                                    }
                                    else
                                    {
                                        ErrorSyntax("ARGS ERROR LABLE");
                                    }

                                }
                                MCcode[PC - (offset + 1)] = MCcode[PC - (offset + 1)].Replace("/", AM);
                                OBJBuffer = OBJBuffer.Replace("/", AM);
                                OrgSrc[LineIndex] = OrgSrc[LineIndex].Replace("/", AM);
                                //Console.WriteLine((PC - (offset + 1)).ToString().PadLeft(4, '0') + ": " + instr.PadRight(5, ' ') + "\t" + MCcode[Math.Max(PC - (offset + 1), 0)]);
                            }
                            else
                            {
                                OrgSrc[LineIndex] = OrgSrc[LineIndex];
                            }
                            if (Q == 1)
                            {
                                Tokens.Add(TokenBuffer);
                            }

                            uint Off = (uint)(PC - (offset + 1));

                            OrgSrc[LineIndex] = OrgSrc[LineIndex].Replace("|", AM1);
                            OrgSrc[LineIndex] = OrgSrc[LineIndex].Replace("/", AM).PadLeft(4, '0');
                            MCcode[Off] = MCcode[Off].Replace("|", AM1);
                            MCcode[Off] = MCcode[Off].Replace("/", AM);
                            OBJBuffer = OBJBuffer.Replace("|", AM1);
                            OBJBuffer = OBJBuffer.Replace("/", AM);
                        }
                    }
                    else
                    {
                        if (Q == 1)
                        {
                            if (OrgSrc[LineIndex].StartsWith('.'))
                            {
                                OrgSrc[LineIndex] = OrgSrc[LineIndex].TrimStart('.');
                                if (OrgSrc[LineIndex].StartsWith("global"))
                                {
                                    Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | Global Lable " + OrgSrc[LineIndex]);
                                    OBJBuffer += ":" + OrgSrc[LineIndex].Split(' ')[1].TrimEnd(':') + "|" + Convert.ToString(PC, 16) + "#"; 
                                    GlobalLables.Add(new Lable()
                                    {
                                        Name = OrgSrc[LineIndex].Split(' ')[1].TrimEnd(':') ,
                                        Addr = PC
                                    });
                                    Assembler = this;
                                    continue;
                                }
                                else
                                {
                                    ErrorDirInstructionNotFound(OrgSrc[LineIndex]);
                                }
                            }
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | Lable " + OrgSrc[LineIndex]);
                            OBJBuffer += ":" + OrgSrc[LineIndex].TrimEnd(':') + "|" + Convert.ToString(PC, 16) + "*";
                            lables.Add(new Lable()
                            {
                                Name = OrgSrc[LineIndex].TrimEnd(':'),
                                Addr = PC
                            });
                        }
                    }
                    Assembler = this;
                }
            }
            Console.WriteLine("Done Assembling");
            LineIndex = 0;
            Tokens.Add("\r\n");
            PC = 0;
            for (int i = 0; i < 0x2FFFF; i++)
            {
                PC = (ushort)i;
                if(MaxPC < PC) MaxPC = PC;
                if (MCcode[i].StartsWith("LABLE:"))
                {
                    uint Add = 0;  
                    if (MCcode[i].Contains("+L"))
                    {
                        Add = long_Offset;
                    }
                    else if (MCcode[i].Contains("+F"))
                    {
                        Add = far_Offset;
                    }
                    else if (MCcode[i].Contains('+'))
                    {
                        Add = uint.Parse(MCcode[i].Split('+')[1]);
                    }
                    string Name = MCcode[i].Split(':', '+')[1];
                    string addr = Convert.ToString(UseLable(Name, true) + Add) + "h";
                    MCcode[i] = ConvFrom(ref addr);
                }
            }
            PC = 0;
        }

        private void RunMacro()
        {
            string name = OrgSrc[LineIndex].Split(' ')[0].TrimStart('&');

            for (int i = 0; i < Macros.Count; i++)
            {
                if (Macros[i].Name == name)
                {
                    Macros[i].InsertMacro(ref OrgSrc, ref PC);
                    Tokens.Add(Convert.ToString(PC).PadLeft(5, '0') + " | " + "MACRO HERE " + name);
                }
            }
        }

        private void MakeMacro()
        {
            string MarcoName = OrgSrc[LineIndex].Split(' ')[0].TrimStart('@');
            string[] MarcoArgs = OrgSrc[LineIndex].Split(' ', 2)[0].Split(' ');
            List<string> marcoInstrs = new List<string>();
            int i = 0;
            if (OrgSrc[LineIndex + 1] == "{")
            {
                for (i = LineIndex + 2; OrgSrc[i] != "}"; i++)
                {
                    marcoInstrs.Add(OrgSrc[i].TrimStart(' '));
                }
            }
            Macros.Add(new Macros()
            {
                Name = MarcoName,
                Args = MarcoArgs.ToList(),
                Instrs = marcoInstrs.ToArray(),
                PCOffset = (uint)(i - LineIndex)
            });
            LineIndex = i;
        }

        private void InstructionHasLength(Instructions instruction, string[] arg, string instr)
        {
            switch (instruction)
            {
                case Instructions.MOV: if (arg.Length == 2) return; break;
                case Instructions.PUSH: if (arg.Length == 1) return; break;
                case Instructions.POP: if (arg.Length == 1) return; break;
                case Instructions.ADD: if (arg.Length == 2) return; break;
                case Instructions.SUB: if (arg.Length == 2) return; break;
                case Instructions.AND: if (arg.Length == 2) return; break;
                case Instructions.OR: if (arg.Length == 2) return; break;
                case Instructions.NOR: if (arg.Length == 2) return; break;
                case Instructions.CMP: if (arg.Length == 2) return; break;
                case Instructions.JNE: if (arg.Length == 1) return; break;
                case Instructions.INT: if (arg.Length == 0 || arg.Length == 1) return; break;
                case Instructions.JMP: if (arg.Length == 1) return; break;
                case Instructions.JME: if (arg.Length == 1) return; break;
                case Instructions.HALT: if (arg.Length == 0 || arg.Length == 1) return; break;
                case Instructions.CALL: if (arg.Length == 1) return; break;
                case Instructions.RTS: if (arg.Length == 0) return; break;
                case Instructions.INC: if (arg.Length == 1) return; break;
                case Instructions.DEC: if (arg.Length == 1) return; break;
                case Instructions.OUTB: if (arg.Length == 2) return; break;
                case Instructions.INB: if (arg.Length == 2) return; break;
                case Instructions.NOP: if (arg.Length == 0) return; break;
                case Instructions.SEF: if (arg.Length == 1) return; break;
                case Instructions.CLF: if (arg.Length == 1) return; break;
                case Instructions.JMZ: if (arg.Length == 1) return; break;
                case Instructions.JNZ: if (arg.Length == 1) return; break;
                case Instructions.JML: if (arg.Length == 1) return; break;
                case Instructions.JMG: if (arg.Length == 1) return; break;
                case Instructions.JMC: if (arg.Length == 1) return; break;
                case Instructions.JNC: if (arg.Length == 1) return; break;
                case Instructions.MUL: if (arg.Length == 2) return; break;
                case Instructions.DIV: if (arg.Length == 2) return; break;
                case Instructions.NOT: if (arg.Length == 1) return; break;
                case Instructions.PUSHR: if (arg.Length == 0) return; break;
                case Instructions.POPR: if (arg.Length == 0) return; break;
                case Instructions.ROL: if (arg.Length == 1 || arg.Length == 2) return; break;
                case Instructions.ROR: if (arg.Length == 1 || arg.Length == 2) return; break;
                default:
                    ErrorInstructionNotFound(instr);
                    break;
            }
            ErrorInstrLength(instr.Length, arg);
        }
        private string GetInstrCode(string instr, out Instructions instruction)
        {
            int[] instructions = (int[])Enum.GetValues(typeof(Instructions));
            string[] name = Enum.GetNames(typeof(Instructions));
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == instr.ToUpper())
                {
                    instruction = (Instructions)Enum.Parse(typeof(Instructions), instr.ToUpper());
                    return Convert.ToString(instructions[i], 16).PadLeft(2, '0');
                }
            }
            ErrorInstructionNotFound(instr);
            instruction = Instructions.HALT;
            return "XXXXXX";
        }
        private uint UseVariables(string Name, out string AddrMode)
        {
            Name = Name.TrimStart('%');
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Name == Variables[i].Name)
                {
                    string HexString = Convert.ToString(Variables[i].Value, 16).PadLeft(5, '0').Remove(0, 1);
                    AddrMode = "1";
                    return Convert.ToUInt32(HexString, 16);
                }
            }
            for (int i = 0; i < PointerVariables.Count; i++)
            {
                if (Name == PointerVariables[i].Name)
                {
                    string HexString = Convert.ToString(PointerVariables[i].Value, 16).PadLeft(5, '0').Remove(0, 1);
                    AddrMode = "1";
                    return Convert.ToUInt32(HexString, 16);
                }
            }
            for (int i = 0; i < ImmVariables.Count; i++)
            {
                if (Name == ImmVariables[i].Name)
                {
                    string HexString = Convert.ToString(ImmVariables[i].Value, 16).PadLeft(5, '0').Remove(0, 1);
                    AddrMode = "0";
                    return Convert.ToUInt32(HexString, 16);
                }
            }
            ErrorVariableNotFound(Name);
            AddrMode = "F";
            return 0;
        }
        int UseLable(string name, bool WriteTokens = false)
        {
            for (int i = 0; i < lables.Count; i++)
            {
                if (name.TrimEnd(':') == lables[i].Name)
                {
                    if(WriteTokens)
                    Tokens.Add(Convert.ToString(lables[i].Addr, 16).PadLeft(5, '0') + " | " + "Lable " + name + " ");
                    return lables[i].Addr;
                }
            }
            for (int i = 0; i < GlobalLables.Count; i++)
            {
                if (name.TrimEnd(':') == GlobalLables[i].Name)
                {
                    if(WriteTokens)
                    Tokens.Add(Convert.ToString(GlobalLables[i].Addr, 16).PadLeft(5, '0') + " | " + "Global Lable " + name + " ");
                    return GlobalLables[i].Addr;
                }
            }
            return 0x8000;
        }
        void NewVariable(string instr, string[] arg, int Q)
        {
            if (arg[1].Contains('#') || arg[1].Contains('&'))
            {
                Console.WriteLine("Error Var " + "BE201");
                //Environment.Exit(1);
            }

            if (arg[0] == "=")
            {
                if (Q == 1)
                    Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | Var " + instr + " " + arg[1]);
                ConvFrom(ref arg[1]);
                Variables.Add(new Variables()
                {
                    Name = instr.TrimStart('$'),
                    Value = Convert.ToUInt32(arg[1], 16)
                });
            }
            else if (arg[0] == "*=")
            {
                ConvFrom(ref arg[1]);
                if (Q == 1)
                    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + "Var Pointer " + instr + " 0000" + " at " + Convert.ToString(Convert.ToUInt32(arg[1], 16), 16));
                MCcode[Convert.ToUInt32(arg[1], 16)] = "FFFFF";
                PointerVariables.Add(new Variables()
                {
                    Name = instr.TrimStart('$'),
                    Value = Convert.ToUInt32(arg[1], 16)
                });
            }
            else if (arg[0] == "#=")
            {
                ConvFrom(ref arg[1]);
                if (Q == 1)
                    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + "Imm Var " + instr + " 0000" + " at " + Convert.ToString(Convert.ToUInt32(arg[1], 16), 16));
                ConvFrom(ref arg[1]);
                Variables.Add(new Variables()
                {
                    Name = instr.TrimStart('$'),
                    Value = Convert.ToUInt32(arg[1], 16)
                });
            }
        }
        void AssemblerInstruction(string instr, string[] arg, int Q)
        {
            instr = instr.TrimStart('.');
            switch (instr)
            {
                case "byte":
                case "word":
                    if (UseSections || InSectionName == Assembler_Keyword_CodeSection)
                    {
                        if (arg[0].Contains('#') || arg[0].Contains('&'))
                        {
                            Console.WriteLine("Error Var " + "BE201");
                            //Environment.Exit(1);
                        }
                        if (Q == 1)
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | " + instr + " " + arg[0]);
                        if (arg[0].Contains('$') == false)
                        {
                            ConvFrom(ref arg[0]);
                            if (!char.IsDigit(arg[0][0]))
                            {
                                MCcode[PC] = "LABLE:" + arg[0];
                            }
                            else
                            {
                                MCcode[PC] = arg[0];
                            }
                        }
                        else
                        {
                            string addr = Convert.ToString(PC, 16) + "h";
                            MCcode[PC] = ConvFrom(ref addr);
                        }
                        PC++;
                    }
                    Assembler = this;
                    break;
                case "strz":
                    if (UseSections || InSectionName == Assembler_Keyword_CodeSection)
                    {
                        if (arg[0].Contains('#') || arg[0].Contains('&'))
                        {
                            Console.WriteLine("Error Var " + "BE201");
                            //Environment.Exit(1);
                        }

                        string FullString = OrgSrc[LineIndex].Split(' ', 2)[1];

                        if (Q == 1)
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | " + instr + " " + FullString);

                        if (FullString.StartsWith('"') && FullString.EndsWith('"'))
                        {
                            FullString = FullString.TrimStart('"');
                            FullString = FullString.TrimEnd('"');

                            List<byte> bytes = Encoding.ASCII.GetBytes(FullString).ToList();
                            bytes.Add(0);

                            for (int b = 0; b < bytes.Count; b++)
                            {
                                MCcode[PC] = Convert.ToString(bytes[b], 16).PadLeft(5, '0');
                                PC++;
                            }

                            PC++;
                        }
                        else
                        {
                            //todo error
                            if (Q == 1)
                                Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | ERROR B3032");
                        }
                        Assembler = this;
                    }
                    break;
                case "str":
                    if (UseSections || InSectionName == Assembler_Keyword_CodeSection)
                    {
                        if (arg[0].Contains('#') || arg[0].Contains('&'))
                        {
                            Console.WriteLine("Error Var " + "BE201");
                            //Environment.Exit(1);
                        }

                        string FullString = OrgSrc[LineIndex].Split(' ', 2)[1];

                        if (Q == 1)
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | " + instr + " " + FullString);

                        if (FullString.StartsWith('"') && FullString.EndsWith('"'))
                        {
                            FullString = FullString.TrimStart('"');
                            FullString = FullString.TrimEnd('"');

                            List<byte> bytes = Encoding.ASCII.GetBytes(FullString).ToList();

                            for (int b = 0; b < bytes.Count; b++)
                            {
                                MCcode[PC] = Convert.ToString(bytes[b], 16).PadLeft(5, '0');
                                PC++;
                            }

                            PC++;
                        }
                        else
                        {
                            //todo error
                            if (Q == 1)
                                Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | ERROR B3032");
                        }
                        Assembler = this;
                    }
                    break;
                case "org":
                    if (arg[0].Contains('#') || arg[0].Contains('&'))
                    {
                        Console.WriteLine("Error Var " + "BE200");
                        //Environment.Exit(1);
                    }
                    if (arg[0].Contains('$') == false)
                    {
                        if (Q == 1)
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | " + instr + " to " + arg[0]);
                        ConvFrom(ref arg[0]);
                        PC = Convert.ToInt32(arg[0], 16);
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

                        if (Q == 1)
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | " + instr + " to " + Convert.ToString(CopyPC, 16));

                        PC = CopyPC;
                        if (MaxPC < PC) MaxPC = PC;
                    }
                    Assembler = this;
                    break;
                case "section":

                    if (UseSections)
                    {
                        //if (Q == 1)
                        //    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + instr + " " + arg[0]);
                        switch (arg[0])
                        {
                            case "data":
                                InSectionName = Assembler_Keyword_CodeSection;
                                break;
                            case "text":
                                InSectionName = Assembler_Keyword_DataSection;
                                break;
                        }
                    }
                    Assembler = this;
                    break;
                case "data":
                    if (UseSections)
                    {
                        //if (Q == 1)
                        //    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | in section " + instr);
                        InSectionName = Assembler_Keyword_DataSection;
                    }
                    break;
                case "text":
                    if (UseSections)
                    {
                        //if(Q == 1)
                        //    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | in section " + instr);
                        InSectionName = Assembler_Keyword_DataSection;
                    }
                    break;
                case "newfile": 
                    if (lables.Count != 0)
                    {
                        for (int i = LastNewFile; i < LineIndex + 1; i++)
                        {
                            if (MCcode[i].StartsWith("LABLE:"))
                            {
                                uint Add = 0;
                                if (MCcode[i].Contains('+'))
                                {
                                    Add = uint.Parse(MCcode[i].Split('+')[1]);
                                }
                                string Name = MCcode[i].Split(':', '+')[1];
                                string addr = Convert.ToString(UseLable(Name) + Add) + "h";
                                if(addr != "8FFFh")
                                    MCcode[i] = ConvFrom(ref addr);
                            }
                        }
                    }

                    lables.Clear();
                    LineNumber = 0;
                    if (Q == 1)
                        Tokens.Add(Convert.ToString(PC, 16).PadLeft(5, '0') + " | New File " + arg[0]);
                    CurrentFile = arg[0];
                    Assembler = this;
                    LastNewFile = LineIndex;
                    break;
                case "out":

                    break;
                case "include":

                    break;
                case "bits":
                    if (arg[0].Contains('#') || arg[0].Contains('&'))
                    {
                        Console.WriteLine("Error Var " + "BE200");
                        //Environment.Exit(1);
                    }
                    ConvFrom(ref arg[0]);
                    if (arg[0] == "8" || arg[0] == "16")
                    Bits = Convert.ToUInt16(arg[0], 16);
                    else
                    {
                        //todo Error
                    }
                    Assembler = this;
                    break;
                default:
                    ErrorDirInstructionNotFound(instr);
                    break;
            }
        }
    }
}
