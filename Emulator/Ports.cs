using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emu
{
    public class Ports
    {
        public static string FloppyDiskPath = "";
        public static string[] DiskFilePath = new string[4];
        public static string CartridgePath = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disk">write a number beteen 0x11 and 0x20</param>
        /// <param name="mem"></param>
        public void ReadFile(uint disk)
        {
            uint Disk = disk - 0x11;
            string filePath = "";
            switch (Disk)
            {
                case 4:
                    filePath = FloppyDiskPath;
                    break;
                case 5:
                    filePath = CartridgePath;
                    break;
                default:
                    filePath = DiskFilePath[Disk];
                    break;
            }


        }
    }
    public interface IExpansionPort
    {
        public uint DataBus { get; set; }
        public uint AddrBus { get; set; }
        public bool Read { get; set; }
        public ushort MemBank { get; set; }

        public void TICK();
        public void RESET();
    }
}
