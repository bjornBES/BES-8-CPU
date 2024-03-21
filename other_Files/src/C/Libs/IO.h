#include "ctypes.h"

uint In(uint address, uint* Data);
void Out(uint address, uint data);
uint InPort(byte port, uint* data);
void OutPort(byte port, uint data);
void* Interrupt(byte InterruptInstruction, ...);
struct Disk GetDisk(byte disk);