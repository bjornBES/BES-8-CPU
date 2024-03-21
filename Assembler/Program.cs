using assembler;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Text;

public class Program
{
    static string InputFile = "\0\0";
    static string OutputFile = "";
    static string BinFile = "";
    static string TokenFile = "";
    static string ParserTokenFile = "";
    static string OutputSrcFile = "";
    public static string FormatOutputFile = "";
    public static string ProgramPath = "";
    public const string CurrentDirectory = "C:\\Users\\bjorn\\Desktop\\BES-8-CPU\\CPUs\\BES-8-CPU";
    public const string LibsPath = $"{CurrentDirectory}\\Libs";

    static bool Build = true;
    static bool DoClear = false;

    static bool UseDebuger = false;
    public static List<FileInfo> Files = new List<FileInfo>();

    public static int Max_Length = 0xFFFF + 1;
    public const int Pading_Length = 4;

    public static void Main(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine(i + " " + args[i]);
        }
        if (args.Length > 2)
        {
            if (args[1] == "-D")
            {
                if (args.Length == 4)
                {
                    DecodeArguments((
                        "UseDebuger\r\n" +
                        "ObjFile ./Builds/DebugASM/a.obj\r\n" +
                        "UseBinary ./Builds/DebugASM/bin.bin\r\n" +
                        "Output ./Builds/DebugASM/a.out\r\n" +
                        "UseSections false\r\n" +
                        "DontBuild false\r\n" +
                        "SrcFile ./Builds/DebugASM/src.out\r\n" +
                        "FillTo 0x05000").Split("\r\n"));
                }
                else if (args.Length == 3)
                {
                    DecodeArguments((
                        $"Input ./src/asm/{args[2]}\r\n" +
                        "UseDebuger\r\n" +
                        "ObjFile ./Builds/DebugASM/a.obj\r\n" +
                        "UseBinary ./Builds/DebugASM/bin.bin\r\n" +
                        "Output ./Builds/DebugASM/a.out\r\n" +
                        "UseSections false\r\n" +
                        "DontBuild false\r\n" +
                        "SrcFile ./Builds/DebugASM/src.out\r\n" +
                        "FillTo 0x05000").Split("\r\n"));
                }
            }
        }
        else if (args.Length == 2)
        {
            DecodeArguments((
                "UseDebuger\r\n" +
                "ObjFile ./Builds/DebugASM/a.obj\r\n" +
                "UseBinary ./Builds/DebugASM/bin.bin\r\n" +
                "Output ./Builds/DebugASM/a.out\r\n" +
                "UseSections false\r\n" +
                "DontBuild false\r\n" +
                "SrcFile ./Builds/DebugASM/src.out\r\n" +
                "FillTo 0x05000").Split("\r\n"));
            string SettingsFile = args[0];
            Conv(ref SettingsFile);
            DecodeArguments(File.ReadAllText(SettingsFile).Split("\r\n"));
        }
        else
        {
            string SettingsFile = args[0];
            Console.WriteLine("Before " + SettingsFile + " " + args[0]);
            Conv(ref SettingsFile);
            Console.WriteLine("After " + SettingsFile);
            DecodeArguments(File.ReadAllText(SettingsFile).Split("\r\n"));
        }
        Console.CursorVisible = false;

        if (InputFile == "\0\0") AssemblerErrors.ErrorCantFindInputFile("");
        if (FormatOutputFile == "") FormatOutputFile = "Bin";
        Formats formats = (Formats)Enum.Parse(typeof(Formats), FormatOutputFile);
        if (OutputFile == "")
        {
            switch (formats)
            {
                case Formats.Bin:
                    OutputFile = "./a.bin";
                    break;
                case Formats.Obj:
                    OutputFile = "./a.obj";
                    break;
                default:
                    break;
            }
        }
        if (BinFile != "")
        {
            Conv(ref BinFile);
        }
        if (TokenFile != "")
        {
            Conv(ref TokenFile);
        }
        if (ProgramPath != "")
        {
            Conv(ref ProgramPath);
        }
        else
        {
            ProgramPath = InputFile;
        }
        if (OutputSrcFile != "")
        {
            Conv(ref OutputSrcFile);
        }
        if (ParserTokenFile != "")
        {
            Conv(ref ParserTokenFile);
        }

