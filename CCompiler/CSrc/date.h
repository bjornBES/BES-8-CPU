#include "BES8Ctypes.h"

struct Date
{
    u8 Day;
    u8 Month;
    u8 Year;
    u8 unused;
};

struct Date GetDate(u24 code, struct Date* d)
{
    struct Date date;
    u8 Day = code & 0x0001F;      // 0b0000_0000_0000_0001_1111
    u8 month = code & 0x001E0;    // 0b0000_0000_0001_1110_0000
    u8 year = code & 0x0FE00;     // 0b0000_1111_1110_0000_0000
    u8 unused = code & 0xF0000;   // 0b1111_0000_0000_0000_0000
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
