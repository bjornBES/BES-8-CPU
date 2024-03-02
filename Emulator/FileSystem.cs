using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace emulator;
public class FileSystem
{
    private const int PageSize = 0x800;
    private const int SectorSize = 0x200;
    private const int EntrieSize = 0x20;
    private const int RootDirectoryLoc = 0x200;
    private const int MetadataPageSize = 5;
    private int RootEntriesCount = 0;

    private byte[] disk;
    private int freePagesCount;

    public void ShowInfo()
    {

    }
    public void ShowVersion()
    {

    }
    void UpdateHeaderPage()
    {
        disk[4] = BitConverter.GetBytes(freePagesCount)[1];
        disk[5] = BitConverter.GetBytes(freePagesCount)[0];
    }

    public void CreateFile(string name, int fileSize, string parentDirectory = null)
    {
        CreateEntry(name, fileSize, false, parentDirectory);
    }

    public void CreateDirectory(string name, string parentDirectory = null)
    {
        CreateEntry(name, 0, true, parentDirectory);
    }

    private void CreateEntry(string name, int fileLength, bool isDirectory, string parentDirectory = null)
    {
        if (!string.IsNullOrEmpty(parentDirectory))
        {
            Entry entry = GetEntryByPath(parentDirectory);
            if (!entry.IsDirectory)
            {
                Console.WriteLine("error: " + $"couldn't create {name} in {parentDirectory} becurse it is a file");
                return;
            }
            SetEntryDetails(entry.StartAddress, name, fileLength, isDirectory, entry.FileLength);
            entry.FileLength += EntrieSize;
            UpdateEntry(entry);
        }
        else
        {
            int entryAddress = RootDirectoryLoc + RootEntriesCount * EntrieSize;
            SetEntryDetails(entryAddress, name, fileLength, isDirectory);
            RootEntriesCount++;
        }
        UpdateHeaderPage();
    }

    private void UpdateEntry(Entry entry)
    {
        // con addr 20 + addr
        int EntryAddress = entry.ContinueAddress - 0x20;
        UpdateEntryDetails(EntryAddress, entry.Name, entry.FileLength, entry.IsDirectory, entry.StartAddress);
    }

    private void InitializeFatTable()
    {
        // Initialize FAT table starting at address 0005
        for (int i = 0; i < 511; i++)
        {
            // Set each entry to 0x00, indicating the page is free
            disk[0006 + i] = 0x00;
        }
    }