        string Input = InputFile;
        Conv(ref Input);
        InputFile = InputFile.Replace(".\\", "/");
        InputFile = InputFile.Replace("./", "/");
        InputFile = InputFile.Replace("/", "\\");
        Conv(ref OutputFile);

        Console.WriteLine("ProgramPath " + ProgramPath);
        Console.WriteLine("Input File " + Input);
        string OutputPath = GetPathFromFile(OutputFile);
        if (File.Exists(Input) == false) AssemblerErrors.ErrorCantFindInputFile(InputFile);
        if (Directory.Exists(OutputPath) == false) Directory.CreateDirectory(OutputPath);
        if (File.Exists(OutputFile) == false) File.Create(OutputFile);
        string src;
        // go in to the assembler
        if (Directory.Exists(ProgramPath))
        {
            DirectoryInfo Programdirectory = new DirectoryInfo(ProgramPath);
            Files = Programdirectory.GetFiles("*.Basm").ToList();
            List<FileInfo> FilesCopy = Files;
            src = ".newfile " + Input + Environment.NewLine;
            src += File.ReadAllText(Input) + Environment.NewLine;
            Includes(ref src, true, FilesCopy);

            for (int f = 0; f < FilesCopy.Count; f++)
            {
                if (FilesCopy[f].FullName.Replace("\\", "/") != Input)
                {
                    src += ".newfile " + ProgramPath + "/" + FilesCopy[f].Name + Environment.NewLine;
                    src += File.ReadAllText(FilesCopy[f].FullName) + Environment.NewLine;
                }
            }
        }
        else
        {
            src = File.ReadAllText(Input);
            src = ".newfile " + Input + Environment.NewLine + src;

            src = src.ReplaceLineEndings(Environment.NewLine);
            Includes(ref src, false);
            src = src.ReplaceLineEndings(Environment.NewLine);
        }
        Assembler Assembler = new Assembler()
        { Src = src };
        Assembler.Build();

        if (UseDebuger)
        {
            List<Token> tokens = Assembler.tokenization.tokens;
            string TokenForamt = "";
            for (int i = 0; i < tokens.Count; i++)
            {
                TokenForamt += $"token{i.ToString().PadRight(5, '0')}{{ {tokens[i].Type.ToString().PadRight(14, ' ')}\t{tokens[i].Value} at line {tokens[i].Line} }}{Environment.NewLine}";
            }
            File.WriteAllText(TokenFile, TokenForamt);
            File.WriteAllText(OutputSrcFile, Assembler.Src);
            File.WriteAllLines(ParserTokenFile, Assembler.generation.ParserTokens);
        }
        switch (formats)
        {
            case Formats.Bin:
                File.WriteAllLines(OutputFile, Assembler.generation.MachineCode);
                break;
            case Formats.Obj:
                string Format = "";
                byte[] bytes = Array.Empty<byte>();
                bytes = ObjFormatter.GetOBJ();
                for (int i = 0; i < bytes.Length; i++)
                {
                    Format += (char)bytes[i];
                }
                File.WriteAllText(OutputFile, Format);
                break;
            default:
                break;
        }

        /*
        if (Assembler.HasError)
            Environment.Exit(0);
        */

        Console.WriteLine("starter");
        //Environment.Exit(0);
        if (Build == false)
        {
            Console.WriteLine("DONT BUILD == TRUE");
            Environment.Exit(0);
        }
        if (DoClear) Console.Clear();

        Max_Length += 1;

        if (Max_Length > 0xFFFFF + 1) Max_Length = 0x100000;

        Console.Clear();
        Console.WriteLine("Writing");
        WriteOutBin(Assembler.generation);

