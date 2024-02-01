using Microsoft.VisualBasic;
using System;
using System.Collections;

namespace emu
{
    public class CPU
    {
        public ushort AX = 0;
        public ushort BX = 0;
        public ushort CX = 0;
        public ushort DX = 0;
        public ushort ZX = 0;
        public uint PC = 0;
        public uint SP = 0;
        public ushort MB = 0;
        public ushort X = 0;
        public ushort Y = 0;

        public ushort EAL = 0;
        public ushort EAH = 0;
        public ushort EBL = 0;
        public ushort EBH = 0;

        public ushort F = 0;

        public const ushort Flag_Less =         0;
        public const ushort Flag_Equal =        1;
        public const ushort Flag_Zero =         2;
        public const ushort Flag_Carry =        3;
        public const ushort Flag_IntEnable =    4;
        public const ushort Flag_Halt =         5;
        public const ushort Flag_Sign =         6;
        public const ushort Flag_Parity =       7;

        public void RESET(ref MEM mem)
        {
            SP = 0x85FF;
            MB = 0;
            F = 0;
            AX = BX = CX = DX = ZX = 0;
            X = Y = 0;
            LoadRegisterAdd(Register.PC, 0xFFFF, mem);
            Ports.RESETAll(ref mem);
            Setflag(Flag_Equal, 0);
        }

        public void TICK(ref MEM mem)
        {
            Ports.TICKAll(ref mem);

            uint InstructionRegister = mem.Read(PC, MB);

            PC++;

            ExecutionInstructions(InstructionRegister, ref mem);

        }

        public Instructions InstructionAtPC(MEM mem)
        {
            byte[] SplitInstr = Split16(Split32(mem.Read(PC, 0))[1]);

            Instructions instructions = (Instructions)Enum.Parse(typeof(Instructions), SplitInstr[1].ToString());
            return instructions;
        }

