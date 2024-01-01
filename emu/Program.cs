using emu;

class Program
{
    CPU CPU;
    MEM MEM;

    string InputFile = "\0\0";
    static void Main(string[] args)
    {
        new Program(args);
    }
    Program(string[] args)
    {
        DecodeArguments(args);
        if (InputFile == "\0\0") { Console.WriteLine("Can't Find the file");  Exit(1); }

        InputFile = InputFile.Replace(".\\", "");
        InputFile = InputFile.Replace("./", "");
        InputFile = InputFile.Replace("/", "\\");

        string InputPath = Environment.CurrentDirectory + "\\" + InputFile;

        if (File.Exists(InputPath) == false) Exit(1);

        string[] src = File.ReadAllText(InputPath).Split('|');

        List<ushort> Instrs = new List<ushort>();

        for (int i = 0; i < src.Length; i++)
        {
            if (string.IsNullOrEmpty(src[i]) == false)
            {
                Instrs.Add(Convert.ToUInt16(src[i], 16));
            }
        }

        Console.WriteLine("Starting the CPU");
        Ports.Reset();
        CPU = new CPU();
        MEM = new MEM();

        MEM.Reset(Instrs.ToArray());
        CPU.RESET(ref MEM);
        Console.WriteLine("CPU is Running");

        Ports.RESETAll(ref MEM);

        do
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if(keyInfo.Key == ConsoleKey.L)
            {
                Console.Clear();

                Console.WriteLine("Write an addr\nXXXXh for hex\nXXXXXXXXXXXXXXXXb for bin\nXXXXXX for dec");
                string StrAddr = Console.ReadLine();

                ushort addr;

                if(StrAddr.EndsWith('h'))
                {
                    addr = Convert.ToUInt16(StrAddr.TrimEnd('h'), 16);
                }
                else if (StrAddr.EndsWith('b'))
                {
                    addr = Convert.ToUInt16(StrAddr.TrimEnd('b'), 2);
                }
                else
                {
                    addr = ushort.Parse(StrAddr);
                }

                Console.Clear();
                Console.WriteLine("At " + StrAddr.TrimEnd('h') + " in MEM " + ToHex(MEM.Read(addr, 0)));
            }
            else if(keyInfo.Key == ConsoleKey.T)
            {
                Console.Clear();

                CPU.TICK(ref MEM);
                WriteInfo();
            }
            else if(keyInfo.Key == ConsoleKey.Escape)
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
    void WriteInfo()
    {
        Console.WriteLine("Registers");
        Console.WriteLine("AX:" + ToHex(CPU.AX) + "\t\tAH:" + ToHexByte(CPU.GetHigh(CPU.AX)) + "\t\tAL:" + ToHexByte(CPU.GetLow(CPU.AX)));
        Console.WriteLine("BX:" + ToHex(CPU.BX) + "\t\tBH:" + ToHexByte(CPU.GetHigh(CPU.BX)) + "\t\tBL:" + ToHexByte(CPU.GetLow(CPU.BX)));
        Console.WriteLine("CX:" + ToHex(CPU.CX) + "\t\tCH:" + ToHexByte(CPU.GetHigh(CPU.CX)) + "\t\tCL:" + ToHexByte(CPU.GetLow(CPU.CX)));
        Console.WriteLine("DX:" + ToHex(CPU.DX) + "\t\tDH:" + ToHexByte(CPU.GetHigh(CPU.DX)) + "\t\tDL:" + ToHexByte(CPU.GetLow(CPU.DX)));
        Console.WriteLine("ZX:" + ToHex(CPU.ZX) + "\t\tZH:" + ToHexByte(CPU.GetHigh(CPU.ZX)) + "\t\tZL:" + ToHexByte(CPU.GetLow(CPU.ZX)));
        Console.WriteLine("X:" + ToHex(CPU.X)+ " Y:" + ToHex(CPU.Y) + " SP:" + ToHex(CPU.SP) + " MB:" + ToHex(CPU.MB));
        Console.WriteLine("PC:" + ToHex(CPU.PC) + "\t\tEAX:" + ToHex(CPU.EAX) + "\t\tEBX:" + ToHex(CPU.EBX));
        Console.WriteLine("F:" + Convert.ToString(CPU.F, 2).PadLeft(16, '0'));
        Console.Write("Parity:" + CPU.Getflag(CPU.Flag_Parity) + " ");
        Console.Write("Sign:" + CPU.Getflag(CPU.Flag_Sign) + " ");
        Console.Write("Halt:" + CPU.Getflag(CPU.Flag_Halt) + " ");
        Console.Write("Int:" + CPU.Getflag(CPU.Flag_IntEnable) + " ");
        Console.Write("Carry:" + CPU.Getflag(CPU.Flag_Carry) + " ");
        Console.Write("Zero:" + CPU.Getflag(CPU.Flag_Zero) + " ");
        Console.Write("Equal:" + CPU.Getflag(CPU.Flag_Equal) + " ");
        Console.Write("Less:" + CPU.Getflag(CPU.Flag_Less) + " ");
        Console.Write("\n");
        Console.Write("\n====MEM====\n");
        Console.WriteLine("Instr " + CPU.InstructionAtPC(MEM));
    }
    string ToHex(uint value)
    {
        return Convert.ToString(value, 16).PadLeft(8, '0');
    }
    string ToHex(ushort value)
    {
        return Convert.ToString(value, 16).PadLeft(4, '0');
    }
    string ToHexByte(ushort value)
    {
        return Convert.ToString(value, 16).PadLeft(2, '0');
    }

    private static void Exit(int exitCode = 0)
    {
        Environment.Exit(exitCode);
    }

    void DecodeArguments(string[] args)
    {
        DecodeInstr(args, "-i", ref InputFile);
    }

    void DecodeInstr(string[] args, string instr, ref string Result)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == instr)
            {
                i++;
                Result = args[i];
            }
        }
    }
    void DecodeInstr(string[] args, string instr, Func<bool> func)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == instr)
            {
                i++;
                func();
            }
        }
    }
}