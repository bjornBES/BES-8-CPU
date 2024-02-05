namespace Compiler
{
    public class Variable
    {
        public string Name;
        public int Size;
        public string Value;
        public uint Addr;

        public bool IsLocal = false;
        public bool IsArgument = false;
        public bool IsPointer = false;
        public bool IsAllocated = false;

        // IsArgument == true
        public string FuncName = "";
    }
}