        private void ExecutionInstructions(uint Instruction, ref MEM mem)
        {
            //decode

            byte[] SplitInstr = Split16(Split32(Instruction)[0]);
            Instructions instructions = (Instructions)Enum.Parse(typeof(Instructions), SplitInstr[1].ToString());

            byte argumentIdent = Split8(SplitInstr[0])[0];
            //exe
            uint imm;
            uint reg;
            Register register;
            uint Addr;
            switch (instructions)
            {
                case Instructions.MOV:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 0:     // imm
                            uint data = mem.Read(PC, MB);
                            LoadRegisterImm(register, data);
                            break;
                        case 1:     // addr
                            uint addr = mem.Read(PC, MB);
                            LoadRegisterAdd(register, addr, mem);
                            break;
                        case 2:     // reg
                            uint reg1 = mem.Read(PC, MB);
                            Register register1 = DecodeRegister(reg1);
                            LoadRegistergReg(register, register1);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.STR:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 1:     // addr
                            Addr = mem.Read(PC, MB);
                            WriteMEMReg(register, Addr, ref mem);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.LOR:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 1:     // addr
                            uint addr = mem.Read(PC, MB);
                            LoadRegisterAdd(register, addr, mem);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.PUSH:

                    switch (argumentIdent)
                    {
                        case 0:     // imm
                            uint data = mem.Read(PC, MB);
                            PushImm(data, mem);
                            break;
                        case 2:     // reg
                            reg = mem.Read(PC, MB);
                            register = DecodeRegister(reg);
                            PushReg(register, mem);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.POP:
                    switch (argumentIdent)
                    {
                        // addr?
                        case 2:     // reg
                            reg = mem.Read(PC, MB);
                            register = DecodeRegister(reg);
                            POPReg(register, mem);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.ADD:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 0:
                            uint data = mem.Read(PC, MB);
                            AddImms(register, data);
                            break;
                        case 2:
                            uint reg1 = mem.Read(PC, MB);
                            Register register1 = DecodeRegister(reg1);
                            Addregs(register, register1);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.STI:
                    imm = mem.Read(PC, MB);
                    PC++;

                    switch (argumentIdent)
                    {
                        case 1:
                            uint addr = mem.Read(PC, MB);
                            WriteMEMImm(imm, addr, mem);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.SUB:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 0:
                            uint data = mem.Read(PC, MB);
                            SubImms(register, data);
                            break;
                        case 2:
                            uint reg1 = mem.Read(PC, MB);
                            Register register1 = DecodeRegister(reg1);
                            Subregs(register, register1);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.AND:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 0:
                            uint data = mem.Read(PC, MB);
                            AndImms(register, data);
                            break;
                        case 2:
                            uint reg1 = mem.Read(PC, MB);
                            Register register1 = DecodeRegister(reg1);
                            Andregs(register, register1);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.OR:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 0:
                            uint data = mem.Read(PC, MB);
                            OrImms(register, data);
                            break;
                        case 2:
                            uint reg1 = mem.Read(PC, MB);
                            Register register1 = DecodeRegister(reg1);
                            Orregs(register, register1);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.NOR:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 0:
                            uint data = mem.Read(PC, MB);
                            NorImms(register, data);
                            break;
                        case 2:
                            uint reg1 = mem.Read(PC, MB);
                            Register register1 = DecodeRegister(reg1);
                            Norregs(register, register1);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.CMP:
                    reg = mem.Read(PC, MB);
                    PC++;
                    register = DecodeRegister(reg);

                    switch (argumentIdent)
                    {
                        case 0:
                            uint data = mem.Read(PC, MB);
                            CMPimm(register, data);
                            break;
                        case 2:
                            uint reg1 = mem.Read(PC, MB);
                            Register register1 = DecodeRegister(reg1);
                            CMPregs(register, register1);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.JNE:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, Flag_Equal, 0);
                    break;
                case Instructions.INT:
                    if (Getflag(Flag_IntEnable) == 1)
                    {
                        Setflag(Flag_IntEnable, 0);
                        switch (argumentIdent)
                        {
                            case 0:
                                ushort data = (ushort)mem.Read(PC, MB);
                                SystemCall.Call(data, AX, BX, CX, DX, ref mem);
                                break;
                            case 0xF:
                                break;
                        }
                        Setflag(Flag_IntEnable, 1);
                    }
                    break;
                case Instructions.JMP:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, 0xFF, 0);
                    break;
                case Instructions.JME:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, Flag_Equal, 1);
                    break;
                case Instructions.HALT:
                    switch (argumentIdent)
                    {
                        case 0:
                            uint data = mem.Read(PC, MB);
                            PC++;
                            mem.Write(0xFFFE, data, MB);
                            break;
                    }

                    Setflag(Flag_IntEnable, 1);
                    Setflag(Flag_Halt, 1);
                    break;
                case Instructions.CALL:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    PushImm32(PC, mem);
                    PushImm(F, mem);
                    PC = Addr;
                    break;
                case Instructions.RTS:
                    POPReg(Register.PC, mem);
                    break;
                case Instructions.INC:
                    switch (argumentIdent)
                    {
                        case 1:
                            uint addr = mem.Read(PC, MB);
                            Inc_Addr(addr, mem);
                            break;
                        case 2:
                            reg = mem.Read(PC, MB);
                            register = DecodeRegister(reg);
                            Inc_Reg(register);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.DEC:
                    switch (argumentIdent)
                    {
                        case 1:
                            uint addr = mem.Read(PC, MB);
                            Dec_Addr(addr, mem);
                            break;
                        case 2:
                            reg = mem.Read(PC, MB);
                            register = DecodeRegister(reg);
                            Dec_Reg(register);
                            break;
                    }
                    PC++;
                    break;
                case Instructions.OUTB:
                    imm = mem.Read(PC, 0);
                    PC++;
                    switch (argumentIdent)
                    {
                        case 0:
                            uint data = mem.Read(PC, MB);
                            Ports.Write(imm, data);
                            break;
                        case 2:
                            reg = mem.Read(PC, MB);
                            register = DecodeRegister(reg);
                            Ports.Write(imm, GetRegValue(register));
                            break;
                    }
                    PC++;
                    break;
                case Instructions.INB:
                    imm = mem.Read(PC, 0);
                    reg = mem.Read(PC, MB);
                    register = DecodeRegister(reg);
                    LoadRegisterImm(register, Ports.Read(imm));
                    PC++;
                    break;
                case Instructions.NOP:
                    break;
                case Instructions.SEF:
                    imm = mem.Read(PC, MB);
                    PC++;
                    Setflag((ushort)imm, 1);
                    break;
                case Instructions.CLF:
                    imm = mem.Read(PC, MB);
                    PC++;
                    Setflag((ushort)imm, 0);
                    break;
                case Instructions.JMZ:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, Flag_Zero, 1);
                    break;
                case Instructions.JNZ:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, Flag_Zero, 0);
                    break;
                case Instructions.JML:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, Flag_Less, 1);
                    break;
                case Instructions.JMG:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, Flag_Less, 0);
                    break;
                case Instructions.JMC:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, Flag_Carry, 1);
                    break;
                case Instructions.JNC:
                    Addr = mem.Read(PC, MB);
                    PC++;
                    Jump(Addr, Flag_Carry, 0);
                    break;
                default:
                    break;
            }
        }

        private void Inc_Addr(uint addr, MEM mem)
        {
            uint value = mem.Read(addr, MB) + 1;
            mem.Write(addr, value, MB);
        }
        private void Inc_Reg(Register register)
        {
            uint value = GetRegValue(register) + 1;
            LoadRegisterImm(register, value);
        }
        private void Dec_Addr(uint addr, MEM mem)
        {
            uint value = mem.Read(addr, MB) - 1;
            mem.Write(addr, value, MB);
        }
        private void Dec_Reg(Register register)
        {
            uint value = GetRegValue(register) - 1;
            LoadRegisterImm(register, value);
        }

        private void CMPimm(Register register, uint data)
        {
            uint RegValue = GetRegValue(register);

            if(RegValue > data)
            {
                Setflag(Flag_Less, 0);
            }
            else if (RegValue < data)
            {
                Setflag(Flag_Less, 1);
            }

            uint CMPValue = RegValue - data;
            if (CMPValue == 0)
            {
                Setflag(Flag_Equal, 1);
            }
            else
            {
                Setflag(Flag_Equal, 0);
            }

        }

        private void CMPregs(Register register, Register register1)
        {
            uint RegValue = GetRegValue(register);

            if (RegValue > GetRegValue(register1))
            {
                Setflag(Flag_Less, 0);
            }
            else if (RegValue < GetRegValue(register1))
            {
                Setflag(Flag_Less, 1);
            }

            uint CMPValue = RegValue - GetRegValue(register1);
            if (CMPValue == 0)
            {
                Setflag(Flag_Equal, 1);
            }
            else
            {
                Setflag(Flag_Equal, 0);
            }
        }

        private void Andregs(Register register, Register register1)
        {
            LoadReg(register, (GetRegValue(register) & GetRegValue(register1)));
        }

        private void AndImms(Register register, uint data)
        {
            LoadReg(register, (GetRegValue(register) & data));
        }
        private void Norregs(Register register, Register register1)
        {
            LoadReg(register, (~GetRegValue(register) | ~GetRegValue(register1)));
        }

        private void NorImms(Register register, uint data)
        {
            LoadReg(register, (~GetRegValue(register) | ~data));
        }
        private void Orregs(Register register, Register register1)
        {
            LoadReg(register, (GetRegValue(register) | GetRegValue(register1)));
        }

        private void OrImms(Register register, uint data)
        {
            LoadReg(register, (GetRegValue(register) | data));
        }

        private void Subregs(Register register, Register register1)
        {
            LoadReg(register, (GetRegValue(register) - GetRegValue(register1) - Getflag(Flag_Carry)));
        }

        private void SubImms(Register register, uint data)
        {
            LoadReg(register, GetRegValue(register) - data - Getflag(Flag_Carry));
        }

        private void AddImms(Register register, uint data)
        {
            LoadReg(register, GetRegValue(register) + data + Getflag(Flag_Carry));
        }

        private void Addregs(Register register, Register register1)
        {
            LoadReg(register, GetRegValue(register) + GetRegValue(register1) + Getflag(Flag_Carry));
        }

        private void PushReg(Register register, MEM mem)
        {
            mem.Write(SP, GetRegValue(register), MB);
            SP--;
        }

        private void PushImm(uint data, MEM mem)
        {
            mem.Write(SP, data, MB);
            SP--;
        }
        private void PushImm32(uint data, MEM mem)
        {
            mem.Write(SP, GetHigh(data), MB);
            SP--;
            mem.Write(SP, GetLow(data), MB);
            SP--;
        }
        private void POPReg(Register register, MEM mem)
        {
            SP++;
            LoadReg(register, mem.Read(SP, MB));
        }

        private Register DecodeRegister(uint reg)
        {
            return (Register)Enum.Parse(typeof(Register), reg.ToString());
        }

        void LoadRegisterImm(Register register, uint data)
        {
            LoadReg(register, data);
        }
        void LoadRegisterAdd(Register register, uint addr, MEM mem)
        {
            LoadReg(register, mem.Read(addr, MB));
        }
        void LoadRegistergReg(Register register, Register data)
        {
            LoadReg(register, GetRegValue(data));
        }

        void WriteMEMReg(Register register, uint addr, ref MEM mem)
        {
            mem.Write(addr, GetRegValue(register), MB);
        }
        void WriteMEMImm(uint imm, uint addr, MEM mem)
        {
            mem.Write(addr, imm, MB);
        }

        // marcos
        void OutByte(uint Data, uint Port, MEM mem)
        {
            Ports.Write(Port, Data);
            Ports.TICK(Port, ref mem);
        }
        uint InByte(uint Port)
        {
            return Ports.Read(Port);
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
        void LoadReg(Register reg, uint data)
        {
            if (data == 0) Setflag(Flag_Zero, 1);
            else Setflag(Flag_Zero, 0);
            switch (reg)
            {
                case Register.AX:
                    AX = (ushort)data;
                    break;
                case Register.AL:
                    LoadLow(0b10_0000, (byte)data);
                    break;
                case Register.AH:
                    LoadHigh(0b01_0000, (byte)data);
                    break;
                case Register.BX:
                    BX = (ushort)data;
                    break;
                case Register.BL:
                    LoadLow(0b10_0001, (byte)data);
                    break;
                case Register.BH:
                    LoadHigh(0b01_0001, (byte)data);
                    break;
                case Register.CX:
                    CX = (ushort)data;
                    break;
                case Register.CL:
                    LoadLow(0b10_0010, (byte)data);
                    break;
                case Register.CH:
                    LoadHigh(0b01_0010, (byte)data);
                    break;
                case Register.DX:
                    DX = (ushort)data;
                    break;
                case Register.DL:
                    LoadLow(0b10_0011, (byte)data);
                    break;
                case Register.DH:
                    LoadHigh(0b01_0011, (byte)data);
                    break;
                case Register.ZX:
                    ZX = (ushort)data;
                    break;
                case Register.ZL:
                    LoadLow(0b10_0100, (byte)data);
                    break;
                case Register.ZH:
                    LoadLow(0b01_0100, (byte)data);
                    break;
                case Register.PC:
                    PC = data;
                    break;
                case Register.SP:
                    SP = data;
                    break;
                case Register.MB:
                    MB = (ushort)data;
                    break;
                case Register.X:
                    X = (ushort)data;
                    break;
                case Register.XL:
                    LoadLow(0b10_1000, (byte)data);
                    break;
                case Register.XH:
                    LoadHigh(0b01_1000, (byte)data);
                    break;
                case Register.Y:
                    Y = (ushort)data;
                    break;
                case Register.YL:
                    LoadLow(0b10_1001, (byte)data);
                    break;
                case Register.YH:
                    LoadHigh(0b01_1001, (byte)data);
                    break;
                case Register.F:
                    F = (ushort)data;
                    break;
                default:
                    break;
            }
        }
        public byte GetLow(ushort reg)
        {
            return Split16(reg)[0];
        }
        public ushort GetLow(uint reg)
        {
            return Split32(reg)[0];
        }

        public byte GetHigh(ushort reg)
        {
            return Split16(reg)[1];
        }
        public ushort GetHigh(uint reg)
        {
            return Split32(reg)[1];
        }
        uint GetRegValue(Register register)
        {
            switch (register)
            {
                case Register.AX:
                    return AX;
                case Register.AL:
                    return GetLow(AX);
                case Register.AH:
                    return GetHigh(AX);
                case Register.BX:
                    return BX;
                case Register.BL:
                    return GetLow(BX);
                case Register.BH:
                    return GetHigh(BX);
                case Register.CX:
                    return CX;
                case Register.CL:
                    return GetLow(CX);
                case Register.CH:
                    return GetHigh(CX);
                case Register.DX:
                    return DX;
                case Register.DL:
                    return GetLow(DX);
                case Register.DH:
                    return GetHigh(DX);
                case Register.ZX:
                    return ZX;
                case Register.ZL:
                    return GetLow(ZX);
                case Register.ZH:
                    return GetHigh(ZX);
                case Register.PC:
                    return GetLow(PC);
                case Register.SP:
                    return SP;
                case Register.MB:
                    return MB;
                case Register.X:
                    return X;
                case Register.XL:
                    return GetLow(X);
                case Register.XH:
                    return GetHigh(X);
                case Register.Y:
                    return Y;
                case Register.YL:
                    return GetLow(Y);
                case Register.YH:
                    return GetHigh(Y);
                case Register.F:
                    return F;
                default:
                    return 0;
            }
        }
        void LoadLow(uint reg, byte data)
        {
            string Low = Convert.ToString(data, 16).PadLeft(2, '0');
            string High = Convert.ToString(Split16(reg)[1], 16).PadLeft(2, '0');

            ushort value = Convert.ToUInt16(High + Low, 16);
            Register register = (Register)Enum.Parse(typeof(Register), reg.ToString());
            switch (register)
            {
                case Register.AL:
                    AX = value;
                    break;
                case Register.BL:
                    BX = value;
                    break;
                case Register.CL:
                    CX = value;
                    break;
                case Register.DL:
                    DX = value;
                    break;
                case Register.ZL:
                    ZX = value;
                    break;
                case Register.XL:
                    X = value;
                    break;
                case Register.YL:
                    Y = value;
                    break;
                default:
                    Console.WriteLine("BUG REG LOAD LOW " + reg + " " + register + " " + data);
                    break;
            }
        }
        void LoadHigh(uint reg, byte data)
        {
            string High = Convert.ToString(data, 16).PadLeft(2, '0');
            string Low = Convert.ToString(Split16(reg)[0], 16).PadLeft(2, '0');

            ushort value = Convert.ToUInt16(High + Low, 16);
            Register register = (Register)Enum.Parse(typeof(Register), reg.ToString());
            switch (register)
            {
                case Register.AH:
                    AX = value;
                    break;
                case Register.BH:
                    BX = value;
                    break;
                case Register.CH:
                    AX = value;
                    break;
                case Register.DH:
                    BX = value;
                    break;
                case Register.ZH:
                    ZX = value;
                    break;
                case Register.XH:
                    X = value;
                    break;
                case Register.YH:
                    Y = value;
                    break;
                default:
                    Console.WriteLine("BUG REG LOAD HIGH " + reg + " " + register + " " + data);
                    break;
            }
        }
        void Load32BitRegister(Register register, uint value)
        {
            ushort[] _16bitvalue = Split32(value);
            switch (register)
            {
                case Register.EAX:
                    EAL = _16bitvalue[0];
                    EAH = _16bitvalue[1];
                    break;
                case Register.EBX:
                    EBL = _16bitvalue[0];
                    EBH = _16bitvalue[1];
                    break;
                default:
                    break;
            }
        }
        public uint Get32BitRegister(Register register)
        {
            switch (register)
            {
                case Register.EAX:
                    return Get32Bits(EAH, EAL);
                case Register.EBX:
                    return Get32Bits(EBH, EBL);
                default:
                    break;
            }
            return 0;
        }
        uint Get32Bits(ushort High, ushort Low)
        {
            string SHIGH = Convert.ToString(High, 16).PadLeft(4, '0');
            string SLOW = Convert.ToString(Low, 16).PadLeft(4, '0');
            uint Value = Convert.ToUInt32(SHIGH + SLOW);
            return Value;
        }
        ushort[] Split32(uint data)
        {
            string value = Convert.ToString(data, 16).PadLeft(4, '0');

            string LowStr = value.Substring(4, 4);
            string HighStr = value.Substring(0, 4);

            return new ushort[] { Convert.ToByte(LowStr, 16), Convert.ToByte(HighStr, 16) };
        }
        byte[] Split16(uint data)
        {
            string value = Convert.ToString(data, 16).PadLeft(4, '0');

            string LowStr = value[2].ToString() + value[3].ToString();
            string HighStr = value[0].ToString() + value[1].ToString();

            return new byte[] {Convert.ToByte(LowStr, 16), Convert.ToByte(HighStr,16)};
        }

        byte[] Split8(byte data)
        {
            string value = Convert.ToString(data, 16).PadLeft(2, '0');

            string LowStr = value[1].ToString();
            string HighStr = value[0].ToString();

            return new byte[] { Convert.ToByte(LowStr, 16), Convert.ToByte(HighStr, 16) };
        }
        public uint Getflag(uint flag)
        {
            char[] StrFlags = Convert.ToString(F, 2).PadLeft(16, '0').ToCharArray();

            Array.Reverse(StrFlags);

            return Convert.ToUInt16(StrFlags[flag].ToString());
        }
        public void Setflag(ushort flag, ushort value)
        {
            if (value == 1)
            {
                F |= flag;
            }
            else
            {
                F &= (ushort)~flag;
            }
        }
    }
}
