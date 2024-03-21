#include "BES8IO.h"
#include "date.h"

#define PageSize 0x800
#define SectorSize 0x200

u8 ReadByte(const u8, u16, u8, u16);
char* ReadCount(const u8, u16, u8, u16);
u8* ReadSector(const u8, u8, u16, u8*);

struct Entry
{
    char Name[16];
    u16 StartAddress;
    u16 FileLength;
    u8 Flags;
    u16 EntryCount;
    struct Date date;
};

struct Disk
{
    char FSVersion[4];
    char FSLetter;
    u16 FreePage;
    struct Entry entrys[20];
};
u8 ReadByte(const u8 DiskPort, u16 address, u8 sector, u16 page)
{
    OUT(DiskPort, page);
    OUT(DiskPort, sector);
    OUT(DiskPort, address);
    return (u8)IN(DiskPort); 
}

char* ReadCount(const u8 Port, u16 Address, u8 sector, u16 page, const u8 count)
{
    char Entrybuffer[sizeof(u8)];
    for (u8 i = 0; i < count; i++)
    {
        Entrybuffer[i] = (char)ReadByte(Port, Address + i, sector, page);
    }
    return Entrybuffer;
}
u8* ReadSector(const u8 DiskPort, u8 sector, u16 page, u8* buffer)
{
    u24 Address = (page * PageSize) + (sector * SectorSize);
    buffer = (u8*)ReadCount(SectorSize, Address, DiskPort);
    return buffer;
}
struct Date ReadDate(const u8 diskPort, u16 address, u8 sector, u8 page)
{
    u24 data = (u24)ReadCount(diskPort, address, sector, page, 5);
    struct Date result;
    GetDate(data, &data);

}

struct Entry ReadEntry(u8 entryIndex, u8 DiskPort, bool* IsNull)
{
    OUT(DiskPort, 0x0);
    if (entryIndex < 0x3FF)
    {
        OUT(DiskPort, 0x1);
    }
    else if (entryIndex >= 0x400)
    {
        OUT(DiskPort, 0x2);
    }
    struct Entry result;
    u16 DiskAddress = 0x200 + entryIndex * 0x20;
    for (u8 i = 0; i < 16; i++)
    {
        OUT(DiskPort, DiskAddress + i);
        result.Name[i] = (char)IN(DiskPort);
    }
    if (result.Name[0] == '\0')
    {
        IsNull = true;
        return result;
    }

    result.StartAddress = (u16)ReadCount(4, DiskAddress + 0x16, DiskPort);
    
    result.FileLength = (u16)ReadCount(4, DiskAddress + 0x20, DiskPort);

    u8 offset = 24;
    OUT(DiskPort, DiskAddress + offset);
    result.Flags = IN(DiskPort);
    offset += 1;

    result.EntryCount = (u16)ReadCount(2, DiskAddress + 0x21, DiskPort);;

    result.date = ReadDate(DiskAddress, DiskPort);

    IsNull = false;
    return result;
}

struct Disk GetDisk(u8 disk)
{
    struct Disk result;
    u8 DiskPort = disk + 0xA;
        OUT(DiskPort, 0x0);
        OUT(DiskPort, 0x0);
    for (u8 i = 0; i < 4; i++)
    {
        OUT(DiskPort, i);
        result.FSVersion[i] = (char)IN(DiskPort);
    }
    OUT(DiskPort, 4);
    result.FSLetter = (char)IN(DiskPort);

    for (u8 i = 0; i < 20; i++)
    {
        bool IsNull;
        result.entrys[i] = ReadEntry(i, DiskPort, &IsNull);
        if (IsNull)
        {
            break;
        }
        
    }

    return result;
}