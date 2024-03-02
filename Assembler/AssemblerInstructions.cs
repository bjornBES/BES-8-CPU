using System.Formats.Asn1;
using System.Reflection.Emit;
using System.Text;

namespace assembler
{
    public partial class Assembler
    {
        public static class AssemblerInstructions
        {
            public static string AM = "F";
            public static string AM1 = "F";
            public static int offset = 0;
            public static int InstrIndex;
            public static string InstrCode;
            public static string MCcodeBuffer;
            public static string TokenBuffer;
            public static void Doinstr(string instr, string[] arg, bool HasArgs)
            {
                offset = 0;
                AM = "F";
                AM1 = "F";
                InstrIndex = 0;
                InstrCode = GetInstrCode(instr, out Instructions instruction).PadRight(2, '0');
                MCcodeBuffer = InstrCode;

                InstructionHasLength(instruction, arg, instr);

                OutSrc[LineIndex] = Convert.ToString(PC, 16).PadLeft(5, '0') + " " + instr.PadRight(5, ' ') + "\t" + MCcodeBuffer.PadLeft(2, '0') + "/|F ";
                MCcodeBuffer += "/|F";
                //OBJBuffer += MCcodeBuffer;
                TokenBuffer = Convert.ToString(PC, 16).PadLeft(5, '0') + " | " + instr;
                AssemblerLists.MCcode[PC] = MCcodeBuffer;
                PC++;
                if (HasArgs)
                {
                    for (int a = 0; a < arg.Length; a++)
                    {
                        arg[a] = arg[a].TrimStart();
                        // Imm
                        if (arg[a].StartsWith('#'))
                        {
                            arg[a] = arg[a].TrimStart('#');
                            if (arg[a].StartsWith('\"') && arg[a].EndsWith('\"') || arg[a].StartsWith('\'') && arg[a].EndsWith('\''))
                            {
                                arg[a] = arg[a].TrimEnd('\'');
                                arg[a] = arg[a].TrimStart('\'');
                                TokenBuffer += " ImmChar " + arg[a];

                                byte CByte = Encoding.ASCII.GetBytes(arg[a].ToCharArray())[0];
                                string CByteHex = Convert.ToString(CByte, 16);

                                AssemblerLists.MCcode[PC] = CByteHex;
                                PC++;
                                arg[a] = arg[a].TrimEnd('h', 'b');

                                OutSrc[LineIndex] += AssemblerMarcos.ConvTo32(16, CByteHex, 16).PadLeft(5, '0') + " ";
                            }
                            else
                            {
                                TokenBuffer += " Imm " + arg[a];

                                string Value = AssemblerMarcos.ConvFrom(ref arg[a]);

                                AssemblerLists.MCcode[PC] = Value;
                                //OBJBuffer += Value;
                                PC++;
                                arg[a] = arg[a].TrimEnd('h', 'b');
                                int BASE = 16;
                                OutSrc[LineIndex] += AssemblerMarcos.ConvTo32(16, arg[a], BASE).PadLeft(5, '0') + " ";
                            }
                            offset++;
                            if (AM == "F")
                                AM = AssemblerLists.ArgumentIdentifier["imm"];
                            else
                                AM1 = AssemblerLists.ArgumentIdentifier["imm"];
                        }
                        // Indexed
                        else if (arg[a].Contains('&') && !arg[a].Contains("&&"))
                        {
                            string Lable = arg[a].Split('&')[0];
                            string Register = arg[a].Split('&')[1];
                            bool IsRegisterFirst = false;

                            if (Lable.StartsWith('['))
                            {
                                Lable = Lable.Replace("[", "");
                                Lable = Lable.Replace("]", "");
                                if (AssemblerLists.Registers.ContainsKey(Lable))
                                {
                                    IsRegisterFirst = true;
                                }
                                Lable = "[" + Lable + "]";
                                if (AssemblerMarcos.CheakAddr(Lable) == "")
                                {
                                    AssemblerErrors.ErrorSyntax(OrgSrc[LineIndex], Lable);
                                    if(IsRegisterFirst) AM = "F";
                                }
                            }
                            else if (Lable.StartsWith('%'))
                            {
                                AssemblerMarcos.CheakAddr(Lable);
                            }
                            else
                            {
                                AssemblerErrors.ErrorSyntax("ARGS");
                            }
                            TokenBuffer += " Indexed " + Lable + " " + Register;
                            if (AssemblerLists.Registers.TryGetValue(Register, out ushort Value))
                            {
                                if (IsRegisterFirst)
                                {
                                    if (AM == "F")
                                        AM = AssemblerLists.ArgumentIdentifier["IndexRegAddrReg"];
                                    else
                                        AM1 = AssemblerLists.ArgumentIdentifier["IndexRegAddrReg"];
                                }
                                else
                                {
                                    if (AM == "F")
                                        AM = AssemblerLists.ArgumentIdentifier["IndexReg"];
                                    else
                                        AM1 = AssemblerLists.ArgumentIdentifier["IndexReg"];
                                }
                                string RegValue = AssemblerMarcos.ConvTo(16, Value.ToString().PadLeft(6, '0'), 10).PadLeft(4, '0');
                                AssemblerLists.MCcode[PC] = RegValue;
                                //OBJBuffer = RegValue;
                            }
                            else if (AssemblerMarcos.IsNumbers(Register))
                            {
                                if (IsRegisterFirst)
                                {
                                    if (AM == "F")
                                        AM = AssemblerLists.ArgumentIdentifier["IndexRegAddrImm"];
                                    else
                                        AM1 = AssemblerLists.ArgumentIdentifier["IndexRegAddrImm"];
                                }
                                else
                                {
                                    if (AM == "F")
                                        AM = AssemblerLists.ArgumentIdentifier["IndexImm"];
                                    else
                                        AM1 = AssemblerLists.ArgumentIdentifier["IndexImm"];
                                }
                                AssemblerLists.MCcode[PC] = AssemblerMarcos.ConvFrom(ref Register);
                                //OBJBuffer = "+" + Register;
                            }
                            else
                            {
                                AssemblerErrors.ErrorRegisterNotFound(Register);
                            }
                            PC++;
                            offset++;
                        }
                        // addr
                        else if (arg[a].StartsWith('[') && arg[a].Contains(']'))
                        {
                            AssemblerMarcos.CheakAddr(arg[a]);
                        }
                        // register
                        else if (AssemblerLists.Registers.TryGetValue(arg[a], out ushort Value))
                        {
                            TokenBuffer += " Reg " + arg[a];
                            string RegValue = AssemblerMarcos.ConvTo(16, Value.ToString().PadLeft(6, '0'), 10).PadLeft(4, '0');
                            //OBJBuffer += RegValue;
                            AssemblerLists.MCcode[PC] = RegValue;
                            OutSrc[LineIndex] += RegValue.PadLeft(4, '0') + " ";
                            if (AM == "F")
                                AM = "2";
                            else
                                AM1 = "2";
                            PC++;
                            offset++;
                        }
                        // variable
                        else if (arg[a].StartsWith('%'))
                        {
                            AssemblerMarcos.CheakAddr(arg[a]);
                        }
                        // curent addr
                        else if (arg[a].StartsWith('$'))
                        {
                            if (AM == "F")
                                AM = AssemblerLists.ArgumentIdentifier["addr"];
                            else
                                AM1 = AssemblerLists.ArgumentIdentifier["addr"];
                            uint CopyPC = (uint)(PC - (offset + 1));

                            if (arg[a].Contains('+'))
                            {
                                uint add = uint.Parse(arg[a].Split('+')[1]);
                                CopyPC += add;
                            }

                            string addr = Convert.ToString(CopyPC, 16) + "h";
                            TokenBuffer += " Get current Addr at " + AssemblerMarcos.ConvTo(16, CopyPC.ToString(), 10);

                            string AddrValue = AssemblerMarcos.ConvFrom(ref addr);

                            AssemblerLists.MCcode[PC] = AddrValue;
                            PC++;
                            addr = addr.TrimEnd('h', 'b');
                            OutSrc[LineIndex] += AssemblerMarcos.ConvTo(16, CopyPC.ToString(), 10).PadLeft(4, '0') + " ";
                            offset++;
                        }
                        else
                        {
                            AssemblerErrors.ErrorSyntax("ARGS ERROR LABLE");
                        }

                    }
                    AssemblerLists.MCcode[PC - (offset + 1)] = AssemblerLists.MCcode[PC - (offset + 1)].Replace("/", AM);
                    OutSrc[LineIndex] = OutSrc[LineIndex].Replace("/", AM);
                    //Console.WriteLine((PC - (offset + 1)).ToString().PadLeft(4, '0') + ": " + instr.PadRight(5, ' ') + "\t" + MCcode[Math.Max(PC - (offset + 1), 0)]);
                }
                else
                {
                }
                AssemblerLists.Tokens.Add(TokenBuffer);
                uint Off = (uint)(PC - (offset + 1));

                OutSrc[LineIndex] = OutSrc[LineIndex].Replace("|", AM1);
                OutSrc[LineIndex] = OutSrc[LineIndex].Replace("/", AM).PadLeft(4, '0');
                AssemblerLists.MCcode[Off] = AssemblerLists.MCcode[Off].Replace("|", AM1);
                AssemblerLists.MCcode[Off] = AssemblerLists.MCcode[Off].Replace("/", AM);

                string[] OBJcopy = new string[3];
                Array.Copy(AssemblerLists.MCcode, Off, OBJcopy, 0, 3);
                AssemblerObj.AddInstr(OBJcopy);
            }

