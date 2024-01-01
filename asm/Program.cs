using asm;

public class Program
{
    static string InputFile = "\0\0";
    static string OutputFile = "";
    static string BinFile = "";
    static string TokenFile = "";
    static string OutputSrcFile = "";
    public static string ProgramPath = "";
    public static string OGPorgramPath = "";

    static bool DoClear = false;

    static readonly Assembler assembler = new();
    static bool UseDebuger = false;
    static bool WriteBin = false;
    public static FileInfo[] Files = null;
    public static void Main(string[] args)
    {
        Console.CursorVisible = false;

        DecodeArguments(args);
        if (InputFile == "\0\0") Exit(1);
        if (OutputFile == "")
        {
            OutputFile = "./a.out";
        }
        if (BinFile != "")
        {
            Conv(ref BinFile);
            WriteBin = true;
        }
        if (TokenFile != "")
        {
            Conv(ref TokenFile);
        }
        if (ProgramPath != "")
        {
            Conv(ref ProgramPath);
        }
        if (OutputSrcFile != "")
        {
            Conv(ref OutputSrcFile);
        }
        string Input = InputFile;
        Conv(ref Input);
        InputFile = InputFile.Replace(".\\", "/");
        InputFile = InputFile.Replace("./", "/");
        InputFile = InputFile.Replace("/", "\\");
        Conv(ref OutputFile);

        Console.WriteLine("ProgramPath " + ProgramPath);
        Console.WriteLine("Input File " + Input);

        if (File.Exists(Input) == false) assembler.ErrorCantFindInputFile(InputFile);
        if (File.Exists(OutputFile) == false) File.Create(OutputFile, 100).Close();

        // go in to the assembler

        if (Directory.Exists(ProgramPath))
        {
            DirectoryInfo Programdirectory = new DirectoryInfo(ProgramPath);
            Files = Programdirectory.GetFiles("*.Basm");
            string src;
            src = ".newfile1011 " + Input + "\r\n";
            src += File.ReadAllText(Input) + "\r\n";
            for (int i = 0; i < Files.Length; i++)
            {
                if (Files[i].FullName != Input)
                {
                    src += ".newfile1011 " + OGPorgramPath + "\\" + Files[i].Name + "\r\n";
                    src += File.ReadAllText(Files[i].FullName) + "\r\n";
                }
            }
            assembler.Build(src.Split("\r\n"));
        }
        else
        {
            string src = File.ReadAllText(Input);
            src = ".newfile1011 " + Input + "\r\n" + src;

            assembler.Build(src.Split("\r\n"));
        }

        if (UseDebuger)
        {
            File.WriteAllLines(TokenFile, assembler.Tokens.ToArray());
            File.WriteAllLines(OutputSrcFile, assembler.OrgSrc);
        }


        Console.WriteLine("starter");

        if (DoClear) Console.Clear();

        Console.WriteLine("Writing");
        Thread BinThread = new Thread(new ThreadStart(WriteOutBin));
        if (WriteBin == true)
        {
            BinThread.Start();
        }

        int SaveProgressBar = 0;
        string outputString = "";

        // main
        for (int i = 0; i < assembler.MCcode.Length; i++)
        {
            if (string.IsNullOrEmpty(assembler.MCcode[i]) == false)
            {
                //Console.WriteLine("data " + assembler.MCcode[i]);
                string Value = assembler.MCcode[i].Trim().ToUpper();
                outputString += Value.PadLeft(4, '0') + "|";
                // info

                int percentComplete = (int)(0.5f + ((100f * i) / 0x1FFFF));
                if (DoClear == false)
                {
                    int ProgressBar = (int)(0.5f + (((Console.WindowWidth - 1) * i) / 0x1FFFF));
                    for (int p = SaveProgressBar; p < ProgressBar; p++)
                    {
                        Console.SetCursorPosition(p, Console.WindowHeight - 1);
                        Console.Write("=");
                        Console.SetCursorPosition(p, Console.WindowHeight - 2);
                        Console.Write("-");
                    }
                    SaveProgressBar = ProgressBar;
                }
                Console.SetCursorPosition((Console.WindowWidth - 18 / 2) / 2, Console.WindowHeight - 2);
                Console.Write(Convert.ToString(i, 16).PadLeft(5, '0') + " / 1FFFF");
                Console.SetCursorPosition((Console.WindowWidth - 12 / 2) / 2, Console.WindowHeight - 1);
                Console.Write(percentComplete.ToString().PadLeft(3, '0') + "% / 100%");
            }
        }
        File.WriteAllText(OutputFile, outputString);

    }
    static void WriteOutBin()
    {
        string outputString = "";


        for (int i = 0; i < assembler.MCcode.Length; i++)
        {
            if (string.IsNullOrEmpty(assembler.MCcode[i]) == false)
            {
                if (i >= 0x10000 && i <= 0x11FFF) continue;
                if (i >= 0x14000 && i <= 0x16FFF) continue;
                //Console.WriteLine("data " + assembler.MCcode[i]);
                string Value = assembler.MCcode[i].Trim().ToUpper();
                if (i % 16 == 0)
                {
                    if (i == 0)
                    {
                        outputString += "00000: ";
                    }
                    else
                    {
                        outputString += "\n" + Convert.ToString(i, 16).PadLeft(5, '0').ToUpper() + ": ";
                    }
                }
                outputString += Value.PadLeft(4, '0') + " ";
            }
        }
        if (File.Exists(BinFile))
        {
            File.WriteAllText(BinFile, outputString);
        }
        else
        {
            Console.WriteLine("Can't find the bin file");
            Exit();
        }
    }
    private static void Exit(int exitCode = 0)
    {
        Environment.Exit(exitCode);
    }

