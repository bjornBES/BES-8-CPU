using Compiler;

internal class Program
{
    static string InputFile = "\0\0";
    static string OutputFile = "";
    static string TokenFile = "";
    static string OutputSrcFile = "";
    public static string ProgramPath = "";
    public static string OGPorgramPath = "";

    static bool DoClear = false;

    static Interpreter interpreter = new();
    static bool UseDebuger = false;
    public static FileInfo[] Files;
    public static void Main(string[] args)
    {
        Console.CursorVisible = false;

        DecodeArguments(args);
        if (InputFile == "\0\0") Exit(1);
        if (OutputFile == "")
        {
            OutputFile = "./a.asm";
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

        if (File.Exists(Input) == false) CompilerErrors.ErrorCantFindInputFile(InputFile);
        if (File.Exists(OutputFile) == false) File.Create(OutputFile, 100).Close();

        // go in to the assembler

        if (Directory.Exists(ProgramPath))
        {
            DirectoryInfo Programdirectory = new DirectoryInfo(ProgramPath);
            Files = Programdirectory.GetFiles("*.BEc");
            string src;
            src = "newfile " + Input + "\r\n";
            src += File.ReadAllText(Input) + "\r\n";
            for (int i = 0; i < Files.Length; i++)
            {
                if (Files[i].FullName != Input)
                {
                    src += "newfile " + OGPorgramPath + "\\" + Files[i].Name + "\r\n";
                    src += File.ReadAllText(Files[i].FullName) + "\r\n";
                }
            }
            interpreter.Build(src.Split("\r\n"));
        }
        else
        {
            string src = File.ReadAllText(Input);
            src = "newfile " + Input + "\r\n" + src;

            interpreter.Build(src.Split("\r\n"));
        }

        if (UseDebuger)
        {
            File.WriteAllLines(OutputSrcFile, interpreter.Src);
        }


        Console.WriteLine("starter");

        if (DoClear) Console.Clear();

        Console.WriteLine("Writing");

        WriteOutAsm();

    }
    static void WriteOutAsm()
    {
    }
    private static void Exit(int exitCode = 0)
    {
        Environment.Exit(exitCode);
    }

    static void DecodeArguments(string[] args)
    {
        DecodeInstr(args, "-i", ref InputFile);
        DecodeInstr(args, "-o", ref OutputFile);
        DecodeInstr(args, "-s", ref DoClear, true);
        DecodeInstr(args, "-I", ref ProgramPath);
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

    static bool useDebug(int index, string[] args)
    {
        UseDebuger = true;
        OutputSrcFile = "./src.BEc";
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