    private int FindFreePage()
    {
        for (int page = 0; page < disk.Length / PageSize; page++)
        {
            int fatEntryAddress = MetadataPageSize + page;

            // Check if the corresponding FAT entry is 0x00 (indicating the page is free)
            if (disk[fatEntryAddress] == 0x00)
            {
                freePagesCount--;
                disk[fatEntryAddress] = 1;
                return page;
            }
        }
        return -1; // No free page found
    }
    private void SetEntryDetails(int entryAddress, string name, int fileLength, bool isDirectory)
    {
        byte[] Name = Encoding.ASCII.GetBytes(name.PadRight(15, '\0'));
        int OffsetAddr = entryAddress;

        for (int i = 0; i < Name.Length; i++)
        {
            disk[OffsetAddr + i] = Name[i];
        }

        int page = FindFreePage();
        string Address = Convert.ToString(page * PageSize, 16).PadLeft(4, '0');

        disk[OffsetAddr + 0x10] = Encoding.ASCII.GetBytes(Address)[0];
        disk[OffsetAddr + 0x11] = Encoding.ASCII.GetBytes(Address)[1];
        disk[OffsetAddr + 0x12] = Encoding.ASCII.GetBytes(Address)[2];
        disk[OffsetAddr + 0x13] = Encoding.ASCII.GetBytes(Address)[3];


        string Length = Convert.ToString(fileLength, 16).PadLeft(4, '0');

        disk[OffsetAddr + 0x14] = Encoding.ASCII.GetBytes(Length)[0];
        disk[OffsetAddr + 0x15] = Encoding.ASCII.GetBytes(Length)[1];
        disk[OffsetAddr + 0x16] = Encoding.ASCII.GetBytes(Length)[2];
        disk[OffsetAddr + 0x17] = Encoding.ASCII.GetBytes(Length)[3];

        byte ID = (byte)(isDirectory ? 1 : 0);
        byte flags1 = (byte)(ID << 7);
        flags1 |= 0b01000000;
        byte flags2 = 0 << 7;

        disk[OffsetAddr + 0x18] = flags1;
        disk[OffsetAddr + 0x19] = flags2;

        disk[OffsetAddr + 0x1A] = 0x00;
        disk[OffsetAddr + 0x1B] = 0x00;

        Address = Convert.ToString(0x200 + (RootEntriesCount + 1) * 0x20, 16).PadLeft(4, '0');

        disk[OffsetAddr + 0x1C] = Encoding.ASCII.GetBytes(Address)[0];
        disk[OffsetAddr + 0x1D] = Encoding.ASCII.GetBytes(Address)[1];
        disk[OffsetAddr + 0x1E] = Encoding.ASCII.GetBytes(Address)[2];
        disk[OffsetAddr + 0x1F] = Encoding.ASCII.GetBytes(Address)[3];

    }
    private void UpdateEntryDetails(int entryAddress, string name, int fileLength, bool isDirectory, int StartAddress)
    {
        byte[] Name = Encoding.ASCII.GetBytes(name.PadRight(15, '\0'));
        int OffsetAddr = entryAddress;

        for (int i = 0; i < Name.Length; i++)
        {
            disk[OffsetAddr + i] = Name[i];
        }

        string Address = Convert.ToString(StartAddress, 16).PadLeft(4, '0');
        disk[OffsetAddr + 0x10] = Encoding.ASCII.GetBytes(Address)[0];
        disk[OffsetAddr + 0x11] = Encoding.ASCII.GetBytes(Address)[1];
        disk[OffsetAddr + 0x12] = Encoding.ASCII.GetBytes(Address)[2];
        disk[OffsetAddr + 0x13] = Encoding.ASCII.GetBytes(Address)[3];


        string Length = Convert.ToString(fileLength, 16).PadLeft(4, '0');

        disk[OffsetAddr + 0x14] = Encoding.ASCII.GetBytes(Length)[0];
        disk[OffsetAddr + 0x15] = Encoding.ASCII.GetBytes(Length)[1];
        disk[OffsetAddr + 0x16] = Encoding.ASCII.GetBytes(Length)[2];
        disk[OffsetAddr + 0x17] = Encoding.ASCII.GetBytes(Length)[3];

        byte flags1 = (byte)((isDirectory ? 1 : 0) << 7);
        byte flags2 = 0 << 7;

        disk[OffsetAddr + 0x18] = flags1;
        disk[OffsetAddr + 0x19] = flags2;

        disk[OffsetAddr + 0x1A] = 0x00;
        disk[OffsetAddr + 0x1B] = 0x00;

        Address = Convert.ToString(entryAddress + 0x20, 16).PadLeft(4, '0');

        disk[OffsetAddr + 0x1C] = Encoding.ASCII.GetBytes(Address)[0];
        disk[OffsetAddr + 0x1D] = Encoding.ASCII.GetBytes(Address)[1];
        disk[OffsetAddr + 0x1E] = Encoding.ASCII.GetBytes(Address)[2];
        disk[OffsetAddr + 0x1F] = Encoding.ASCII.GetBytes(Address)[3];
    }
    private void SetEntryDetails(int entryAddress, string name, int fileLength, bool isDirectory, int DirectoryEntriesCount)
    {
        byte[] Name = Encoding.ASCII.GetBytes(name.PadRight(15, '\0'));
        int OffsetAddr = entryAddress + DirectoryEntriesCount;

        for (int i = 0; i < Name.Length; i++)
        {
            disk[OffsetAddr + i] = Name[i];
        }
        int page = FindFreePage();
        string Address = Convert.ToString(page * PageSize, 16).PadLeft(4, '0');

        disk[OffsetAddr + 0x10] = Encoding.ASCII.GetBytes(Address)[0];
        disk[OffsetAddr + 0x11] = Encoding.ASCII.GetBytes(Address)[1];
        disk[OffsetAddr + 0x12] = Encoding.ASCII.GetBytes(Address)[2];
        disk[OffsetAddr + 0x13] = Encoding.ASCII.GetBytes(Address)[3];


        string Length = Convert.ToString(fileLength, 16).PadLeft(4, '0');

        disk[OffsetAddr + 0x14] = Encoding.ASCII.GetBytes(Length)[0];
        disk[OffsetAddr + 0x15] = Encoding.ASCII.GetBytes(Length)[1];
        disk[OffsetAddr + 0x16] = Encoding.ASCII.GetBytes(Length)[2];
        disk[OffsetAddr + 0x17] = Encoding.ASCII.GetBytes(Length)[3];

        byte flags1 = (byte)((isDirectory ? 1 : 0) << 7);
        byte flags2 = 0 << 7;

        disk[OffsetAddr + 0x18] = flags1;
        disk[OffsetAddr + 0x19] = flags2;

        disk[OffsetAddr + 0x1A] = 0x00;
        disk[OffsetAddr + 0x1B] = 0x00;

        Address = Convert.ToString(entryAddress + DirectoryEntriesCount + 0x20, 16).PadLeft(4, '0');

        disk[OffsetAddr + 0x1C] = Encoding.ASCII.GetBytes(Address)[0];
        disk[OffsetAddr + 0x1D] = Encoding.ASCII.GetBytes(Address)[1];
        disk[OffsetAddr + 0x1E] = Encoding.ASCII.GetBytes(Address)[2];
        disk[OffsetAddr + 0x1F] = Encoding.ASCII.GetBytes(Address)[3];
    }

