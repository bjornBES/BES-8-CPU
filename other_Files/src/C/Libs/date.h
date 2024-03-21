#include "ctypes.h"

struct Date
{
    byte Day;
    byte Month;
    byte Year;
    byte unused;
};

struct Date GetDate(uint code, struct Date* d)
{
    struct Date date;
    byte Day = code & 0x0001F;      // 0b0000_0000_0000_0001_1111
    byte month = code & 0x001E0;    // 0b0000_0000_0001_1110_0000
    byte year = code & 0x0FE00;     // 0b0000_1111_1110_0000_0000
    byte unused = code & 0xF0000;   // 0b1111_0000_0000_0000_0000
    date.Day = Day;
    date.Month = month;
    date.Year = year;
    date.unused = unused;
    d->Day = Day;
    d->Month = month;
    d->Year = year;
    d->unused = unused;
    return date;
}
