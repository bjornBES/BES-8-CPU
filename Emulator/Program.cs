using emulator;
using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    public static CPU CPU = new CPU();

    static string InputFile = "\0\0";
    static void Main(string[] args)
    {
        //Console.SetWindowSize(192, 60);
        //Console.SetBufferSize(192, 60);
        //MEM = new MEM();
        //Engine.RESET(new uint[0]);

        //Environment.Exit(0);
        string ConfigFile = Environment.CurrentDirectory + "\\" + args[0];

        DecodeArguments(File.ReadAllText(ConfigFile).Split("\r\n"));
        if (InputFile == "\0\0") { Console.WriteLine("Can't Find the file"); Exit(1); }

        InputFile = InputFile.Replace(".\\", "");
        InputFile = InputFile.Replace("./", "");
        InputFile = InputFile.Replace("/", "\\");

        string InputPath = Environment.CurrentDirectory + "\\" + InputFile;

        if (File.Exists(InputPath) == false) Exit(1);


        List<uint> Instrs = new List<uint>();

        string[] src = File.ReadAllText(InputPath).Split('|');
        for (int i = 0; i < src.Length; i++)
        {
            if (string.IsNullOrEmpty(src[i]) == false)
            {
                Instrs.Add(Convert.ToUInt32(src[i], 16));
            }
        }
        Console.WriteLine(Instrs[0]);

        //Environment.Exit(0);

        Console.WriteLine("Starting the CPU");
        CPU = new CPU();

        MEM.Reset(Instrs.ToArray());
        CPU.RESET();
        Console.WriteLine("CPU is Running");

        do
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (keyInfo.Key == ConsoleKey.L)
            {
                Console.Clear();

                Console.WriteLine("Write an addr\nXXXXh for hex\nXXXXXXXXXXXXXXXXb for bin\nXXXXXX for dec");
                string StrAddr = Console.ReadLine();

                uint addr;

                if (StrAddr.EndsWith('h'))
                {
                    addr = Convert.ToUInt32(StrAddr.TrimEnd('h'), 16);
                }
                else if (StrAddr.EndsWith('b'))
                {
                    addr = Convert.ToUInt32(StrAddr.TrimEnd('b'), 2);
                }
                else
                {
                    addr = uint.Parse(StrAddr);
                }

                Console.WriteLine("Write the bank");
                string Str_Bank = Console.ReadLine();

                uint Bank;

                if (Str_Bank.EndsWith('h'))
                {
                    Bank = Convert.ToUInt32(Str_Bank.TrimEnd('h'), 16);
                }
                else if (Str_Bank.EndsWith('b'))
                {
                    Bank = Convert.ToUInt32(Str_Bank.TrimEnd('b'), 2);
                }
                else
                {
                    Bank = uint.Parse(Str_Bank);
                }

                Console.Clear();
                Console.WriteLine("At " + StrAddr.TrimEnd('h') + " in MEM " + ToHex(MEM.Read(Bank, addr)));
            }
            else if (keyInfo.Key == ConsoleKey.T)
            {
                Console.Clear();

                CPU.TICK();
                WriteInfo();
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                break;
            }
            else if (keyInfo.Key == ConsoleKey.I)
            {
                Console.Clear();
                WriteInfo();
            }


        } while (CPU.Getflag(CPU.Flag_Halt) == 0);
    }
    static void WriteInfo()
    {
        Console.WriteLine("Registers");
        Console.WriteLine($"AX:{ToHex20(CPU.AX)}\tAL:{ToHexByte(CPU.AX.GetLowByte())}\tAH:{ToHexByte(CPU.AX.GetHighByte())}");
        Console.WriteLine($"BX:{ToHex20(CPU.BX)}\tBL:{ToHexByte(CPU.BX.GetLowByte())}\tBH:{ToHexByte(CPU.BX.GetHighByte())}");
        Console.WriteLine($"CX:{ToHex20(CPU.CX)}\tCL:{ToHexByte(CPU.CX.GetLowByte())}\tCH:{ToHexByte(CPU.CX.GetHighByte())}");
        Console.WriteLine($"DX:{ToHex20(CPU.DX)}\tDL:{ToHexByte(CPU.DX.GetLowByte())}\tDH:{ToHexByte(CPU.DX.GetHighByte())}");
        Console.WriteLine($"ZX:{ToHex20(CPU.ZX)}\tZL:{ToHexByte(CPU.ZX.GetLowByte())}\tZH:{ToHexByte(CPU.ZX.GetHighByte())}");
        Console.WriteLine($"PC:{ToHex(CPU.PC)}");
        Console.WriteLine($"X:{ToHex20(CPU.X)} Y:{ToHex20(CPU.Y)} SP:{ToHex20(CPU.SP)} MB:{ToHex20(CPU.MB)}");
        Console.WriteLine($"F:{Convert.ToString(CPU.F.m_value, 2).PadLeft(16, '0')}");
        Console.Write("Error:" + CPU.Getflag(CPU.Flag_Error) + " ");
        Console.Write("Parity:" + CPU.Getflag(CPU.Flag_Parity) + " ");
        Console.Write("Sing:" + CPU.Getflag(CPU.Flag_Sing) + " ");
        Console.Write("Halt:" + CPU.Getflag(CPU.Flag_Halt) + " ");
        Console.Write("Carry:" + CPU.Getflag(CPU.Flag_Carry) + " ");
        Console.Write("Less:" + CPU.Getflag(CPU.Flag_Less) + " ");
        Console.Write("Equal:" + CPU.Getflag(CPU.Flag_Equal) + " ");
        Console.Write("Zero:" + CPU.Getflag(CPU.Flag_Zero) + " ");
        Console.Write("\n");
        Console.Write("\n====MEM====\n");
        Console.Write($"Next MEM[{ToHex(CPU.PC)}] = {ToHex(MEM.Read(0, CPU.PC))}\n");
        Console.Write($"Next Instr {CPU.DecodeInstr(MEM.Read(0, CPU.PC), out ushort am1, out ushort am2)}\nAM1:{CPU.ArgumentIdentifier[am1]}\nAM2:{CPU.ArgumentIdentifier[am2]}");
    }
    static string ToHex(Register value)
    {
        return Convert.ToString(value.m_value, 16).PadLeft(8, '0');
    }
    static string ToHex20(Register value)
    {
        return Convert.ToString(value.m_value, 16).PadLeft(5, '0');
    }
    static string ToHexWord(Register value)
    {
        return Convert.ToString(value.m_value, 16).PadLeft(4, '0');
    }
    static string ToHexByte(Register value)
    {
        return Convert.ToString(value.m_value, 16).PadLeft(2, '0');
    }

    private static void Exit(int exitCode = 0)
    {
        Environment.Exit(exitCode);
    }

    static void DecodeArguments(string[] args)
    {
        DecodeInstr(args, "Input", ref InputFile);

        DecodeInstr(args, "Disk_Pos_1", ref Ports.DiskFilePath[0]);
        DecodeInstr(args, "Disk_Pos_2", ref Ports.DiskFilePath[1]);
        DecodeInstr(args, "Disk_Pos_3", ref Ports.DiskFilePath[2]);
        DecodeInstr(args, "Disk_Pos_4", ref Ports.DiskFilePath[3]);

        DecodeInstr(args, "mem_layout", ref InputFile);
        DecodeInstr(args, "Banks", ref MEM.BankCount);
    }

    static void DecodeInstr(string[] args, string instr, ref string Result)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(instr))
            {
                Result = args[i].Split(" ", 2)[1];
                return;
            }
        }
    }
    static void DecodeInstr(string[] args, string instr, ref bool Result, string TRUE = "true", string FALSE = "false")
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(instr))
            {
                string[] instrs = args[i].Split(" ");
                if (instrs[0] != instr) continue;
                for (int a = 1; a < instrs.Length; a++)
                {
                    if (instrs[a] == TRUE)
                    {
                        Result = true;
                    }
                    else if (instrs[a] == FALSE)
                    {
                        Result = false;
                    }
                    else
                    {
                        Exit(1);
                        Result = false;
                    }
                }
            }
        }
    }
    static void DecodeInstr(string[] args, string instr, ref bool Result, bool TO)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(instr))
            {
                Result = TO;
            }
        }
    }
    static void DecodeInstr(string[] args, string instr, ref int Result)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(instr))
            {
                string[] instrs = args[i].Split(" ");
                for (int a = 1; a < instrs.Length; a++)
                {
                    if (instrs[a].Contains("0x"))
                    {
                        Result = Convert.ToInt32(instrs[a].Remove(0, 2), 16);
                    }
                    else if (instrs[a].Contains("0b"))
                    {
                        Result = Convert.ToInt32(instrs[a].Remove(0, 2), 2);
                    }
                    else
                    {
                        Result = int.Parse(instrs[a].Remove(0, 2));
                    }
                }
            }
        }
    }
    static void DecodeInstr(string[] args, string instr, ref uint Result)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(instr))
            {
                string[] instrs = args[i].Split(" ");
                for (int a = 1; a < instrs.Length; a++)
                {
                    if (instrs[a].Contains("0x"))
                    {
                        Result = Convert.ToUInt32(instrs[a].Remove(0, 2), 16);
                    }
                    else if (instrs[a].Contains("0b"))
                    {
                        Result = Convert.ToUInt32(instrs[a].Remove(0, 2), 2);
                    }
                    else
                    {
                        Result = uint.Parse(instrs[a].Remove(0, 2));
                    }
                }
            }
        }
    }
    static void DecodeInstr(string[] args, string instr, Func<int, string[], bool> func)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(instr))
            {
                string[] instrs = args[i].Split(" ");
                for (int a = 0; a < instrs.Length; a++)
                {
                    func(a, instrs);
                }
            }
        }
    }
}