    public void SaveToFile(string filePath)
    {
        File.WriteAllBytes(filePath, disk);
        Console.WriteLine($"File system saved to {filePath}.");
    }

    public FileSystem LoadFromFile(string filePath)
    {
        byte[] loadedDisk = File.ReadAllBytes(filePath);
        FileSystem fileSystem = new FileSystem();
        fileSystem.disk = loadedDisk;
        int pageCount = loadedDisk[4] + loadedDisk[5];
        fileSystem.freePagesCount = pageCount;
        return fileSystem;
    }
    public void WriteFile(string path, string Contents)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(Contents);

        Entry entry = GetEntryByPath(path);

        if (bytes.Length > entry.FileLength)
        {
            Array.Resize(ref bytes, entry.FileLength);
        }

        for (int i = entry.StartAddress; i < entry.StartAddress + bytes.Length; i++)
        {
            disk[i] = bytes[i - entry.StartAddress];
        }
    }
    public byte[] ReadSector(uint pageNumber, uint sectorNumber)
    {
        if (pageNumber < 0 || pageNumber >= disk.Length / PageSize)
        {
            Console.WriteLine("Error: Invalid page number.");
            return null;
        }

        uint sectorStart = pageNumber * PageSize + sectorNumber * SectorSize;
        byte[] sectorData = new byte[SectorSize];
        Array.Copy(disk, sectorStart, sectorData, 0, SectorSize);

        return sectorData;
    }
    public Entry GetEntryByPath(string path)
    {
        string[] pathParts = path.Split('/');

        int pathIndex = 0;
        int addr = 0x200;
        Entry ResultEntry = null;
        Entry[] entries = null;
        int i = 0;
        int GotSomeThing = 0;

        while (true)
        {
            if (GetEntrysFromAddress(addr).Length == 0)
            {
                if (entries == null) return null;
                ResultEntry = entries[i];
                break;
            }
            entries = GetEntrysFromAddress(addr);
            if (entries.Length == 0)
            {

            }
            for (i = 0; i < entries.Length; i++)
            {
                if (pathIndex >= pathParts.Length)
                {
                    break;
                }
                if (entries[i].Name == pathParts[pathIndex])
                {
                    addr = entries[i].StartAddress;
                    pathIndex++;
                    GotSomeThing = -1;
                    break;
                }
                else
                {
                    addr += 0x20;
                }
            }
            if (pathIndex >= pathParts.Length || entries[i].Name == pathParts[pathIndex])
            {
                ResultEntry = entries[i - (GotSomeThing + 1)];
                break;
            }
        }
        if (ResultEntry == null) return null;
        return ResultEntry;
    }


    public void ShowEntrys(string path)
    {
        Entry[] entries;
        if (path == "")
        {
            entries = GetEntrysFromAddress(0x200);
        }
        else
        {
            Entry entry = GetEntryByPath(path);
            entries = GetEntrysFromAddress(entry.StartAddress);
        }

        byte NumberOfFiles = 0;
        byte NumberOfDirectory = 0;

        Console.WriteLine($"Directory of {path}/.");
        foreach (Entry entry in entries)
        {
            if (entry.IsDirectory)
            {
                Console.WriteLine($"{entry.Name.PadRight(15, ' ').ToUpper()} <DIR>");
                NumberOfDirectory++;
            }
            else
            {
                Console.WriteLine($"{entry.Name.PadRight(15, ' ').ToUpper()} {entry.FileLength}");
                NumberOfFiles++;
            }
        }
        Console.WriteLine($"{NumberOfFiles.ToString().PadLeft(5, ' ')} Fils(s)");
        Console.WriteLine($"{NumberOfDirectory.ToString().PadLeft(5, ' ')} Dir(s)");
        Console.WriteLine($"{freePagesCount} of pages left");
    }

    private Entry[] GetEntrysFromIndex(int index)
    {
        List<Entry> entries = new List<Entry>();

        Entry BaseEntry = GetEntryByIndex(index);
        int OffsetAddress = 0;
        while (GetEntryByAddress(BaseEntry.StartAddress + OffsetAddress).Name[0] != '\0')
        {
            entries.Add(GetEntryByAddress(BaseEntry.StartAddress + OffsetAddress));
            OffsetAddress += 0x20;
        }

        return entries.ToArray();
    }
    private Entry[] GetEntrysFromAddress(int Address)
    {
        List<Entry> entries = new List<Entry>();

        int OffsetAddress = 0;
        while (GetEntryByAddress(Address + OffsetAddress) != null)
        {
            entries.Add(GetEntryByAddress(Address + OffsetAddress));
            OffsetAddress += 0x20;
        }

        return entries.ToArray();
    }

    private Entry GetEntryInDirectory(Entry directory, string name)
    {
        int continuationAddress = directory.StartAddress;

        // Check entries in the current directory
        while (continuationAddress != 0)
        {
            Entry entry = GetEntryByAddress(continuationAddress);

            // Check if the entry matches the specified name
            if (entry != null && entry.Name == name)
            {
                return entry;
            }

            continuationAddress = entry.ContinueAddress;
        }

        // Entry not found in the current directory
        Console.WriteLine($"Error: Entry '{name}' not found in directory '{directory.Name}'.");
        return null;
    }

    private Entry GetEntryByIndex(int index)
    {
        int entryAddress = 0x200 + index * 0x20;
        return ReadEntry(entryAddress);
    }

    private Entry GetEntryByAddress(int entryAddress)
    {
        return ReadEntry(entryAddress);
    }

    private Entry ReadEntry(int entryAddress)
    {
        byte[] nameBytes = new byte[15];
        Array.Copy(disk, entryAddress, nameBytes, 0, 15);

        string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');

        byte[] buffer = new byte[4];
        Array.Copy(disk, entryAddress + 16, buffer, 0, 4);
        if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0 && buffer[3] == 0) return null;
        int startAddress = Convert.ToInt32(Encoding.ASCII.GetString(buffer), 16);

        Array.Copy(disk, entryAddress + 20, buffer, 0, 4);
        int fileLength = Convert.ToInt32(Encoding.ASCII.GetString(buffer), 16);

        bool isDirectory = (disk[entryAddress + 24] & 0x80) == 0x80;

        Array.Copy(disk, entryAddress + 28, buffer, 0, 4);
        int continueAddress = Convert.ToInt32(Encoding.ASCII.GetString(buffer), 16);

        return new Entry(name, startAddress, fileLength, isDirectory, continueAddress);
    }
    public static void ReadSectorFromFile(uint disk, uint page, uint sector)
    {
        uint Disk = disk - 0xA;
        string filePath;
        switch (Disk)
        {
            case 6:
                filePath = Ports.FloppyDiskPath;
                break;
            case 7:
                filePath = Ports.CartridgePath;
                break;
            default:
                filePath = Ports.DiskFilePath[Disk];
                break;
        }

        byte[] loadedDisk = File.ReadAllBytes(filePath);
        FileSystem fileSystem = new FileSystem();
        fileSystem.disk = loadedDisk;
        int pageCount = loadedDisk[4] + loadedDisk[5];
        fileSystem.freePagesCount = pageCount;

        fileSystem.LoadFromFile(filePath);
        byte[] Sector = fileSystem.ReadSector(page, sector);
        MEM.WriteCache(Sector);
    }
}