    static void DecodeArguments(string[] args)
    {
        DecodeInstr(args, "-i", ref InputFile);
        DecodeInstr(args, "-o", ref OutputFile);
        DecodeInstr(args, "-v", ref assembler.VarPC);
        DecodeInstr(args, "-s", ref DoClear, true);
        DecodeInstr(args, "-vh", SetvarHex);
        DecodeInstr(args, "-I", ref ProgramPath);
        DecodeInstr(args, "-B", useBinFiles);
        DecodeInstr(args, "-D", useDebug);
    }

    static void DecodeInstr(string[] args, string instr, ref string Result)
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
    static void DecodeInstr(string[] args, string instr, ref bool Result, string TRUE = "true", string FALSE = "false")
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == instr)
            {
                i++;
                if (args[i] == TRUE)
                {
                    Result = true;
                }
                else if (args[i] == FALSE)
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
    static void DecodeInstr(string[] args, string instr, ref bool Result, bool TO)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == instr)
            {
                Result = TO;
            }
        }
    }
    static void DecodeInstr(string[] args, string instr, ref int Result)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == instr)
            {
                i++;
                Result = int.Parse(args[i]);
            }
        }
    }
    static void DecodeInstr(string[] args, string instr, ref uint Result)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == instr)
            {
                i++;
                Result = ushort.Parse(args[i]);
            }
        }
    }
    static void DecodeInstr(string[] args, string instr, Func<int, string[], bool> func)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == instr)
            {
                i++;
                func(i, args);
            }
        }
    }

    static bool useBinFiles(int index, string[] args)
    {
        WriteBin = true;
        BinFile = args[index];

        return false;
    }
    static bool useDebug(int index, string[] args)
    {
        UseDebuger = true;
        OutputSrcFile = "./src.txt";
        TokenFile = "./Tokens.txt";
        return false;
    }
    static bool SetvarHex(int index, string[] args)
    {
        string Value = args[index];
        ushort Dec = Convert.ToUInt16(Value, 16);

        assembler.VarPC = Dec;

        return false;
    }

    static void Conv(ref string file)
    {
        file = file.Replace(".\\", Environment.CurrentDirectory + "\\");
        file = file.Replace("./", Environment.CurrentDirectory + "\\");
        file = file.Replace("/", "\\");
        return;
    }
}