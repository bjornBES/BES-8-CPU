#include "BES8IO.h"
#include "date.h"

#define MaxPageSize 0x800
#define MaxSectorSize 0x200

struct Entry
{
    char Name[15];
    u16* StartAddress;
    u16* FileLength;
    u8 Flags;
    struct Date date;
};

struct Disk
{
    char DiskVersion[4];
    char DiskLetter;
    u16 FreePages;
    u8 SectorsParPage;
    u16 PageSize;
    struct Entry RootEntrys[30];
};


struct Disk GetDisk(u8 disk, struct Disk* ResultDisk)
{
    struct Disk OutDisk = GetDisk(disk, ResultDisk);
    return OutDisk;
}

struct Entry GetEntry(char* EntryName, struct Disk disk)
{
    struct Entry entry;
    for (u8 i = 0; i < sizeof(disk.RootEntrys); i++)
    {
        entry = disk.RootEntrys[i];
        if (entry.Name == EntryName)
        {
            return entry;
        }
    }

}

u8 GetDiskIndex(struct Disk disk)
{
    char DiskLetter = disk.DiskLetter;
    for (u8 i = 0; i < 6; i++)
    {
        char Letter = (char)ReadByte(i, 0, 0x005, 1);
        if (Letter == DiskLetter)
        {
            return i;
        }
    }
    return 0xFF;    
}

u8 ReadByte(u8 disk, u16 page, u16 address, u8 sector)
{
    struct Disk Disk; 
    GetDisk(disk, &Disk);
    u24 SectorAddress = sector * (Disk.PageSize / Disk.SectorsParPage);
    u24 FullAddress = SectorAddress + page * Disk.PageSize + address;
    u8 data = (u8)Interrupt(0xE, FullAddress, disk);
    return data;
}

void WriteByte(u8 disk, u16 page, u16 address, u8 sector, u8 data)
{
    struct Disk Disk; 
    GetDisk(disk, &Disk);
    Interrupt(0x10, disk, sector, address, page, data);
}

void ReadSector(struct Disk disk, u16 page, u8 sector)
{
    Interrupt(0xF, GetDiskIndex(disk), sector, page);
}
void WriteString(struct Disk disk, u16 page, u8 sector, char* data, u16 startaddr)
{
    u8 diskIndex = GetDiskIndex(disk);
    u8 DataLength = sizeof(data);
    for (u8 i = 0; i < DataLength; i++)
    {
        u16 address = startaddr + i;
        Writeu8(diskIndex, page, address, sector, (u8)data[i]);
    }
}