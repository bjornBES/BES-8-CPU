using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emulator
{
    public static class Ports
    {
        static FileSystem fileSystem = new FileSystem();

        static List<IExpansionPort> ExpansionPorts = new List<IExpansionPort>(4);
        static List<IUserPorts> UserPorts = new List<IUserPorts>(4);

        public static void ConnectPort(IExpansionPort expansionPort)
        {
            if (ExpansionPorts.Count != ExpansionPorts.Capacity)
            {
                ExpansionPorts.Add(expansionPort);
            }
            else
            {
                Console.WriteLine("There can't be any more Expansion Ports");
            }
        }
        public static void ConnectPort(IUserPorts userPorts)
        {
            if (UserPorts.Count != UserPorts.Capacity)
            {
                UserPorts.Add(userPorts);
            }
            else
            {
                Console.WriteLine("There can't be any more User Ports");
            }
        }

        public static string FloppyDiskPath = "";
        public static string[] DiskFilePath = new string[4];
        public static string CartridgePath = "";

        public static void Out(int Port, uint source)
        {
            if (Port <= 4)
            {
                IExpansionPort expansionPort = ExpansionPorts[Port];
                if (expansionPort.Read == false)
                {
                    if (expansionPort.AddrBus != 0 && expansionPort.MemBank != 0)
                    {
                        MEM.Write(expansionPort.MemBank, expansionPort.AddrBus, expansionPort.DataBus);
                    }
                    else
                    {
                        expansionPort.DataBus = source;
                    }
                }
            }

            if (Port >= 0x6 && Port <= 0x9)
            {
                IUserPorts userPorts = UserPorts[Port];
            }
            if (Port >= 0xA && Port <= 0xF)
            {
            }
        }
        public static Register In(int Port, out Register destination)
        {
            if (Port <= 4)
            {
                IExpansionPort expansionPort = ExpansionPorts[Port];
                if (expansionPort.Read == true)
                {
                    if (expansionPort.AddrBus != 0 && expansionPort.MemBank != 0)
                    {
                        uint MemoryData = MEM.Read(expansionPort.MemBank, expansionPort.AddrBus);

                        destination = MemoryData;
                        return MemoryData;
                    }
                    else
                    {
                        destination = expansionPort.DataBus;
                        return expansionPort.DataBus;
                    }
                }
            }

            if (Port >= 0x6 && Port <= 0x9)
            {
                IUserPorts userPorts = UserPorts[Port];
                destination = userPorts.DataBus;
                return userPorts.DataBus;
            }
            destination = new Register();
            return 0;
        }
    }
    public interface IExpansionPort
    {
        /// <summary>
        /// The <![CDATA[Databus]]> is the way to communicate to and from the CPU and other divices
        /// </summary>
        public uint DataBus { get; set; }

        public uint AddrBus { get; set; }
        public ushort MemBank { get; set; }

        /// <summary>
        /// The <![CDATA[Read]]> will say if the CPU is trying to read or write to the port
        /// </summary>
        public bool Read { get; set; }

        public void TICK();
        public void RESET();
    }

    /// <summary>
    /// the User ports will talk to the CPU using SPI
    /// </summary>
    public interface IUserPorts
    {
        /// <summary>
        /// 
        /// </summary>
        public uint DataBus { get; set; }
    }
}
