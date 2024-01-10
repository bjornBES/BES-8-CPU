using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Compiler
{
    public enum BitSize
    {
        _U8Bit = 1,
        _8Bit = 1,
        _16Bit = 1,
        _U16Bit = 1,
        _32Bit = 2,
    }

    public class Interpreter
    {
        /// <summary>
        /// this space is for Variables
        /// </summary>
        public uint FreeAddrSpace = 0;

        Literals Word = new()
        {
            BitSize = BitSize._U16Bit,
            Name = "Word",
        };
        Literals Byte = new()
        {
            BitSize = BitSize._8Bit,
            Name = "Byte",
        };
        Literals Char = new()
        {
            BitSize = BitSize._U8Bit,
            Name = "Char",
        };
        Literals Long = new()
        {
            BitSize = BitSize._32Bit,
            Name = "Long",
        };
        Literals Ptr = new()
        {
            BitSize = BitSize._32Bit,
            Name = "Ptr",
        };

        List<Variables> variables = new();


        public const string Comment = "//";

        public List<string> Src;
        public void Build(string[] src)
        {
            Src = src.ToList();

            RemoveAllWhiteSpaces();
            RemoveAllComment();


        }

        void RemoveAllComment()
        {
            for (int i = 0; i < Src.Count; i++)
            {
                if (Src[i].StartsWith(Comment))
                {
                    Src.RemoveAt(i);
                }
                if (Src[i].Contains(Comment))
                {
                    int CommentStart = Src[i].IndexOf(Comment);
                    int Leng = Src[i].Length - CommentStart;
                    Src[i] = Src[i].Remove(CommentStart, Leng);
                }
                for (int a = 0; a < 10; a++)
                {
                    Src[i] = Src[i].TrimEnd();
                }
            }
        }

        void RemoveAllWhiteSpaces()
        {
            for (int i = 0; i < Src.Count; i++)
            {
                for (int a = 0; a < 10; a++)
                {
                    Src[i] = Src[i].TrimStart();
                }
            }
        }

        Variables NewVariables(Literals literals, string Name, uint Addr = 0x1FFFF, uint Value = 0x1FFFF)
        {
            return new Variables() { Name = Name, Type = literals, Addr = Addr, Value = Value };
        }
        public void Allocate()
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Addr == 0x1FFFF)
                {

                }
            }
        }
    }
    struct Literals
    {
        public required BitSize BitSize;
        public required string Name;
        public int GetOffset()
        {
            return (int)BitSize;
        }
    }
    struct Variables
    {
        public required Literals Type;
        public required string Name;
        public uint Addr;
        public uint Value;
    }
}
