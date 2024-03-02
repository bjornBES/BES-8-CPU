﻿namespace emulator
{
    public enum Instructions
    {
        NOP,
        MOV,
        PUSH,
        POP,
        ADD,
        SUB,
        MUL,
        DIV,
        AND,
        OR,
        NOT,
        NOR,
        ROL,
        ROR,
        JMP,
        CMP,
        JLE,
        JE,
        JGE,
        JG,
        JNE,
        JL,
        JER,
        JMC,
        JMZ,
        JNC,
        INT,
        CALL,
        RTS,
        RET,
        PUSHR,
        POPR,
        INC,
        DEC,
        IN,
        OUT,
        CLF,
        SEF,
        XOR,
        JMS,
        JNS,
        SHL,
        SHR,
        HALT,
    }
    public enum ArgumentIdent
    {
        Imm,
        Addr,
        Reg,
        RegAddr,
        IndexReg,
        IndexImm,
        IndexRegAddrReg,
        IndexRegAddrImm,

        none,
    }
}
