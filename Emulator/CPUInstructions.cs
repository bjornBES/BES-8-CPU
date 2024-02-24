using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emulator
{
    public class CPUInstructions
    {
        public const uint Bank_Stack = 1;

        public Register AX = 0;
        public Register BX = 0;
        public Register CX = 0;
        public Register DX = 0;
        public Register ZX = 0;
        public Register PC = 0;
        public Register SP = 0;
        public Register MB = 0;
        public Register X = 0;
        public Register Y = 0;

        public Register F = 0;

        public static bool Running;

        public const ushort Flag_Zero = 0;
        public const ushort Flag_Equal = 1;
        public const ushort Flag_Less = 2;
        public const ushort Flag_Carry = 3;
        public const ushort Flag_Halt = 4;
        public const ushort Flag_Sing = 5;
        public const ushort Flag_Parity = 6;
        public const ushort Flag_Error = 7;

        public static Dictionary<ushort, ArgumentIdent> ArgumentIdentifier = new Dictionary<ushort, ArgumentIdent>();
        public Instructions DecodeInstr(uint instr, out ushort AM1, out ushort AM2)
        {
            ushort Instr =  (ushort)((instr & 0xFF000) >> 12);
            AM1 =           (ushort)((instr & 0x00F00) >> 8);
            AM2 =           (ushort)((instr & 0x000F0) >> 4);

            if (Enum.TryParse(typeof(Instructions), Instr.ToString(), out object result))
            {
                Instructions instructions;
                instructions = (Instructions)result;
                return instructions;
            }
            return Instructions.NOP;
        }
        public uint FetchInstr()
        {
            return MEM.Read(0, PC++);
        }
        public void MOV(ArgumentIdent AM1, ArgumentIdent AM2)
        {
            // mov AX #0
            Registers register;
            uint addr;
            uint TOAddr;
            switch (AM1)
            {
                case ArgumentIdent.Addr:
                    TOAddr = FetchInstr();
                    switch (AM2)
                    {
                        case ArgumentIdent.Imm:
                            MEM.Write(MB, TOAddr, FetchInstr());
                            break;
                        case ArgumentIdent.Addr:
                            MEM.Write(MB, TOAddr, ReadMem(FetchInstr()));
                            break;
                        case ArgumentIdent.Reg:
                            MEM.Write(MB, TOAddr, GetRegValue(DecodeRegister(FetchInstr())));
                            break;
                        case ArgumentIdent.RegAddr:
                            MEM.Write(MB, TOAddr, ReadMem(GetRegValue(DecodeRegister(FetchInstr()))));
                            break;
                        case ArgumentIdent.IndexReg:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexImm:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrReg:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrImm:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        default:
                            break;
                    }
                    break;
                case ArgumentIdent.Reg:
                    register = DecodeRegister(FetchInstr());
                    switch (AM2)
                    {
                        case ArgumentIdent.Imm:
                            LoadRegister(register, FetchInstr());
                            break;
                        case ArgumentIdent.Addr:
                            LoadRegister(register, ReadMem(FetchInstr()));
                            break;
                        case ArgumentIdent.Reg:
                            LoadRegister(register, DecodeRegister(FetchInstr()));
                            break;
                        case ArgumentIdent.RegAddr:
                            LoadRegister(register, ReadMem(GetRegValue(DecodeRegister(FetchInstr()))));
                            break;
                        case ArgumentIdent.IndexReg:
                            addr = FetchInstr();
                            LoadRegister(register,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexImm:
                            addr = FetchInstr();
                            LoadRegister(register,
                                ReadMem(FetchInstr() + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrReg:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            LoadRegister(register,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrImm:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            LoadRegister(register,
                                ReadMem(FetchInstr() + addr));
                            break;
                        default:
                            break;
                    }
                    break;
                case ArgumentIdent.RegAddr:
                    register = DecodeRegister(FetchInstr());
                    switch (AM2)
                    {
                        case ArgumentIdent.Imm:
                            MEM.Write(MB, GetRegValue(register), FetchInstr());
                            break;
                        case ArgumentIdent.Addr:
                            MEM.Write(MB, GetRegValue(register), ReadMem(FetchInstr()));
                            break;
                        case ArgumentIdent.Reg:
                            MEM.Write(MB, GetRegValue(register), GetRegValue(DecodeRegister(FetchInstr())));
                            break;
                        case ArgumentIdent.RegAddr:
                            MEM.Write(MB, GetRegValue(register), ReadMem(GetRegValue(DecodeRegister(FetchInstr()))));
                            break;
                        case ArgumentIdent.IndexReg:
                            addr = FetchInstr();
                            MEM.Write(MB, GetRegValue(register),
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexImm:
                            addr = FetchInstr();
                            MEM.Write(MB, GetRegValue(register),
                                ReadMem(FetchInstr() + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrReg:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, GetRegValue(register),
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrImm:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, GetRegValue(register),
                                ReadMem(FetchInstr() + addr));
                            break;
                        default:
                            break;
                    }
                    break;
                case ArgumentIdent.IndexReg:
                    TOAddr = FetchInstr() /*base addr*/ + GetRegValue(DecodeRegister(FetchInstr())).m_value /*offset*/;
                    switch (AM2)
                    {
                        case ArgumentIdent.Imm:
                            MEM.Write(MB, TOAddr, FetchInstr());
                            break;
                        case ArgumentIdent.Addr:
                            MEM.Write(MB, TOAddr, ReadMem(FetchInstr()));
                            break;
                        case ArgumentIdent.Reg:
                            MEM.Write(MB, TOAddr, GetRegValue(DecodeRegister(FetchInstr())));
                            break;
                        case ArgumentIdent.RegAddr:
                            MEM.Write(MB, TOAddr, ReadMem(GetRegValue(DecodeRegister(FetchInstr()))));
                            break;
                        case ArgumentIdent.IndexReg:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexImm:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrReg:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrImm:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        default:
                            break;
                    }
                    break;
                case ArgumentIdent.IndexImm:
                    TOAddr = FetchInstr() /*base addr*/ + FetchInstr() /*offset*/;
                    switch (AM2)
                    {
                        case ArgumentIdent.Imm:
                            MEM.Write(MB, TOAddr, FetchInstr());
                            break;
                        case ArgumentIdent.Addr:
                            MEM.Write(MB, TOAddr, ReadMem(FetchInstr()));
                            break;
                        case ArgumentIdent.Reg:
                            MEM.Write(MB, TOAddr, GetRegValue(DecodeRegister(FetchInstr())));
                            break;
                        case ArgumentIdent.RegAddr:
                            MEM.Write(MB, TOAddr, ReadMem(GetRegValue(DecodeRegister(FetchInstr()))));
                            break;
                        case ArgumentIdent.IndexReg:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexImm:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrReg:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrImm:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        default:
                            break;
                    }
                    break;
                case ArgumentIdent.IndexRegAddrReg:
                    TOAddr = GetRegValue(DecodeRegister(FetchInstr())).m_value /*base addr*/ + GetRegValue(DecodeRegister(FetchInstr())).m_value /*offset*/;
                    switch (AM2)
                    {
                        case ArgumentIdent.Imm:
                            MEM.Write(MB, TOAddr, FetchInstr());
                            break;
                        case ArgumentIdent.Addr:
                            MEM.Write(MB, TOAddr, ReadMem(FetchInstr()));
                            break;
                        case ArgumentIdent.Reg:
                            MEM.Write(MB, TOAddr, GetRegValue(DecodeRegister(FetchInstr())));
                            break;
                        case ArgumentIdent.RegAddr:
                            MEM.Write(MB, TOAddr, ReadMem(GetRegValue(DecodeRegister(FetchInstr()))));
                            break;
                        case ArgumentIdent.IndexReg:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexImm:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrReg:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrImm:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        default:
                            break;
                    }
                    break;
                case ArgumentIdent.IndexRegAddrImm:
                    TOAddr = GetRegValue(DecodeRegister(FetchInstr())).m_value /*base addr*/ + FetchInstr() /*offset*/;
                    switch (AM2)
                    {
                        case ArgumentIdent.Imm:
                            MEM.Write(MB, TOAddr, FetchInstr());
                            break;
                        case ArgumentIdent.Addr:
                            MEM.Write(MB, TOAddr, ReadMem(FetchInstr()));
                            break;
                        case ArgumentIdent.Reg:
                            MEM.Write(MB, TOAddr, GetRegValue(DecodeRegister(FetchInstr())));
                            break;
                        case ArgumentIdent.RegAddr:
                            MEM.Write(MB, TOAddr, ReadMem(GetRegValue(DecodeRegister(FetchInstr()))));
                            break;
                        case ArgumentIdent.IndexReg:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexImm:
                            addr = FetchInstr();
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrReg:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(GetRegValue(DecodeRegister(FetchInstr())).m_value + addr));
                            break;
                        case ArgumentIdent.IndexRegAddrImm:
                            addr = MEM.Read(MB, GetRegValue(DecodeRegister(FetchInstr())).m_value);
                            MEM.Write(MB, TOAddr,
                                ReadMem(FetchInstr() + addr));
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        public void PUSH(ArgumentIdent AM1)
        {
            switch (AM1)
            {
                case ArgumentIdent.Imm:
                    Push(FetchInstr());
                    break;
                case ArgumentIdent.Addr:
                    Push(FetchInstr(), MB.m_value);
                    break;
                case ArgumentIdent.Reg:
                    Push(DecodeRegister(FetchInstr()));
                    break;
                case ArgumentIdent.RegAddr:
                    Push(ReadMem(GetRegValue(DecodeRegister(FetchInstr()))));
                    break;
                default:
                    break;
            }
        }
        public void POP(ArgumentIdent AM1)
        {
            switch (AM1)
            {
                case ArgumentIdent.Addr:
                    Pop(FetchInstr());
                    break;
                case ArgumentIdent.Reg:
                    Pop(out Register register);
                    LoadRegister(DecodeRegister(FetchInstr()), register.m_value);
                    break;
                case ArgumentIdent.RegAddr:
                    Pop(out register);
                    WriteMem(GetRegValue(DecodeRegister(FetchInstr())), register);
                    break;
            }
        }
        public void ALU(ArgumentIdent AM1, ArgumentIdent AM2, Instructions instructions)
        {
            uint destination;
            uint source;

            switch (AM1)
            {
                case ArgumentIdent.Addr:
                case ArgumentIdent.Reg:
                case ArgumentIdent.RegAddr:
                    destination = FetchInstr();
                    break;
                default:
                    return;
            }

            switch (AM2)
            {
                case ArgumentIdent.Imm:
                    source = FetchInstr();
                    break;
                case ArgumentIdent.Addr:
                    source = ReadMem(FetchInstr());
                    break;
                case ArgumentIdent.Reg:
                    source = GetRegValue(DecodeRegister(FetchInstr())).m_value;
                    break;
                case ArgumentIdent.RegAddr:
                    source = ReadMem(GetRegValue(DecodeRegister(FetchInstr())));
                    break;
                default:
                    return;
            }

            uint Result;
            switch (instructions)
            {
                case Instructions.ADD:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = GetRegValue(DecodeRegister(destination)).m_value + source;
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ReadMem(destination) + source;
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.SUB:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = GetRegValue(DecodeRegister(destination)).m_value - source;
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ReadMem(destination) - source;
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.MUL:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = GetRegValue(DecodeRegister(destination)).m_value * source;
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ReadMem(destination) * source;
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.DIV:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = GetRegValue(DecodeRegister(destination)).m_value / source;
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ReadMem(destination) / source;
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.AND:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = GetRegValue(DecodeRegister(destination)).m_value & source;
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ReadMem(destination) & source;
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.OR:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = GetRegValue(DecodeRegister(destination)).m_value | source;
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ReadMem(destination) | source;
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.NOT:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = ~GetRegValue(DecodeRegister(destination)).m_value;
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ~ReadMem(destination);
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.NOR:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = ~GetRegValue(DecodeRegister(destination)).m_value | source;
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ~ReadMem(destination) | source;
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.ROL:
                    LoadReg(DecodeRegister(destination),(destination << (int)source) | (destination >> (int)(32 - source)));
                    break;
                case Instructions.ROR:
                    LoadReg(DecodeRegister(destination),(destination >> (int)source) | (destination << (int)(32 - source)));
                    break;
                case Instructions.CMP:
                    Setflag(Flag_Equal, 0);
                    Setflag(Flag_Less, 0);
                    Setflag(Flag_Zero, 0);
                    Setflag(Flag_Sing, 0);
                    Setflag(Flag_Parity, 0);

                    if (destination == source) Setflag(Flag_Equal, 1);
                    if (destination < source) Setflag(Flag_Less, 1);
                    if (destination == 0) Setflag(Flag_Zero, 1);
                    if ((destination & 0x80000) == 0x80000) Setflag(Flag_Sing, 1);
                    if (destination % 3 == 1) Setflag(Flag_Parity, 1);
                    break;
                case Instructions.XOR:
                    switch (AM1)
                    {
                        case ArgumentIdent.Reg:
                            Result = GetRegValue(DecodeRegister(destination)).m_value | source & ~(GetRegValue(DecodeRegister(destination)).m_value & source);
                            LoadRegister(DecodeRegister(destination), Result);
                            break;
                        case ArgumentIdent.Addr:
                        case ArgumentIdent.RegAddr:
                            Result = ReadMem(destination) | source & ~(ReadMem(destination) & source);
                            WriteMem(destination, Result);
                            break;
                    }
                    break;
                case Instructions.SHL:
                    LoadReg(DecodeRegister(destination),(uint)(destination << (int)source));
                    break;
                case Instructions.SHR:
                    LoadReg(DecodeRegister(destination),(uint)(destination >> (int)source));
                    break;
                default:
                    break;
            }
        }
        #region Instruction
        public void LoadRegister(Registers registers, uint ImmValue)
        {
            LoadReg(registers, ImmValue);
        }
        public void LoadRegister(Registers registers, Registers registers1)
        {
            LoadReg(registers, GetRegValue(registers1).m_value);
        }
        public void LoadRegister(Registers registers, uint Addr, uint Bank)
        {
            LoadReg(registers, MEM.Read(Bank, Addr));
        }
        public void LoadRegister(Registers registers, uint Addr, uint Indexed, uint Bank)
        {
            LoadReg(registers, MEM.Read(Bank, Addr + Indexed));
        }
        public uint ReadMem(uint Addr, uint MB)
        {
            return MEM.Read(MB, Addr);
        }
        public uint ReadMem(Register Addr, uint MB)
        {
            return MEM.Read(MB, Addr);
        }
        public uint ReadMem(Register Addr)
        {
            return MEM.Read(MB, Addr);
        }
        public uint ReadMem(uint Addr)
        {
            return MEM.Read(MB, Addr);
        }
        public void WriteMem(Register Addr, Register data, Register MB)
        {
            MEM.Write(MB, Addr, data);
        }
        public void WriteMem(uint Addr, Register data, Register MB)
        {
            MEM.Write(MB, Addr, data);
        }
        public void WriteMem(Register Addr, uint data, Register MB)
        {
            MEM.Write(MB, Addr, data);
        }
        public void WriteMem(uint Addr, uint data, Register MB)
        {
            MEM.Write(MB, Addr, data);
        }
        public void WriteMem(Register Addr, Register data, uint MB)
        {
            MEM.Write(MB, Addr, data);
        }
        public void WriteMem(uint Addr, Register data, uint MB)
        {
            MEM.Write(MB, Addr, data);
        }
        public void WriteMem(Register Addr, uint data, uint MB)
        {
            MEM.Write(MB, Addr, data);
        }
        public void WriteMem(uint Addr, uint data, uint MB)
        {
            MEM.Write(MB, Addr, data);
        }
        public void WriteMem(Register Addr, Register data)
        {
            MEM.Write(MB.m_value, Addr, data);
        }
        public void WriteMem(uint Addr, Register data)
        {
            MEM.Write(MB.m_value, Addr, data);
        }
        public void WriteMem(Register Addr, uint data)
        {
            MEM.Write(MB.m_value, Addr, data);
        }
        public void WriteMem(uint Addr, uint data)
        {
            MEM.Write(MB.m_value, Addr, data);
        }
        public void Push(uint Data)
        {
            WriteMem(SP, Data, Bank_Stack);
            SP--;
        }
        public void Push(Register Data)
        {
            WriteMem(SP, Data, Bank_Stack);
            SP--;
        }
        public void Push(uint Addr, uint Bank)
        {
            WriteMem(SP, ReadMem(Addr, Bank_Stack), Bank_Stack);
            SP--;
        }
        public void Push(Registers Data)
        {
            WriteMem(SP, GetRegValue(Data), Bank_Stack);
            SP--;
        }
        public void Pop(out Register Data)
        {
            SP++;
            Data = ReadMem(SP, Bank_Stack);
        }
        public void Pop(uint Addr)
        {
            SP++;
            WriteMem(Addr, ReadMem(SP, Bank_Stack));
        }
        public uint Pop()
        {
            SP++;
            return ReadMem(SP, Bank_Stack);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="flag">jumps imm if 255</param>
        /// <param name="value"></param>
        public void Jump(uint addr, uint flag, uint value)
        {
            if (flag == 0xFF)
            {
                PC = addr;
            }
            else
            {
                if (Getflag(flag) == value)
                {
                    PC = addr;
                }
            }
        }
        public void JumpAnd(uint addr, uint flag1, uint value1, uint flag2, uint value2)
        {
            if (Getflag(flag1) == value1 && Getflag(flag2) == value2)
            {
                PC = addr;
            }
        }
        public void JumpOr(uint addr, uint flag1, uint value1, uint flag2, uint value2)
        {
            if (Getflag(flag1) == value1 || Getflag(flag2) == value2)
            {
                PC = addr;
            }
        }
        public void LoadReg(Registers reg, uint data)
        {
            if (data == 0) Setflag(Flag_Zero, 1);
            else Setflag(Flag_Zero, 0);
            switch (reg)
            {
                case Registers.AX:
                    AX = data;
                    break;
                case Registers.AL:
                    AX.SetLowByte((ushort)data);
                    break;
                case Registers.AH:
                    AX.SetHighByte((ushort)data);
                    break;
                case Registers.BX:
                    BX = data;
                    break;
                case Registers.BL:
                    BX.SetLowByte((ushort)data);
                    break;
                case Registers.BH:
                    BX.SetHighByte((ushort)data);
                    break;
                case Registers.CX:
                    CX = data;
                    break;
                case Registers.CL:
                    CX.SetLowByte((ushort)data);
                    break;
                case Registers.CH:
                    CX.SetHighByte((ushort)data);
                    break;
                case Registers.DX:
                    DX = data;
                    break;
                case Registers.DL:
                    DX.SetLowByte((ushort)data);
                    break;
                case Registers.DH:
                    DX.SetHighByte((ushort)data);
                    break;
                case Registers.ZX:
                    ZX = data;
                    break;
                case Registers.ZL:
                    ZX.SetLowByte((ushort)data);
                    break;
                case Registers.ZH:
                    ZX.SetHighByte((ushort)data);
                    break;
                case Registers.PC:
                    PC = data;
                    break;
                case Registers.SP:
                    SP = data;
                    break;
                case Registers.MB:
                    MB = data;
                    break;
                case Registers.X:
                    X = data;
                    break;
                case Registers.XL:
                    X.SetLowByte((ushort)data);
                    break;
                case Registers.XH:
                    X.SetHighByte((ushort)data);
                    break;
                case Registers.Y:
                    Y = data;
                    break;
                case Registers.YL:
                    Y.SetLowByte((ushort)data);
                    break;
                case Registers.YH:
                    Y.SetHighByte((ushort)data);
                    break;
                case Registers.F:
                    F = data;
                    break;
                default:
                    break;
            }
        }
        public Register GetRegValue(Registers register)
        {
            switch (register)
            {
                case Registers.AX:
                    return AX;
                case Registers.AL:
                    return AX.GetLowByte();
                case Registers.AH:
                    return AX.GetHighByte();
                case Registers.BX:
                    return BX;
                case Registers.BL:
                    return BX.GetLowByte();
                case Registers.BH:
                    return BX.GetHighByte();
                case Registers.CX:
                    return CX;
                case Registers.CL:
                    return CX.GetLowByte();
                case Registers.CH:
                    return CX.GetHighByte();
                case Registers.DX:
                    return DX;
                case Registers.DL:
                    return DX.GetLowByte();
                case Registers.DH:
                    return DX.GetHighByte();
                case Registers.ZX:
                    return ZX;
                case Registers.ZL:
                    return ZX.GetLowByte();
                case Registers.ZH:
                    return ZX.GetHighByte();
                case Registers.PC:
                    return PC;
                case Registers.SP:
                    return SP;
                case Registers.MB:
                    return MB;
                case Registers.X:
                    return X;
                case Registers.XL:
                    return X.GetLowByte();
                case Registers.XH:
                    return X.GetHighByte();
                case Registers.Y:
                    return Y;
                case Registers.YL:
                    return Y.GetLowByte();
                case Registers.YH:
                    return Y.GetHighByte();
                case Registers.F:
                    return F;
                default:
                    return 0;
            }
        }
        public uint Getflag(uint flag)
        {
            char[] StrFlags = Convert.ToString(F.m_value, 2).PadLeft(16, '0').ToCharArray();

            Array.Reverse(StrFlags);

            return Convert.ToUInt16(StrFlags[flag].ToString());
        }
        public void Setflag(uint flag, ushort value)
        {
            if (value == 1)
            {
                F.m_value |= flag;
            }
            else
            {
                F.m_value &= (ushort)~flag;
            }
        }
        public Registers DecodeRegister(uint reg)
        {
            return (Registers)Enum.Parse(typeof(Registers), reg.ToString());
        }
        public Register GetLow(Register reg)
        {
            return reg.GetLowByte();
        }

        public Register GetHigh(Register reg)
        {
            return reg.GetHighByte();
        }

        public void CheakStackPointerOverflowError()
        {
            if(SP > 0x1FFF || SP < 0x1FF)
            {
                Setflag(Flag_Error, 1);
                LoadReg(Registers.ZX, 0xF8021); //1111_1000_0000_0010_0001
            }
        }
    }
}
