namespace emulator
{
    public class SystemCall
    {
        public static void Call(uint imm16, Register AX, Register BX, Register CX, Register DX, Register ZX, Register X, Register Y)
        {
            switch (imm16)
            {
                    // int #00h
                    // instr Exit
                case 0x00:  
                    CPU.Running = false;
                    MEM.Write(0, 0x0000, AX);
                    Program.CPU.PC = 0x0000;
                    break;
            }
        }
    }
}