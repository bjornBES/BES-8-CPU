namespace emulator
{
    public class SystemCall
    {
        public static void Call(uint imm16, ref Register AX, ref Register BX, ref Register CX, ref Register DX, ref Register ZX, ref Register X, ref Register Y)
        {
            switch (imm16)
            {
                // int #E0h
                // instr Exit
                case 0xE0:
                    CPU.Running = false;
                    MEM.Write(0, 0x0000, AX);
                    Program.CPU.PC = 0x0000;
                    break;

                // int #E1h
                // instr Console Text Output
                case 0xE1:
                    break;

                // int #E6h
                // instr Exit
                case 0xE6:
                    break;

                // int #EAh
                // instr Read
                case 0xEA:
                    // on the disk offset on -10 (0xA)
                    byte Disk = (byte)DX.Get16BitLowByte().m_value;
                    byte Sector = (byte)DX.Get16BitHighByte().m_value;
                    ushort page = (ushort)CX.m_value;
                    ushort address = (ushort)AX.m_value;
                    FileSystem.ReadSectorFromFile(Disk, page, Sector);

                    uint result = MEM.Read(2, address + 0x10000);

                    ZX = result;
                    break;
            }
        }
    }
}