        // main
        Console.WriteLine("DONE");
        //File.WriteAllText(OutputFile, outputString);

    }
    private static string GetPathFromFile(string File)
    {
        string[] Path = File.Split('/');
        string Result = "";
        for (int i = 0; i < Path.Length - 1; i++)
        {
            Result += Path[i] + "/";
        }
        return Result.TrimEnd('/');
    }
    private static void Includes(ref string src, bool Mult, List<FileInfo> FilesCopy = null)
    {
        for (int i = 0; i < src.Split("\r\n").Length; i++)
        {
            bool InTheSame = false;
            if (src.Split("\r\n")[i].Contains(".include"))
            {
                string FileName = src.Split("\r\n")[i].Split(' ')[1];
                FileName = FileName.Replace("/", "\\");
                string file = FileName.Replace(".\\", CurrentDirectory + "\\");

                if (FileName == "BIOS")
                {
                    file = LibsPath + "\\BIOS.basm";
                }


                // Check if the file path is relative, and if so, combine it with the current directory
                if (!Path.IsPathRooted(file))
                {
                    file = Path.GetFullPath(file);
                }

                // Replace forward slashes with the correct directory separator
                file = file.Replace('/', Path.DirectorySeparatorChar);

                // Trim any leading or trailing whitespace
                file = file.Trim();

                FileInfo fileInfo = new FileInfo(file);
                bool fileExists = fileInfo.Exists;

                if (fileExists)
                {
                    src = src.Replace(src.Split("\r\n")[i], ".newfile " + file + Environment.NewLine + File.ReadAllText(file));
                }

                if (Mult == true)
                {
                    for (int f = 0; f < FilesCopy.Count; f++)
                    {
                        if (file == FilesCopy[f].FullName)
                        {
                            FilesCopy.RemoveAt(f);
                            InTheSame = true;
                        }
                    }
                    if (InTheSame == true)
                    {
                        src = src.Replace(src.Split("\r\n")[i], ".newfile " + file + "\r\n; in the same dir " + file + Environment.NewLine + File.ReadAllText(file));
                        continue;
                    }
                }
                if (File.Exists(file))
                {
                    src = src.Replace(src.Split("\r\n")[i], ".newfile " + file + Environment.NewLine + File.ReadAllText(file));
                }
                else
                {
                    Console.WriteLine("File not found " + file);
                    Environment.Exit(1);
                }
            }
        }
    }

    static void WriteOutBin(Generation generation)
    {
        string outputString = "";
        for (int i = 0; i < Max_Length; i++)
        {
            if (!(i < generation.MachineCode.Count)) break;
            if (string.IsNullOrEmpty(generation.MachineCode[i]) == false)
            {
                if (i >= 0x30000 && i <= 0x30FFF) continue;
                if (i >= 0x37000 && i <= 0xFFFF9) continue;
                //Console.WriteLine("data " + assembler.MCcode[i]);
                string Value = generation.MachineCode[i].Trim().ToUpper();
                if (i % 16 == 0)
                {
                    if (i == 0)
                    {
                        outputString += "".PadLeft(Pading_Length, '0') + ": ";
                    }
                    else
                    {
                        outputString += "\n" + Convert.ToString(i, 16).PadLeft(Pading_Length, '0').ToUpper() + ": ";
                    }
                }
                outputString += Value.PadLeft(Pading_Length, '0') + " ";
            }
        }
        if (File.Exists(BinFile))
        {
            File.WriteAllText(BinFile, outputString);
        }
        else
        {
            File.Create(BinFile).Close();
            File.WriteAllText(BinFile, outputString);
        }
    }
    private static void Exit(int exitCode = 0)
    {
        Environment.Exit(exitCode);
    }

    static void DecodeArguments(string[] text)
    {
        DecodeInstr(text, "Input", ref InputFile);
        DecodeInstr(text, "InputDir", ref ProgramPath);
        DecodeInstr(text, "Output", ref OutputFile);
        DecodeInstr(text, "Format", ref FormatOutputFile);
        DecodeInstr(text, "DontBuild", ref Build, "false", "true");
        DecodeInstr(text, "Silent", ref DoClear, true);
        DecodeInstr(text, "FillTo", ref Max_Length);
        DecodeInstr(text, "UseBinary", useBinFiles);
        DecodeInstr(text, "UseDebuger", useDebug);
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

    static bool useBinFiles(int index, string[] args)
    {
        BinFile = args[index];

        return false;
    }
    static bool useDebug(int index, string[] args)
    {
        UseDebuger = true;
        OutputSrcFile = "./Builds/src.txt";
        TokenFile = "./Builds/Tokens.txt";
        ParserTokenFile = "./Builds/ParserTokens.txt";
        return false;
    }
    static void Conv(ref string file)
    {
        file = file.Replace(@"\", "/");
        file = file.Replace("./", CurrentDirectory + "/");
        file = file.Replace(@"\", "/");
        return;
    }
}