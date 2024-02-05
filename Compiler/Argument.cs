using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Argument
    {
        public string Name;
        public Token type;
        public bool IsPointer;
        public dynamic Value;
    }
}