            public static string GetInstrCode(string instr, out Instructions instruction)
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
                AssemblerErrors.ErrorInstructionNotFound(instr);
                instruction = Instructions.NOP;
                return "00000";
            }
            public static void InstructionHasLength(Instructions instruction, string[] arg, string instr)
            {
                switch (instruction)
                {
                    case Instructions.NOP: if (arg.Length == 1) return; break;
                    case Instructions.MOV: if (arg.Length == 2) return; break;
                    case Instructions.PUSH: if (arg.Length == 1) return; break;
                    case Instructions.POP: if (arg.Length == 1) return; break;
                    case Instructions.ADD: if (arg.Length == 2) return; break;
                    case Instructions.SUB: if (arg.Length == 2) return; break;
                    case Instructions.MUL: if (arg.Length == 2) return; break;
                    case Instructions.DIV: if (arg.Length == 2) return; break;
                    case Instructions.AND: if (arg.Length == 2) return; break;
                    case Instructions.OR: if (arg.Length == 2) return; break;
                    case Instructions.NOT: if (arg.Length == 1) return; break;
                    case Instructions.NOR: if (arg.Length == 2) return; break;
                    case Instructions.ROL: if (arg.Length == 2) return; break;
                    case Instructions.ROR: if (arg.Length == 2) return; break;
                    case Instructions.JMP: if (arg.Length == 1) return; break;
                    case Instructions.CMP: if (arg.Length == 2) return; break;
                    case Instructions.JLE: if (arg.Length == 1) return; break;
                    case Instructions.JE: if (arg.Length == 1) return; break;
                    case Instructions.JGE: if (arg.Length == 1) return; break;
                    case Instructions.JG: if (arg.Length == 1) return; break;
                    case Instructions.JNE: if (arg.Length == 1) return; break;
                    case Instructions.JL: if (arg.Length == 1) return; break;
                    case Instructions.JER: if (arg.Length == 1) return; break;
                    case Instructions.JMC: if (arg.Length == 1) return; break;
                    case Instructions.JMZ: if (arg.Length == 1) return; break;
                    case Instructions.JNC: if (arg.Length == 1) return; break;
                    case Instructions.INT: if (arg.Length == 1) return; break;
                    case Instructions.CALL: if (arg.Length == 1) return; break;
                    case Instructions.RTS: if (arg.Length == 0) return; break;
                    case Instructions.RET: if (arg.Length == 1) return; break;
                    case Instructions.PUSHR: if (arg.Length == 0) return; break;
                    case Instructions.POPR: if (arg.Length == 0) return; break;
                    case Instructions.INC: if (arg.Length == 1) return; break;
                    case Instructions.DEC: if (arg.Length == 1) return; break;
                    case Instructions.IN: if (arg.Length == 2) return; break;
                    case Instructions.OUT: if (arg.Length == 2) return; break;
                    case Instructions.CLF: if (arg.Length == 1) return; break;
                    case Instructions.SEF: if (arg.Length == 1) return; break;
                    case Instructions.XOR: if (arg.Length == 2) return; break;
                    case Instructions.JMS: if (arg.Length == 1) return; break;
                    case Instructions.JNS: if (arg.Length == 1) return; break;
                    default:
                        AssemblerErrors.ErrorInstructionNotFound(instr);
                        break;
                }
                AssemblerErrors.ErrorInstrLength(instr.Length, arg);
            }
        }
    }
}
