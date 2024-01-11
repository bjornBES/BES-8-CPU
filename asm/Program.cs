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
    public static List<FileInfo> Files = new List<FileInfo>();

    public const int Max_Length = 0xFFFFF + 1;
    public const int Pading_Length = 5;

    public static void Main(string[] args)
    {
        Console.CursorVisible = false;

        string SettingsFile = args[0];
        Conv(ref SettingsFile);

        DecodeArguments(File.ReadAllText(SettingsFile).Split("\r\n"));

        //Environment.Exit(0);
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
        else
        {
            ProgramPath = InputFile;
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
            Files = Programdirectory.GetFiles("*.Basm").ToList();
            List<FileInfo> FilesCopy = Files;
            string src;
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

            assembler.Build(src.Split("\r\n"));
        }
        else
        {
            string src = File.ReadAllText(Input);
            src = ".newfile " + Input + "\r\n" + src;

            Includes(ref src, false);

            assembler.Build(src.Split("\r\n"));
        }

        if (UseDebuger)
        {
            File.WriteAllLines(TokenFile, assembler.Tokens.ToArray());
            File.WriteAllLines(OutputSrcFile, assembler.OrgSrc);
        }

        if(assembler.HasError)
            Environment.Exit(0);

        Console.WriteLine("starter");
        //Environment.Exit(0);
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
        for (int i = 0; i < Max_Length; i++)
        {
            if (string.IsNullOrEmpty(assembler.MCcode[i]) == false)
            {
                //if (i >= 0x30000 && i <= 0x30FFF) continue;
                //if (i >= 0x37000 && i <= 0xFFFF9) continue;
                //if (i == 0x31000) outputString += "%31000\r\n";
                //if (i == 0xFFFFA) outputString += "%FFFFA\r\n";
                //Console.WriteLine("data " + assembler.MCcode[i]);
                string Value = assembler.MCcode[i].Trim().ToUpper();
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
                string file = Environment.CurrentDirectory + "\\";
                if(FileName == "BIOS")
                {
                    file += "src/asm/os/BIOS.Basm";
                }
                else
                {
                    file += FileName + ".Basm";
                }
                file = file.Replace("/", "\\");

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
                src = src.Replace(src.Split("\r\n")[i], ".newfile " + file + "\r\n" + File.ReadAllText(file));
            }
        }
    }

    static void WriteOutBin()
    {
        string outputString = "";
        for (int i = 0; i < Max_Length; i++)
        {
            if (string.IsNullOrEmpty(assembler.MCcode[i]) == false)
            {
                if (i >= 0x30000 && i <= 0x30FFF) continue;
                if (i >= 0x37000 && i <= 0xFFFF9) continue;
                //Console.WriteLine("data " + assembler.MCcode[i]);
                string Value = assembler.MCcode[i].Trim().ToUpper();
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
            Console.WriteLine("Can't find the bin file");
            Exit();
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
        DecodeInstr(text, "VariableCounter", ref assembler.VariablePC);
        DecodeInstr(text, "Silent", ref DoClear, true);
        DecodeInstr(text, "UseSections", ref assembler.UseSections);
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
                for (int a = 0; a < instrs.Length; a++)
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
                        Result = Convert.ToUInt32(instrs[a].Remove(0,2), 16);
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
        OutputSrcFile = "./src.txt";
        TokenFile = "./Tokens.txt";
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