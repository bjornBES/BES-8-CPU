using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Variable
    {
        public string Name;
        public uint Address;
        public uint Size;

        public bool IsLocal;
        public string FuncName = "";

        public bool IsConst;

        public bool IsPublic;

        public bool IsGlobal;

        public bool IsProtected;

        public bool IsPtr = false;
    }
}
