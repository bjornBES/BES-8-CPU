using assembler;
using System.Security.AccessControl;
using System.Text;

public class Program
{
    static string InputFile = "\0\0";
    static string SrcCopyFile = "";
    static string ObjFile = "";
    static string OutputFile = "";
    static string BinFile = "";
    static string TokenFile = "";
    static string OutputSrcFile = "";
    public static string ProgramPath = "";
    public static string OGPorgramPath = "";
    public static string LibsPath = "C:\\Users\\bjorn\\Desktop\\CPUs\\BES-8-CPU\\Libs";
    public static string CurrentDirectory = "C:\\Users\\bjorn\\Desktop\\CPUs\\BES-8-CPU";

    static bool Build = true;
    static bool DoClear = false;

    static bool UseDebuger = false;
    static bool WriteBin = false;
    public static List<FileInfo> Files = new List<FileInfo>();

    public static int Max_Length = 0x2FFFF + 1;
    public const int Pading_Length = 5;

    public static void Main(string[] args)
    {
        if (args.Length > 2)
        {
            if (args[1] == "-D")
            {
                if (args.Length == 4)
                {
                    DecodeArguments((
                        $"Input ./src/asm/{args[2]}\r\n" +
                        $"InputDir ./src/asm/{args[3]}\r\n" +
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
        else
        {
            string SettingsFile = args[0];
            Conv(ref SettingsFile);
            DecodeArguments(File.ReadAllText(SettingsFile).Split("\r\n"));
        }
        Console.CursorVisible = false;

        if (InputFile == "\0\0") Assembler.AssemblerErrors.ErrorCantFindInputFile("");
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
        if (SrcCopyFile != "")
        {
            Conv(ref SrcCopyFile);
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
        if (ObjFile != "")
        {
            Conv(ref ObjFile);
        }
        string Input = InputFile;
        Conv(ref Input);
        InputFile = InputFile.Replace(".\\", "/");
        InputFile = InputFile.Replace("./", "/");
        InputFile = InputFile.Replace("/", "\\");
        Conv(ref OutputFile);

        Console.WriteLine("ProgramPath " + ProgramPath);
        Console.WriteLine("Input File " + Input);

        if (File.Exists(Input) == false) Assembler.AssemblerErrors.ErrorCantFindInputFile(InputFile);
        if (File.Exists(OutputFile) == false) File.Create(OutputFile, 100).Close();
        string src;
        // go in to the assembler
        if (Directory.Exists(ProgramPath))
        {
            DirectoryInfo Programdirectory = new DirectoryInfo(ProgramPath);
            Files = Programdirectory.GetFiles("*.Basm").ToList();
            List<FileInfo> FilesCopy = Files;
            src = ".newfile " + Input + "\r\n";
            src += File.ReadAllText(Input) + "\r\n";
            Includes(ref src, true, FilesCopy);

            for (int f = 0; f < FilesCopy.Count; f++)
            {
                if (FilesCopy[f].FullName != Input)
                {
                    src += ".newfile " + OGPorgramPath + "\\" + FilesCopy[f].Name + "\r\n";
                    src += File.ReadAllText(FilesCopy[f].FullName) + "\r\n";
                }
            }
        }
        else
        {
            src = File.ReadAllText(Input);
            src = ".newfile " + Input + "\r\n" + src;

            src = src.Replace("\n", "\r\n");
            Includes(ref src, false);
            src = src.Replace("\n", "\r\n");
        }
        Assembler.Build(src.Split("\r\n"));

        if (UseDebuger)
        {
            File.WriteAllLines(TokenFile, Assembler.AssemblerLists.Tokens.ToArray());
            File.WriteAllLines(OutputSrcFile, Assembler.OrgSrc);
        }

        if (Assembler.HasError)
            Environment.Exit(0);

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
        Thread BinThread = new Thread(new ThreadStart(WriteOutBin));
        if (WriteBin == true)
        {
            BinThread.Start();
        }

        int SaveProgressBar = 0;
        string outputString = "";

        // main
        if (ObjFile != "")
        {
            string formated = "";

            for (int i = 0; i < Assembler.AssemblerObj.OBJString.Count; i++)
            {
                formated += Assembler.AssemblerObj.OBJString[i];
            }

            File.WriteAllText(ObjFile, formated);
        }
        if (SrcCopyFile != "")
        {
            File.WriteAllLines(SrcCopyFile, Assembler.OutSrc);
        }
        for (int i = 0; i < Max_Length; i++)
        {
            if (i > Assembler.MaxPC) break;
            if (!(i < Assembler.AssemblerLists.MCcode.Length)) break;
            if (string.IsNullOrEmpty(Assembler.AssemblerLists.MCcode[i]) == false)
            {
                //Console.WriteLine("data " + assembler.MCcode[i]);
                string Value = Assembler.AssemblerLists.MCcode[i].Trim().ToUpper();
                outputString += Value.PadLeft(Pading_Length, '0') + "|";
                // info


                int percentComplete = (int)(0.5f + ((100f * i) / (Max_Length - 1)));
                if (DoClear == false)
                {
                    int ProgressBar = (int)(0.5f + (((Console.WindowWidth - 1) * i) / (Max_Length - 1)));
                    for (int p = SaveProgressBar; p < ProgressBar; p++)
                    {
                        Console.SetCursorPosition(p, Console.WindowHeight - 1);
                        Console.Write("=");
                        Console.SetCursorPosition(p, Console.WindowHeight - 2);
                        Console.Write("-");
                    }
                    SaveProgressBar = ProgressBar;
                    Console.SetCursorPosition((Console.WindowWidth - 18 / 2) / 2, Console.WindowHeight - 2);
                    Console.Write(Convert.ToString(i, 16).PadLeft(5, '0') + " / " + Convert.ToString(Max_Length, 16));
                    Console.SetCursorPosition((Console.WindowWidth - 14 / 2) / 2, Console.WindowHeight - 1);
                    Console.Write(percentComplete.ToString().PadLeft(3, '0') + "% / 100%");
                }
            }
        }
        Console.WriteLine("DONE");
        File.WriteAllText(OutputFile, outputString);

    }

    private static void Includes(ref string src, bool Mult, List<FileInfo> FilesCopy = null)
    {
        for (int i = 0; i < src.Split("\r\n").Length; i++)
        {
            bool InTheSame = false;
            if (src.Split("\r\n")[i].Contains(".include"))
            {
                string FileName = src.Split("\r\n")[i].Split(' ')[1];
                string file = Path.Combine(CurrentDirectory, FileName);
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
                    src = src.Replace(src.Split("\r\n")[i], ".newfile " + file + "\r\n" + File.ReadAllText(file));
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
                        src = src.Replace(src.Split("\r\n")[i], ".newfile " + file + "\r\n; in the same dir " + file + "\r\n" + File.ReadAllText(file));
                        continue;
                    }
                }
                if (File.Exists(file))
                {
                    src = src.Replace(src.Split("\r\n")[i], ".newfile " + file + "\r\n" + File.ReadAllText(file));
                }
                else
                {
                    // todo Error
                    Environment.Exit(1);
                }
            }
        }
    }

    static void WriteOutBin()
    {
        string outputString = "";
        for (int i = 0; i < Max_Length; i++)
        {
            if (!(i < Assembler.AssemblerLists.MCcode.Length)) break;
            if (string.IsNullOrEmpty(Assembler.AssemblerLists.MCcode[i]) == false)
            {
                if (i >= 0x30000 && i <= 0x30FFF) continue;
                if (i >= 0x37000 && i <= 0xFFFF9) continue;
                //Console.WriteLine("data " + assembler.MCcode[i]);
                string Value = Assembler.AssemblerLists.MCcode[i].Trim().ToUpper();
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
        DecodeInstr(text, "ObjFile", ref ObjFile);
        DecodeInstr(text, "DontBuild", ref Build, "false", "true");
        DecodeInstr(text, "Silent", ref DoClear, true);
        DecodeInstr(text, "FillTo", ref Max_Length);
        DecodeInstr(text, "UseBinary", useBinFiles);
        DecodeInstr(text, "UseDebuger", useDebug);
        DecodeInstr(text, "SrcFile", ref SrcCopyFile);
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
        WriteBin = true;
        BinFile = args[index];

        return false;
    }
    static bool useDebug(int index, string[] args)
    {
        UseDebuger = true;
        OutputSrcFile = "./Builds/src.txt";
        TokenFile = "./Builds/Tokens.txt";
        return false;
    }
    static void Conv(ref string file)
    {
        file = file.Replace(".\\", CurrentDirectory + "\\");
        file = file.Replace("./", CurrentDirectory + "\\");
        file = file.Replace("/", "\\");
        return;
    }
}