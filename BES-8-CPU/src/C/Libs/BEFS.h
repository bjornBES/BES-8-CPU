#include "ctypes.h"
#include "date.h"
#include "io.h"

struct Entry
{
    char Name[15];
    ushort* StartAddress;
    ushort* FileLength;
    byte Flags;
    struct Date date;
};

struct Disk
{
    char DiskVersion[4];
    char DiskLetter;
    ushort FreePages;
    byte SectorsParPage;
    ushort PageSize;
    struct Entry RootEntrys[30];
};


struct Disk GetDisk(byte disk, struct Disk* ResultDisk)
{
    struct Disk OutDisk = GetDisk(disk, ResultDisk);
    return OutDisk;
}

struct Entry GetEntry(char* EntryName, struct Disk disk)
{
    struct Entry entry;
    for (size_t i = 0; i < sizeof(disk.RootEntrys); i++)
    {
        entry = disk.RootEntrys[i];
        if (entry.Name == EntryName)
        {
            return entry;
        }
    }

}

byte GetDiskIndex(struct Disk disk)
{
    char DiskLetter = disk.DiskLetter;
    for (size_t i = 0; i < 6; i++)
    {
        char Letter = (char)ReadByte(i, 0, 0x005, 1);
        if (Letter == DiskLetter)
        {
            return i;
        }
    }
    return 0xFF;    
}

byte ReadByte(byte disk, ushort page, ushort address, byte sector)
{
    struct Disk Disk; 
    GetDisk(disk, &Disk);
    uint SectorAddress = sector * (Disk.PageSize / Disk.SectorsParPage);
    uint FullAddress = SectorAddress + page * Disk.PageSize + address;
    byte data = (byte)Interrupt(0xE, FullAddress, disk);
    return data;
}

void WriteByte(byte disk, ushort page, ushort address, byte sector, byte data)
{
    struct Disk Disk; 
    GetDisk(disk, &Disk);
    Interrupt(0x10, disk, sector, address, page, data);
}

void ReadSector(struct Disk disk, ushort page, byte sector)
{
    Interrupt(0xF, GetDiskIndex(disk), sector, page);
}
void WriteString(struct Disk disk, ushort page, byte sector, char* data, ushort startaddr)
{
    byte diskIndex = GetDiskIndex(disk);
    size_t DataLength = sizeof(data);
    for (size_t i = 0; i < DataLength; i++)
    {
        ushort address = startaddr + i;
        WriteByte(diskIndex, page, address, sector, (byte)data[i]);
    }
}