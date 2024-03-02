using Microsoft.VisualBasic;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;

namespace emulator
{
    public class CPU : CPUInstructions
    {
        public void RESET()
        {
            ArgumentIdentifier.Clear();
            ArgumentIdentifier.Add(0, ArgumentIdent.Imm);
            ArgumentIdentifier.Add(1, ArgumentIdent.Addr);
            ArgumentIdentifier.Add(2, ArgumentIdent.Reg);
            ArgumentIdentifier.Add(3, ArgumentIdent.RegAddr);
            ArgumentIdentifier.Add(4, ArgumentIdent.IndexReg);
            ArgumentIdentifier.Add(5, ArgumentIdent.IndexImm);

            ArgumentIdentifier.Add(0xf, ArgumentIdent.none);

            Running = true;

            SP = 0x01FFF;
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
            if (Running == false) return;
            uint Instr = FetchInstr();
            Instructions instructions = DecodeInstr(Instr, out ushort AM1, out ushort AM2);
            ExecuteInstr(instructions, AM1, AM2);

            CheakStackPointerOverflowError();
        }

        private void ExecuteInstr(Instructions instructions, ushort AM1, ushort AM2)
        {
            ArgumentIdent argumentIdent1 = ArgumentIdentifier[AM1];
            ArgumentIdent argumentIdent2 = ArgumentIdentifier[AM2];
            switch (instructions)
            {
                case Instructions.NOP: break;
                case Instructions.MOV:
                    MOV(argumentIdent1, argumentIdent2);
                    break;
                case Instructions.PUSH:
                    PUSH(argumentIdent1);
                    break;
                case Instructions.POP:
                    POP(argumentIdent1);
                    break;
                case Instructions.ADD:
                case Instructions.SUB:
                case Instructions.MUL:
                case Instructions.DIV:
                case Instructions.AND:
                case Instructions.OR:
                case Instructions.NOT:
                case Instructions.NOR:
                case Instructions.ROL:
                case Instructions.ROR:
                case Instructions.CMP:
                case Instructions.XOR:
                case Instructions.SHL:
                case Instructions.SHR:
                    ALU(argumentIdent1, argumentIdent2, instructions);
                    break;
                case Instructions.JMP:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), 255, 0);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value, 255, 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JLE:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            JumpOr(FetchInstr(), Flag_Less, 1, Flag_Equal, 1);
                            break;
                        case ArgumentIdent.RegAddr:
                            JumpOr(GetRegValue(DecodeRegister(FetchInstr())).m_value, 
                                Flag_Less, 1, 
                                Flag_Equal, 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JE:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Equal, 1);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Equal, 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JGE:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            JumpOr(FetchInstr(), Flag_Less, 0, Flag_Equal, 1);
                            break;
                        case ArgumentIdent.RegAddr:
                            JumpOr(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Less, 0,
                                Flag_Equal, 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JG:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Less, 0);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Less, 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JNE:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Equal, 0);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Equal, 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JL:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Less, 0);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Less, 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JER:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Error, 1);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Error, 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JMC:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Carry, 1);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Carry, 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JMZ:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Zero, 1);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Zero, 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JNC:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Carry, 0);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Carry, 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.INT:
                    if (argumentIdent1 == ArgumentIdent.Imm)
                    {
                        SystemCall.Call(FetchInstr(), ref AX, ref BX, ref CX, ref DX, ref ZX, ref X, ref Y);
                    }
                    else if (argumentIdent1 == ArgumentIdent.none)
                    {
                    }
                    break;
                case Instructions.CALL:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Push(PC);
                            Jump(FetchInstr(), 255, 0);
                            break;
                        case ArgumentIdent.RegAddr:
                            Push(PC);
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,255, 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.RTS:
                    Pop(out PC);
                    break;
                case Instructions.RET:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Imm:
                            Pop(out PC);
                            PC += FetchInstr();
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.PUSHR:
                    Push(AX);
                    Push(BX);
                    Push(CX);
                    Push(DX);
                    Push(ZX);
                    Push(X);
                    Push(Y);
                    break;
                case Instructions.POPR:
                    Pop(out Y);
                    Pop(out X);
                    Pop(out ZX);
                    Pop(out DX);
                    Pop(out CX);
                    Pop(out BX);
                    Pop(out AX);
                    break;
                case Instructions.INC:
                    INCDEC(argumentIdent1, false);
                    break;
                case Instructions.DEC:
                    INCDEC(argumentIdent1, true);
                    break;
                case Instructions.IN:
                    InPort(argumentIdent1, argumentIdent2);
                    break;
                case Instructions.OUT:
                    OutPort(argumentIdent1, argumentIdent2);
                    break;
                case Instructions.CLF:
                    if(argumentIdent1 == ArgumentIdent.Imm)
                    {
                        Setflag(FetchInstr(), 0);
                    }
                    break;
                case Instructions.SEF:
                    if (argumentIdent1 == ArgumentIdent.Imm)
                    {
                        Setflag(FetchInstr(), 1);
                    }
                    break;
                case Instructions.JMS:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Sing, 1);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Sing, 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.JNS:
                    switch (argumentIdent1)
                    {
                        case ArgumentIdent.Addr:
                            Jump(FetchInstr(), Flag_Sing, 0);
                            break;
                        case ArgumentIdent.RegAddr:
                            Jump(GetRegValue(DecodeRegister(FetchInstr())).m_value,
                                Flag_Sing, 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case Instructions.HALT:
                    Setflag(Flag_Halt, 1);
                    break;
            }
        }
    }
}
