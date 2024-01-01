using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emu
{
    public class ConsolePort : IExpansionPort
    {
        ushort[] VRam = new ushort[0x6C00];
        public MEM MEM { get; set; }
        public ushort DataBus { get; set; }
        public ushort AddrBus { get; set; }
        public bool Read { get; set; }
        public ushort MemBank { get; set; }

        const int MaxX = 192;
        const int MaxY = 144;

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
            if (Read)   // read
            {
                //instrs
            }
            else        // write
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

        void ClearScreen()
        {
            StatusRegister = 2;
            WriteOutToMEM(0, 0, 0);
        }

        public void Con(MEM mem)
        {
            MEM = mem;
        }
    }
}
