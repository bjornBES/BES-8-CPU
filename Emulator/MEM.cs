using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace emulator
{
    public static class MEM
    {
        public const uint MAX_MEM = 16384 * 64; // == 0x100000
        public static List<uint[]> mem = new List<uint[]>();
        public static uint BankCount = 3;

        public static void Reset(uint[] program)
        {
            ResetBanks();
            Console.WriteLine(program.Length);
            for (uint i = 0; i < program.Length; i++)
            {
                Write(0, i, program[i]);
            }
        }
        public static uint[] ReadCount(uint MB, uint StartAddr, uint Count)
        {
            List<uint> Buffer = new List<uint>();
            for (uint i = StartAddr; i < Count + StartAddr; i++)
            {
                Buffer.Add(Read(MB, i));
            }
            return Buffer.ToArray();
        }
        public static uint[] ReadCount(uint MB, uint StartAddr,int Count)
        {
            List<uint> Buffer = new List<uint>();
            for (uint i = StartAddr; i < (uint)Count + StartAddr; i++)
            {
                Buffer.Add(Read(MB, i));
            }
            return Buffer.ToArray();
        }
        public static uint Read(uint MB, uint Addr)
        {
            return mem[(int)MB][Addr];
        }
        public static uint Read(Register MB, uint Addr)
        {
            return mem[(int)MB][Addr];
        }
        public static uint Read(uint MB, Register Addr)
        {
            return mem[(int)MB][(int)Addr];
        }
        public static uint Read(Register MB, Register Addr)
        {
            return mem[(int)MB][(int)Addr];
        }

        public static void WriteCount(uint MB, uint StartAddr, uint[] Data, uint Count)
        {
            for (uint i = StartAddr; i < Count + StartAddr; i++)
            {
                Write(MB, i, Data[i - StartAddr]);
            }
        }
        public static void WriteCount(uint MB, uint StartAddr, uint[] Data, int Count)
        {
            for (uint i = StartAddr; i < (uint)Count + StartAddr; i++)
            {
                Write(MB, i, Data[i - StartAddr]);
            }
        }
        public static void WriteCount(uint MB, uint StartAddr, uint[] Data)
        {
            WriteCount(MB, StartAddr, Data, Data.Length);
        }

        public static void WriteCache(uint[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                Write(2, 0x10000 + i, data[i]);
            }
        }
        public static void WriteCache(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                Write(2, 0x10000 + i, data[i]);
            }
        }

        public static void Write(uint MB, uint Addr, uint Data)
        {
            mem[(int)MB][Addr] = Data;
        }
        public static void Write(Register MB, uint Addr, uint Data)
        {
            mem[(int)MB][Addr] = Data;
        }
        public static void Write(uint MB, Register Addr, uint Data)
        {
            mem[(int)MB][(int)Addr] = Data;
        }
        public static void Write(uint MB, uint Addr, Register Data)
        {
            mem[(int)MB][(int)Addr] = Data.m_value;
        }
        public static void Write(Register MB, Register Addr, uint Data)
        {
            mem[(int)MB][(int)Addr] = Data;
        }
        public static void Write(uint MB, Register Addr, Register Data)
        {
            mem[(int)MB][(int)Addr] = Data.m_value;
        }
        public static void Write(Register MB, Register Addr, Register Data)
        {
            mem[(int)MB][(int)Addr] = Data.m_value;
        }

        static void ResetBanks()
        {
            AddBank(BankCount);
        }
        public static void AddBank(uint count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                uint[] BankArray = new uint[MAX_MEM];
                BankArray.Initialize();
                mem.Add(BankArray);
            }
        }
    }
}
