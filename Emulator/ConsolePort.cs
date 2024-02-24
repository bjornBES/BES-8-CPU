using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emulator
{
    public class ConsolePort : IExpansionPort
    {
        ushort[] VRam = new ushort[0x6C00];
        public uint DataBus { get; set; }
        public uint AddrBus { get; set; }
        public bool Read { get; set; }
        public ushort MemBank { get; set; }

        const int MaxX = 192;
        const int MaxY = 144;
        public bool Wait = false;
        public ushort DataRegister;
        public ushort CursorPosX;
        public ushort CursorPosY;

        // instruction layout
        // DDDD DDDD CCCC UUUU
        // U = unused
        // D = Data
        // C = Register identifier
        //

        // this will say if the display is in text mode or in video mode
        ushort StatusRegister = 0;              // this is register is 0010
        const ushort StatusRegisterIdentifier = 0b0000_0000_0010_0000;

        public void TICK()
        {
            if (Read == false)
            {
                if (Wait == true)
                {
                    DataRegister = (ushort)DataBus;
                    return;
                }
                if ((DataBus & 0x800F) == 0x800F)
                {
                    // cheak
                    // return 1 else 0
                    DataBus = 0x1;
                }
                else if ((DataBus & 0x8018) == 0x8018)
                {
                    // set data register
                    Wait = true;
                }
                else if ((DataBus & 0x8009) == 0x8009)
                {
                    // Write Buffer

                }
                else if ((DataBus & 0x8001) == 0x8001)
                {
                    // Print DataRegister Char
                    byte[] CharBytes = { (byte)DataRegister };
                    char PrintChar = Encoding.Unicode.GetChars(CharBytes)[0];
                    writeConsole(PrintChar, CursorPosX, CursorPosY, 15);
                }
                else
                {
                    ClearScreen();
                    PrintBuffer();
                }
            }
            else // read
            {

            }
            return;
        }

        void WriteOutToMEM(ushort X, ushort Y, ushort data)
        {
            if(StatusRegister == 1) // text mode
            {

            }
            else if (StatusRegister == 2) // video mode
            {
                if(X > MaxX || Y > MaxY) 
                {
                    return;
                }
                VRam[X * Y] = data;
            }
        }

        public void RESET()
        {
            VRam.Initialize();
            StatusRegister = 0;
            Read = false;
            AddrBus = 0xA000;
            DataBus = 0;
            MemBank = 2;
            ClearScreen();
        }

        void writeConsole(char Char, ushort CursorX, ushort CursorY, ushort TextColor)
        {
            // char = 0x48 (H)
            // color = 0xF
            // 0x0048F
            ushort data = Convert.ToUInt16(Char);
            string StrData = Convert.ToString(data, 16).PadLeft(4, '0') +
                             Convert.ToString(TextColor, 16).PadLeft(1, '0');

            //MEM.Write((uint)(0x32000 + CursorX + CursorY), Convert.ToUInt32(StrData, 16), 1);
            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), TextColor.ToString());
            Console.SetCursorPosition(CursorX, CursorY);
            Console.Write(Char);
        }
        private void ClearScreen()
        {
            Console.Clear();
            CursorPosX = 0;
            CursorPosY = 0;
        }

        private void ClearBuffer()
        {
            for (uint CX = 0; CX < 192 + 1; CX++)
            {
                for (uint CY = 0; CY < 144 + 1; CY++)
                {
                    //MEM.Write(0x32000 + (CX + CY), 0, 1);
                }
            }
        }
        private void PrintBuffer()
        {
            for (uint CX = 0; CX < 192 + 1; CX++)
            {
                for (uint CY = 0; CY < 144 + 1; CY++)
                {
                    //byte[] Bytes = { (byte)MEM.Read(0x32000 + (CX + CY), 1) };
                    //char Char = Encoding.ASCII.GetString(Bytes)[0];
                    //Console.Write(Char);
                }
            }
        }
    }
}
