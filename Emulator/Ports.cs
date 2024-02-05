using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emu
{
    public class Ports
    {
        public static IPort[] UserPorts = new IPort[4];
        public static IExpansionPort[] ExpansionPort = new IExpansionPort[10];
        public static IExpansionPort ConsolePort;
        public static Screen Screen;
        public static void Reset()
        {
            ConsolePort = new ConsolePort();
        }
        public static void Add(IPort port, int portAddr)
        {
            UserPorts[portAddr] = port;
        }
        public static void Add(IExpansionPort port, int portAddr, MEM mem)
        {
            ExpansionPort[portAddr] = port;
            ExpansionPort[portAddr].Con(mem);
        }
        public static uint Read(uint port)
        {
            Console.WriteLine("Ports " + port);
            if(port == 1)                           // port 1           Screen
            {
                ConsolePort.Read = true;
                return ConsolePort.DataBus;
            }
            if (port > 1 && port < 6)               // port 2 - 5 4     User Ports
            {
                if (UserPorts[port] != null)
                {
                    return UserPorts[port].OutputBus;
                }
            }
            else if (port >= 6 && port < 0x010)     // port 6 - 15 6    Expansion Ports
            {
                if (ExpansionPort[port - 4] != null)
                {
                    ExpansionPort[port - 4].Read = true;
                    return ExpansionPort[port - 4].DataBus;
                }
            }
            else if (port == 0x010)                 // 16               Disk Header
            {

            }
            else if (port > 0x010 && port < 0x020)  // 17 - 31 14       Disks
            {

            }
            return 0;
        }
        public static void TICK(uint port, ref MEM mem)
        {
            if (UserPorts[port] != null)
            {
                UserPorts[port].TICK(ref mem);
            }
        }
        public static void Write(uint port, uint data)
        {
            if (port < 4)
            {
                if (UserPorts[port] != null)
                {
                    UserPorts[port].InputBus = data;
                }
            }
            else if (port >= 4)
            {
                if (ExpansionPort[port - 4] != null)
                {
                    ExpansionPort[port - 4].Read = false;
                    ExpansionPort[port - 4].DataBus = data;
                }
            }
        }
        public static void TICKAll(ref MEM mem)
        {
            for (int i = 0; i < UserPorts.Length; i++)
            {
                if (UserPorts[i] != null)
                {
                    UserPorts[i].TICK(ref mem);
                }
            }
            for (int i = 0; i < ExpansionPort.Length; i++)
            {
                if (ExpansionPort[i] != null)
                {
                    ExpansionPort[i].TICK();
                    if (ExpansionPort[i].Read == false)
                    {
                        mem.Write(ExpansionPort[i].AddrBus, ExpansionPort[i].DataBus, ExpansionPort[i].MemBank);
                    }
                    else
                    {
                        ExpansionPort[i].DataBus = mem.Read(ExpansionPort[i].AddrBus, ExpansionPort[i].MemBank);
                    }
                }    
            }
        }
        public static void RESETAll(ref MEM mem)
        {
            for (int i = 0; i < UserPorts.Length; i++)
            {
                if (UserPorts[i] != null)
                {
                    UserPorts[i].RESET(ref mem);
                }
            }
            for (int i = 0; i < ExpansionPort.Length; i++)
            {
                if (ExpansionPort[i] != null)
                {
                    ExpansionPort[i].RESET();
                }
            }
        }
        public static uint GetStatusPort(MEM mem)
        {
            if (UserPorts[0] != null)
            {
                uint Buf = UserPorts[0].InputBus;
                UserPorts[0].TICK(ref mem);

                UserPorts[0].InputBus = Buf;

                return UserPorts[0].OutputBus;
            }
            return 0xF0;
        }
    }

    public interface IPort
    {
        public uint OutputBus { get; set; }
        public uint InputBus { get; set; }

        public void TICK(ref MEM mem);
        public void RESET(ref MEM mem);
    }
    public interface IExpansionPort
    {
        public MEM MEM { get; set; }
        public uint DataBus { get; set; }
        public uint AddrBus { get; set; }
        public bool Read { get; set; }
        public ushort MemBank { get; set; }

        public void Con(MEM mem);
        public void TICK();
        public void RESET();
    }
}
