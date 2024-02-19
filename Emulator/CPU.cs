using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;

namespace emu
{
    public class CPU
    {
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

        public const ushort Flag_Zero =         0;
        public const ushort Flag_Equal =        1;
        public const ushort Flag_Less =         2;
        public const ushort Flag_Carry =        3;
        public const ushort Flag_Halt =         4;
        public const ushort Flag_Sing =         5;
        public const ushort Flag_Parity =       6;
        public const ushort Flag_Error =        7;

        public static Dictionary<ushort, ArgumentIdent> ArgumentIdentifier = new Dictionary<ushort, ArgumentIdent>();

        public void RESET()
        {
            ArgumentIdentifier.Clear();
            ArgumentIdentifier.Add(0, ArgumentIdent.Imm);
            ArgumentIdentifier.Add(1, ArgumentIdent.Addr);
            ArgumentIdentifier.Add(2, ArgumentIdent.Reg);
            ArgumentIdentifier.Add(3, ArgumentIdent.RegAddr);
            ArgumentIdentifier.Add(4, ArgumentIdent.IndexReg);
            ArgumentIdentifier.Add(5, ArgumentIdent.IndexImm);

            SP = 0x85FF;
            MB = 0;
            F = 0;
            AX = BX = CX = DX = ZX = 0;
            X = Y = 0;
            LoadRegister(Registers.PC, (uint)0);
            //Ports.RESETAll(ref mem);
            Setflag(Flag_Equal, 0);
        }

        public void TICK()
        {
            uint Instr = FetchInstr();
            Instructions instructions = DecodeInstr(Instr, out ushort AM1, out ushort AM2);
            ExecuteInstr(instructions, AM1, AM2);
        }

        private uint FetchInstr()
        {
            return ReadMem(PC++, 0);
        }

        private Instructions DecodeInstr(uint instr, out ushort AM1, out ushort AM2)
        {
            ushort Instr = (ushort)(instr & 0xFF00);
            AM1 = (ushort)(instr & 0x00F0);
            AM2 = (ushort)(instr & 0x000F);

            if(Enum.TryParse(typeof(Instructions), Instr.ToString(), out object result))
            {
                Instructions instructions;
                instructions = (Instructions)result;
                return instructions; 
            }
            return Instructions.NOP;
        }

        private void ExecuteInstr(Instructions instructions, ushort AM1, ushort AM2)
        {
            ArgumentIdent argumentIdent1 = ArgumentIdentifier[AM1];
            ArgumentIdent argumentIdent2 = ArgumentIdentifier[AM2];
            switch (instructions)
            {
                case Instructions.NOP: break;
                case Instructions.MOV:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm: 
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.PUSH:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.POP:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.ADD:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.SUB:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.MUL:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.DIV:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.AND:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.OR:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.NOT:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.NOR:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.ROL:
                    break;
                case Instructions.ROR:
                    break;
                case Instructions.JMP:
                    break;
                case Instructions.CMP:
                    break;
                case Instructions.JLE:
                    break;
                case Instructions.JE:
                    break;
                case Instructions.JGE:
                    break;
                case Instructions.JG:
                    break;
                case Instructions.JNE:
                    break;
                case Instructions.JL:
                    break;
                case Instructions.JER:
                    break;
                case Instructions.JMC:
                    break;
                case Instructions.JMZ:
                    break;
                case Instructions.JNC:
                    break;
                case Instructions.INT:
                    break;
                case Instructions.CALL:
                    break;
                case Instructions.RTS:
                    break;
                case Instructions.RET:
                    break;
                case Instructions.PUSHR:
                    break;
                case Instructions.POPR:
                    break;
                case Instructions.INC:
                    break;
                case Instructions.DEC:
                    break;
                case Instructions.IN:
                    break;
                case Instructions.OUT:
                    break;
                case Instructions.CLF:
                    break;
                case Instructions.SEF:
                    break;
                case Instructions.XOR:
                    break;
                case Instructions.JMS:
                    break;
                case Instructions.JNS:
                    break;
            }
        }
        void MOV(ArgumentIdent AM1, ArgumentIdent AM2)
        {
            dynamic Data;
            switch (AM1)
            {
                case ArgumentIdent.Addr:
                    Data = FetchInstr();
                    break;
                case ArgumentIdent.Reg:
                    Data = DecodeRegister(FetchInstr());
                    break;
                case ArgumentIdent.IndexReg:
                    uint Indexed_Addr = FetchInstr();
                    Register Indexed_Reg = GetRegValue(DecodeRegister(FetchInstr()));

                    switch (AM2)
                    {
                        case ArgumentIdent.Imm:
                            break;
                        case ArgumentIdent.Addr:
                            break;
                        case ArgumentIdent.Reg:
                            break;
                        case ArgumentIdent.RegAddr:
                            break;
                        case ArgumentIdent.IndexReg:
                            break;
                        case ArgumentIdent.IndexImm:
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        #region Instruction
        void LoadRegister(Registers registers, uint ImmValue)
        {
            LoadReg(registers, ImmValue);
        }
        void LoadRegister(Registers registers, Registers registers1)
        {
            LoadReg(registers, GetRegValue(registers1).m_value);
        }
        void LoadRegister(Registers registers, uint Addr, ushort Bank)
        {
            LoadReg(registers, MEM.Read(Bank, Addr));
        }
        void LoadRegister(Registers registers, uint Addr, uint Indexed, ushort Bank)
        {
            LoadReg(registers, MEM.Read(Bank, Addr + Indexed));
        }
        uint ReadMem(uint Addr, ushort MB)
        {
            return MEM.Read(Addr, MB);
        }
        uint ReadMem(Register Addr, ushort MB)
        {
            return MEM.Read(Addr, MB);
        }
        uint ReadMem(Register Addr)
        {
            return MEM.Read(Addr, MB);
        }
        uint ReadMem(ushort Addr)
        {
            return MEM.Read(Addr, MB);
        }
        #endregion

        private Registers DecodeRegister(uint reg)
        {
            return (Registers)Enum.Parse(typeof(Registers), reg.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="flag">jumps imm if 255</param>
        /// <param name="value"></param>
        void Jump(uint addr, uint flag, uint value)
        {
            if(flag == 0xFF)
            {
                PC = addr;
            }
            else
            {
                if(Getflag(flag) == value)
                {
                    PC = addr;
                }
            }
        }


        void LoadReg(Registers reg, uint data)
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
        public Register GetLow(Register reg)
        {
            return reg.GetLowByte();
        }

        public Register GetHigh(Register reg)
        {
            return reg.GetHighByte();
        }
        Register GetRegValue(Registers register)
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
        public void Setflag(ushort flag, ushort value)
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
    }
}
