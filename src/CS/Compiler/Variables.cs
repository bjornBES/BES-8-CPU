namespace Compiler
{
    class Variables
    {
        public required string Name;
        public required int Size;
        public required string Value;
        public bool IsPointer = false;
        public bool IsAllocated = false;
        public uint Addr;
    }
}
