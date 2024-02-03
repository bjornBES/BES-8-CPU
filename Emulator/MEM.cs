using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emu
{
    public class MEM
    {
        public const uint MAX_MEM = 16384 * 64;
        public const int MEM_BANK = 0x3FFF + 1;
        public uint[] mem = new uint[MAX_MEM];

        public List<uint[]> banks = new List<uint[]>();

        public void Reset(uint[] program)
        {
            ResetBanks();
            mem.Initialize();
            banks[0].Initialize();
            Console.WriteLine(program.Length);
            for (int i = 0; i < program.Length; i++)
            {
                Write((uint)i, program[i], 0);
            }
        }
        const ushort Bank_addr_MIN = 0xA000;
        const ushort Bank_addr_MAX = 0xCFFF;
        public uint Read(uint addr, ushort MB)
        {
            if(addr >= Bank_addr_MIN && addr <= Bank_addr_MAX)
            {
                return banks[MB][addr - Bank_addr_MIN];
            }
            else
            {
                return mem[addr];
            }
        }
        public ushort Read(ushort addr, ushort MB)
        {
            if (addr >= Bank_addr_MIN && addr <= Bank_addr_MAX)
            {
                return (ushort)banks[MB][addr - Bank_addr_MIN];
            }
            else
            {
                return (ushort)mem[addr];
            }
        }
        public void Write(uint addr, uint data, ushort MB)
        {
            if (addr >= Bank_addr_MIN && addr <= Bank_addr_MAX)
            {
                banks[MB][addr - Bank_addr_MIN] = data;
            }
            else
            {
                mem[addr] = data;
            }
        }

        void ResetBanks()
        {
            banks.Clear();
            AddBank(2);
        }
        public void AddBank(ushort count = 1)
        {
            uint[] BankArray = new uint[MEM_BANK];
            BankArray.Initialize();
            for (int i = 0; i < count; i++)
            {
                banks.Add(BankArray);
            }
        }
    }
}
