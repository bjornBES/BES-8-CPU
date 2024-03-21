using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public enum Registers
    {
        AX =    0b0000_0000,
        AL =    0b0010_0000,
        AH =    0b0001_0000,
        BX =    0b0000_0001,
        BL =    0b0010_0001,
        BH =    0b0001_0001,
        CX =    0b0000_0010,
        CL =    0b0010_0010,
        CH =    0b0001_0010,
        DX =    0b0000_0011,
        DL =    0b0010_0011,
        DH =    0b0001_0011,
        ZX =    0b0000_0100,
        ZL =    0b0010_0100,
        ZH =    0b0001_0100,
        PC =    0b0000_0101,
        SP =    0b0000_0110,
        MB =    0b0000_0111,
        X =     0b0000_1000,
        XL =    0b0010_1000,
        XH =    0b0001_1000,
        Y =     0b0000_1001,
        YL =    0b0010_1001,
        YH =    0b0001_1001,
        XY =    0b0000_1010,
        AB =    0b0001_1010,
        CD =    0b0010_1010,
        R1 =    0b0000_0101,
        R2 =    0b0001_0101,
        R3 =    0b0010_0101,
        R4 =    0b0011_0101,
        PMB =   0b0000_1100,
        F =     0b0000_1111,
    }
}
