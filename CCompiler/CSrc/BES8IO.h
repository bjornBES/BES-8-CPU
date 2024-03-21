#include "BES8Ctypes.h"


u24 IN(u8 Port)
{
    u24* PortAddress = (u24*)Port;
    u24 data = *PortAddress;
    return data;
}

void OUT(u8 Port, u24 data)
{
    u24* PortAddress = (u24*)Port;
    *PortAddress = data; 
}
