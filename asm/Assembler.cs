using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
namespace asm
{
    public class Assembler : AssemblerErrors
    {
        List<Lable> lables = new List<Lable>();
        List<Lable> Alllables = new List<Lable>();
        List<Lable> GlobalLables = new List<Lable>();
        List<Variables> Variables = new List<Variables>();
        public string[] MCcode = new string[0x1FFFF + 1];
        public List<string> Tokens = new List<string>();
        Dictionary<string, string> KeyPattens = new Dictionary<string, string>();
        public Dictionary<string, byte> Registers = new Dictionary<string, byte>();
        public uint PC = 0x0000;
        public uint VarPC = 0x11000;
        public string[] OrgSrc = new string[0];
        public string[] Src;
        public string CurrentFile;
        public int LineNumber = 0;
        public int LineIndex = 0;

        bool InCodeSection = false;
        bool InDataSection = false;
        public void Build(string[] src)
        {
            Src = (string[])src.Clone();
            OrgSrc = src;
            lables.Clear();
            Variables.Clear();
            Assembler = this;
            if (Registers.Count == 0)
            {
                MCcode.Initialize();
                Array.Fill(MCcode, "0000", 0x0000, 0x1FFFF + 1);
                Registers.Add("A", 0b00_0000);
                Registers.Add("AX", 0b00_0000); Registers.Add("AL", 0b10_0000); Registers.Add("AH", 0b01_0000);
                Registers.Add("B", 0b00_0001);
                Registers.Add("BX", 0b00_0001); Registers.Add("BL", 0b10_0001); Registers.Add("BH", 0b01_0001);
                Registers.Add("C", 0b00_0010);
                Registers.Add("CX", 0b00_0010); Registers.Add("CL", 0b10_0010); Registers.Add("CH", 0b01_0010);
                Registers.Add("D", 0b00_0011);
                Registers.Add("DX", 0b00_0011); Registers.Add("DL", 0b10_0011); Registers.Add("DH", 0b01_0011);
                Registers.Add("Z", 0b00_0100);
                Registers.Add("ZX", 0b00_0100); Registers.Add("ZL", 0b10_0100); Registers.Add("ZH", 0b01_0100);
                Registers.Add("PC", 0b00_0101);
                Registers.Add("SP", 0b00_0110);
                Registers.Add("MB", 0b00_0111);

                Registers.Add("X", 0b00_1000); Registers.Add("XL", 0b10_1000); Registers.Add("XH", 0b01_1000);
                Registers.Add("Y", 0b00_1001); Registers.Add("YL", 0b10_1001); Registers.Add("YH", 0b01_1001);

                Registers.Add("EAX", 0b11_1010);
                Registers.Add("EBX", 0b11_1011);
                Registers.Add("F", 0b00_1111);
            }

            KeyPattens.Add("NULL", "00");
            KeyPattens.Add("KW_ENT", "#Dh");
            KeyPattens.Add("KW_ESC", "#1Bh");
            KeyPattens.Add("KW_BS", "#8h");
            KeyPattens.Add("KW_SP", "#20h");


            Console.WriteLine("varPC is at " + Convert.ToString(VarPC, 16));
            Console.WriteLine("DONE");

            Console.WriteLine("Start MC " + MCcode.Length);

            Loop(src);

            Console.WriteLine("END MC " + MCcode.Length);
        }
        void Loop(string[] src)
        {
            uint VarPCCopy = VarPC;
            for (int Q = 0; Q < 2; Q++)
            {
                LineNumber = 0;
                OrgSrc = (string[])src.Clone();
                VarPC = VarPCCopy;
                MCcode.Initialize();
                PC = 0;
                for (LineIndex = 0; LineIndex < src.Length; LineIndex++)
                {
                    LineNumber++;
                    OrgSrc[LineIndex] = OrgSrc[LineIndex].Trim();
                    if (string.IsNullOrEmpty(OrgSrc[LineIndex]))
                    {
                        continue;
                    }
                    if (OrgSrc[LineIndex].Contains(';'))
                    {
                        if (!OrgSrc[LineIndex].StartsWith(';'))
                        {
                            //if (Q == 1)
                            //Tokens.Add(ConvTo(16, PC.ToString(), 10).PadLeft(4, '0') + " | [TEXT];" + OrgSrc[LineIndex].Split(';')[1]);
                            OrgSrc[LineIndex] = OrgSrc[LineIndex].Split(';')[0];
                        }
                        else
                        {
                            //if (Q == 1)
                            //Tokens.Add(ConvTo(16, PC.ToString(), 10).PadLeft(4, '0') + " | [TEXT]" + OrgSrc[LineIndex]);
                            OrgSrc[LineIndex] = OrgSrc[LineIndex].Remove(0);
                            continue;
                        }
                    }
                    int reo = 0;
                    while (OrgSrc[LineIndex].EndsWith(' '))
                    {
                        reo++;
                        OrgSrc[LineIndex] = OrgSrc[LineIndex].TrimEnd(' ');
                        if (reo == 50)
                        {
                            break;
                        }
                    }
                    if (OrgSrc[LineIndex].Contains('\t'))
                    {
                        OrgSrc[LineIndex] = OrgSrc[LineIndex].Split("\t", 2)[0];
                    }

                    if (OrgSrc[LineIndex].EndsWith(':') == false)
                    {
                        bool HasArgs = OrgSrc[LineIndex].Split(' ').Length > 1;
                        string[] arg;
                        if (OrgSrc[LineIndex].Split(' ').Length > 1)
                        {
                            List<string> args = new List<string>();
                            for (int a = 1; a < OrgSrc[LineIndex].Split(' ').Length; a++)
                            {
                                args.Add(OrgSrc[LineIndex].Split(' ')[a]);
                            }
                            arg = args.ToArray();
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
                                if (KeyPattens.TryGetValue(arg[a].TrimStart('#'), out string Value))
                                {
                                    arg[a] = Value;
                                }
                            }
                        }
                        Assembler = this;
                        if (instr.Contains('$'))
                        {
                            if (arg[1].Contains('#') || arg[1].Contains('&'))
                            {
                                Console.WriteLine("Error Var " + "BE201");
                                Environment.Exit(1);
                            }
                            NewVariable(instr, arg, Q);
                            Assembler = this;
                            continue;
                        }
                        if (instr.StartsWith('.'))
                        {
                            AssemblerInstruction(instr, arg, Q);
                            continue;
                        }
                        if (InCodeSection == true)
                        {
                            MCcode[PC] = GetInstrCode(instr, out Instructions instruction).PadRight(3, '0');
                            InstructionHasLength(instruction, arg, instr);

                            OrgSrc[LineIndex] = instr.PadRight(4, ' ') + "\t\t";
                            OrgSrc[LineIndex] += MCcode[PC].PadLeft(3, '0') + "| ";
                            MCcode[PC] += "|";
                            string TokenBuffer = Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + instr;
                            PC++;
                            string AM = "F";
                            int offset = 0;
                            if (HasArgs)
                            {
                                bool Using32Bits = src[LineIndex].Contains("EAX") || src[LineIndex].Contains("EBX");
                                for (int a = 0; a < arg.Length; a++)
                                {
                                    Assembler = this;
                                    if (arg[a].StartsWith('#'))
                                    {
                                        arg[a] = arg[a].TrimStart('#');

                                        if (Using32Bits)
                                        {
                                            TokenBuffer += " 32 bit Imm " + arg[a];

                                            char EndChar = arg[a][arg[a].Length - 1];

                                            arg[a] = arg[a].TrimEnd('h', 'b');

                                            string[] num = Split32bitNumber(arg[a], EndChar);

                                            num[0] += EndChar;
                                            num[1] += EndChar;
                                            MCcode[PC] = ConvFrom(ref num[0]);
                                            PC++;
                                            MCcode[PC] = ConvFrom(ref num[1]);
                                            PC++;

                                            int BASE = 10;

                                            if (EndChar == 'h') BASE = 16;
                                            if (EndChar == 'b') BASE = 2;

                                            OrgSrc[LineIndex] += ConvTo32(16, arg[a], BASE).PadLeft(8, '0') + " ";

                                            AM = "8";
                                            offset++;
                                            offset++;
                                        }
                                        else
                                        {
                                            TokenBuffer += " Imm " + arg[a];
                                            MCcode[PC] = ConvFrom(ref arg[a]);
                                            OrgSrc[LineIndex] += MCcode[PC].PadLeft(4, '0') + " ";
                                            PC++;
                                            offset++;
                                            AM = "0";
                                        }
                                    }
                                    else if (arg[a].StartsWith('&'))
                                    {
                                        if (arg[a].Length == 5)
                                        {
                                            AM = "5";
                                        }
                                        else
                                        {
                                            AM = "1";
                                        }
                                        arg[a] = arg[a].TrimStart('&');
                                        TokenBuffer += " addr " + arg[a];
                                        MCcode[PC] = ConvFrom(ref arg[a]);
                                        OrgSrc[LineIndex] += MCcode[PC].PadLeft(4, '0') + " ";
                                        PC++;
                                        offset++;
                                    }
                                    else if (arg[a].Contains(','))
                                    {
                                        AM = "3";
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

                                        TokenBuffer += " Indexed " + Lable + " " + Register;
                                        MCcode[PC] = "LABLE:" + Lable;
                                        PC++;
                                        if (Registers.TryGetValue(Register, out byte Value))
                                        {
                                            MCcode[PC] = ConvTo(16, Value.ToString().PadLeft(6, '0'), 10).PadLeft(4, '0');
                                        }
                                        else
                                        {
                                            ErrorRegisterNotFound(Register);
                                        }
                                        PC++;
                                        offset++;
                                        offset++;
                                        continue;
                                    }
                                    else if (Registers.TryGetValue(arg[a], out byte Value))
                                    {
                                        TokenBuffer += " Reg " + arg[a];
                                        MCcode[PC] = ConvTo(16, Value.ToString().PadLeft(6, '0'), 10).PadLeft(4, '0');
                                        OrgSrc[LineIndex] += MCcode[PC].PadLeft(4, '0') + " ";
                                        AM = "2";
                                        PC++;
                                        offset++;
                                    }
                                    else if (arg[a].StartsWith('%'))
                                    {
                                        if ((UseVariables(arg[a]) & 0x10000) == 0x10000)
                                        {
                                            AM = "5";
                                            OrgSrc[LineIndex] += ConvTo20(16, UseVariables(arg[a]).ToString(), 10).PadLeft(4, '0');
                                            MCcode[PC] = ConvTo20(16, UseVariables(arg[a]).ToString(), 10).PadLeft(4, '0');
                                        }
                                        else
                                        {
                                            OrgSrc[LineIndex] += ConvTo(16, UseVariables(arg[a]).ToString(), 10).PadLeft(4, '0');
                                            MCcode[PC] = ConvTo(16, UseVariables(arg[a]).ToString(), 10).PadLeft(4, '0');
                                            AM = "1";
                                        }
                                        TokenBuffer += " Var " + arg[a];
                                        PC++;
                                        offset++;
                                    }
                                    else if (arg[a].StartsWith('$'))
                                    {
                                        if ((PC & 0x10000) == 0x10000)
                                        {
                                            AM = "5";
                                        }
                                        else
                                        {
                                            AM = "1";
                                        }
                                        ushort CopyPC = (ushort)(PC - (offset + 1));
                                        TokenBuffer += " Get current Addr at " + ConvTo(16, CopyPC.ToString(), 10);
                                        MCcode[PC] = ConvTo(16, CopyPC.ToString(), 10);
                                        OrgSrc[LineIndex] += ConvTo(16, CopyPC.ToString(), 10).PadLeft(4, '0') + " ";
                                        PC++;
                                        offset++;
                                    }
                                    else
                                    {
                                        if (Q == 1)
                                        {
                                            MCcode[PC] = "LABLE:" + arg[a];

                                            TokenBuffer += " Lable " + arg[a];

                                            AM = "1";
                                            PC++;
                                            offset++;
                                            continue;
                                        }

                                    }
                                }
                            }
                            else
                            {
                                OrgSrc[LineIndex] = OrgSrc[LineIndex];
                            }
                            if (Q == 1)
                            {
                                Tokens.Add(TokenBuffer);
                            }
                            OrgSrc[LineIndex] = OrgSrc[LineIndex].Replace("|", AM).PadLeft(4, '0');
                            MCcode[PC - (offset + 1)] = MCcode[PC - (offset + 1)].Replace("|", AM);
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
                                    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | Global Lable " + OrgSrc[LineIndex]);
                                    GlobalLables.Add(new Lable()
                                    {
                                        Name = OrgSrc[LineIndex].Split(' ')[1].TrimEnd(':'),
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
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | Lable " + OrgSrc[LineIndex]);
                            lables.Add(new Lable()
                            {
                                Name = OrgSrc[LineIndex].TrimEnd(':'),
                                Addr = PC
                            });
                            Alllables.Add(new Lable()
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
            PC = 0;
            Tokens.Add("\r\n");
            for (int i = 0; i < 0x8000; i++)
            {
                PC = (ushort)i;
                if (MCcode[i].EndsWith("F0") || MCcode[i].EndsWith("F1") || MCcode[i].EndsWith("F3") || MCcode[i].EndsWith("F4") || MCcode[i].EndsWith("F5") || MCcode[i].EndsWith("FF"))
                {
                    LineIndex++;
                }
                if (MCcode[i].StartsWith("LABLE:"))
                {
                    string Name = MCcode[i].Split(':')[1];
                    MCcode[i] = ConvTo(16, UseLableUsingAll(Name).ToString(), 10).PadLeft(4, '0');
                }
            }
            PC = 0;
        }

        private string[] Split32bitNumber(string Value, char EndChar)
        {
            string LOW;
            string HIGH;
            if (EndChar == 'h')
            {
                Value = Value.PadLeft(8, '0');

                HIGH = Value.Substring(0, 4);
                LOW = Value.Substring(4, 4);
            }
            else if (EndChar == 'b')
            {
                Value = Value.PadLeft(32, '0');

                HIGH = Value.Substring(0, 16);
                LOW = Value.Substring(16, 16);
            }
            else
            {
                Value = Value.PadLeft(10, '0');

                HIGH = Value.Substring(0, 5);
                LOW = Value.Substring(5, 5);
            }

            return new string[] { HIGH, LOW };
        }

        private void InstructionHasLength(Instructions instruction, string[] arg, string instr)
        {
            switch (instruction)
            {
                case Instructions.MOV: if (arg.Length == 2 || arg.Length == 3) return; break;
                case Instructions.STR: if (arg.Length == 2) return; break;
                case Instructions.LOR: if (arg.Length == 2) return; break;
                case Instructions.PUSH: if (arg.Length == 1) return; break;
                case Instructions.POP: if (arg.Length == 1) return; break;
                case Instructions.ADD: if (arg.Length == 2) return; break;
                case Instructions.STI: if (arg.Length == 2) return; break;
                case Instructions.SUB: if (arg.Length == 2) return; break;
                case Instructions.AND: if (arg.Length == 2) return; break;
                case Instructions.OR: if (arg.Length == 2) return; break;
                case Instructions.NOR: if (arg.Length == 2) return; break;
                case Instructions.CMP: if (arg.Length == 2) return; break;
                case Instructions.JNE: if (arg.Length == 1) return; break;
                case Instructions.INT: if (arg.Length == 0) return; break;
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
                default:
                    ErrorInstructionNotFound(instr);
                    break;
            }
            ErrorInstrLength(instr.Length, arg);
        }

        string ConvTo(int Base, string value, int FromBase)
        {
            return Convert.ToString(Convert.ToUInt16(value, FromBase), Base);
        }
        string ConvTo20(int Base, string value, int FromBase)
        {
            return Convert.ToString(Convert.ToUInt32(value, FromBase), Base);
        }
        string ConvTo32(int Base, string value, int FromBase)
        {
            return Convert.ToString(Convert.ToUInt32(value, FromBase), Base);
        }
        string ConvFrom(ref string value)
        {
            if (value.Last() == 'h')
            {
                value = value.TrimEnd('h').PadLeft(4, '0');
                return value;
            }
            else if (value.Last() == 'b')
            {
                value = value.TrimEnd('b');
                value = ConvTo(16, value, 2).PadLeft(4, '0');
                return value;
            }
            else if (!char.IsDigit(value[0]))
            {
                return "LABLE: " + value;
            }
            else
            {
                value = ConvTo(16, value, 10).PadLeft(4, '0');
                return value;
            }
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
                    return Convert.ToString(instructions[i], 16).PadLeft(2, '0') + "F";
                }
            }
            ErrorInstructionNotFound(instr);
            instruction = Instructions.HALT;
            return "XXXXXX";
        }

        private uint UseVariables(string Name)
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Name.TrimStart('%') == Variables[i].Name)
                {
                    string HexString = Convert.ToString(Variables[i].Addr, 16).PadLeft(5, '0').Remove(0, 1);
                    return Convert.ToUInt32(HexString, 16);
                }
            }
            ErrorVariableNotFound(Name);
            return 0;
        }

        uint UseLable(string name)
        {
            for (int i = 0; i < lables.Count; i++)
            {
                if (name.TrimEnd(':') == lables[i].Name)
                {
                    return lables[i].Addr;
                }
            }
            for (int i = 0; i < GlobalLables.Count; i++)
            {
                if (name.TrimEnd(':') == GlobalLables[i].Name)
                {
                    return GlobalLables[i].Addr;
                }
            }
            ErrorLebleNotFound(name);
            return 0;
        }
        uint UseLableUsingAll(string name)
        {
            for (int i = 0; i < Alllables.Count; i++)
            {
                if (name.TrimEnd(':') == Alllables[i].Name)
                {
                    Tokens.Add(Convert.ToString(Alllables[i].Addr, 16).PadLeft(4, '0') + " | " + "Lable " + name + " ");
                    return Alllables[i].Addr;
                }
            }
            for (int i = 0; i < GlobalLables.Count; i++)
            {
                if (name.TrimEnd(':') == GlobalLables[i].Name)
                {
                    Tokens.Add(Convert.ToString(GlobalLables[i].Addr, 16).PadLeft(4, '0') + " | " + "Global Lable " + name + " ");
                    return GlobalLables[i].Addr;
                }
            }
            ErrorLebleNotFound(name);
            return 0;
        }
        void NewVariable(string instr, string[] arg, int Q)
        {
            if (arg[0] == "=")
            {
                if (Q == 1)
                    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + "Var " + instr + " " + arg[1] + " at " + Convert.ToString(VarPC, 16));
                ConvFrom(ref arg[1]);
                MCcode[VarPC] = arg[1];
                Variables.Add(new Variables()
                {
                    Name = instr.TrimStart('$'),
                    Addr = VarPC
                });
                VarPC++;
            }
            else if (arg[0] == "*=")
            {
                ConvFrom(ref arg[1]);
                if (Q == 1)
                    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + "Var Pointer " + instr + " 0000" + " at " + Convert.ToString(Convert.ToUInt16(arg[1], 16), 16));
                MCcode[Convert.ToUInt16(arg[1], 16)] = "FFFF";
                Variables.Add(new Variables()
                {
                    Name = instr.TrimStart('$'),
                    Addr = Convert.ToUInt16(arg[1], 16)
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
                    if (InDataSection)
                    {
                        if (arg[0].Contains('#') || arg[0].Contains('&'))
                        {
                            Console.WriteLine("Error Var " + "BE201");
                            Environment.Exit(1);
                        }
                        if (Q == 1)
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + instr + " " + arg[0]);

                        ConvFrom(ref arg[0]);
                        if (!char.IsDigit(arg[0][0]))
                        {
                            MCcode[PC] = "LABLE:" + arg[0];
                        }
                        else
                        {
                            MCcode[PC] = arg[0];
                        }
                        PC++;
                    }
                    Assembler = this;
                    break;
                case "str":
                    if (InDataSection)
                    {
                        if (arg[0].Contains('#') || arg[0].Contains('&'))
                        {
                            Console.WriteLine("Error Var " + "BE201");
                            Environment.Exit(1);
                        }

                        string FullString = OrgSrc[LineIndex].Split(' ', 2)[1];

                        if (Q == 1)
                            Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + instr + " " + FullString);

                        if (FullString.StartsWith('"') && FullString.EndsWith('"'))
                        {
                            FullString = FullString.TrimStart('"');
                            FullString = FullString.TrimEnd('"');

                            List<byte> bytes = Encoding.ASCII.GetBytes(FullString).ToList();
                            bytes.Add(0);

                            for (int b = 0; b < bytes.Count; b++)
                            {
                                MCcode[PC] = Convert.ToString(bytes[b], 16).PadLeft(4, '0');
                                PC++;
                            }

                            PC++;
                        }
                        else
                        {
                            //todo error
                            if (Q == 1)
                                Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | ERROR B3032");
                        }
                        Assembler = this;
                    }
                    break;
                case "org":
                    if (arg[0].Contains('#') || arg[0].Contains('&'))
                    {
                        Console.WriteLine("Error Var " + "BE200");
                        Environment.Exit(1);
                    }
                    if (Q == 1)
                        Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + instr + " to " + arg[0]);
                    ConvFrom(ref arg[0]);
                    PC = Convert.ToUInt32(arg[0], 16);
                    Assembler = this;
                    break;
                case "section":

                    //if (Q == 1)
                    //    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | " + instr + " " + arg[0]);
                    switch (arg[0])
                    {
                        case "data":
                            InDataSection = true;
                            InCodeSection = false;
                            break;
                        case "text":
                            InDataSection = false;
                            InCodeSection = true;
                            break;
                    }
                    Assembler = this;
                    break;
                case "data":
                    //if (Q == 1)
                    //    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | in section " + instr);
                    InDataSection = true;
                    InCodeSection = false;
                    break;
                case "text":
                    //if(Q == 1)
                    //    Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | in section " + instr);
                    InDataSection = false;
                    InCodeSection = true;
                    break;
                case "newfile1011":
                    lables.Clear();
                    LineNumber = 0;
                    if (Q == 1)
                        Tokens.Add(Convert.ToString(PC, 16).PadLeft(4, '0') + " | New File " + arg[0]);
                    CurrentFile = arg[0];
                    Assembler = this;
                    break;
                default:
                    ErrorDirInstructionNotFound(instr);
                    break;
            }
        }
    }
}
