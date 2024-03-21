#include "ctypes.h"
#include "io.h"

const void* NULL = ((void*)0);

void PutC(char c)
{
    OutPort(0x5, (uint)c);
}

void PutS(char* c)
{
    for (size_t i = 0; i < sizeof(c); i++)
    {
        OutPort(0x5, (uint)c[i]);
    }
}

char GetC(char* c)
{
    return InPort(0x5, c);
}