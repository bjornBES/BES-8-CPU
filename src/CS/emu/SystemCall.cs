namespace emu
{
    public class SystemCall
    {
        public static void Call(ushort imm16, ushort AX, ushort BX, ushort CX, ushort DX, ref MEM mem)
        {
            switch (imm16)
            {
                case 0x10:
                    Int10h(AX, BX, CX, DX, ref mem);
                    break;
            }
        }
        static void Int10h(ushort AX, ushort BX, ushort CX, ushort DX, ref MEM mem)
        {
            switch (AX)
            {
                case 1:
                    Ports.Write(1, 0x800F);
                    Ports.TICK(1, ref mem);
                    uint data = Ports.Read(1);
                    if(data == 1)
                    {
                        // do shit
                        Ports.Write(1, 0x8018);
                        Ports.TICK(1, ref mem);
                        Ports.Write(1, BX);
                        Ports.TICK(1, ref mem);
                        Ports.Write(1, 0x8001);
                        Ports.TICK(1, ref mem);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}