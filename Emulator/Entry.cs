namespace emulator;
public class Entry
{
    public string Name { get; }
    public int StartAddress { get; set; }
    public int FileLength { get; set; }
    public bool IsDirectory { get; }
    public int ContinueAddress { get; }

    public Entry(string name, int startAddress, int fileLength, bool isDirectory, int continueAddress)
    {
        Name = name;
        StartAddress = startAddress;
        FileLength = fileLength;
        IsDirectory = isDirectory;
        ContinueAddress = continueAddress;
    }
}
