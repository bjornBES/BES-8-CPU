#include "Libs/ctypes.h"
#include "Libs/stdio.h"
#include "Libs/BEFS.h"

struct Disk BootDisk;

int main(byte disk)
{
    GetDisk(disk, &BootDisk);

    

    return 0;
}
