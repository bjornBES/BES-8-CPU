using Compiler;
using System.Text.Json;

internal class Program
{
    static string InputFile = "\0\0";
    static string OutputFile = "";
    public static string ProgramPath = "";
    public static string OGPorgramPath = "";

    static bool DoClear = false;

    static Tokenization Tokenization = new Tokenization();
    static Generation Generation = new Generation();
    public static FileInfo[] Files;
    public static string[] SrcCode;

    public static string CurrentDirectory = "C:\\Users\\bjorn\\Desktop\\BES-8-CPU\\CPUs\\BES-8-CPU";

    public static void Main(string[] args)
    {
        Console.CursorVisible = false;
        DecodeArguments(args);
        if (InputFile == "\0\0") { Console.WriteLine("Missing Input Arguments"); Exit(1); }
        if (OutputFile == "")
        {
            OutputFile = "./a.asm";
        }
        if (ProgramPath != "")
        {
            Conv(ref ProgramPath);
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

        string src = "";

        if (Directory.Exists(ProgramPath))
        {
            DirectoryInfo Programdirectory = new DirectoryInfo(ProgramPath);
            Files = Programdirectory.GetFiles("*.BEc");
            src += File.ReadAllText(Input) + Environment.NewLine;
            for (int i = 0; i < Files.Length; i++)
            {
                if (Files[i].FullName != Input)
                {
                    src += File.ReadAllText(Files[i].FullName) +
                        Environment.NewLine +
                        $"FILE{Files[i].FullName}" +
                        Environment.NewLine;
                }
            }
        }
        else
        {
            //src = "newfile " + Input + "\r\n" + src;
        }
        src = "Main(10,20,30)" + Environment.NewLine + src;
        src = $"FILE:{Input}{Environment.NewLine}" + File.ReadAllText(Input);
        SrcCode = src.Split(Environment.NewLine);
        Tokenization.Build(src);
        Console.WriteLine("TOKEN DONE");
        Generation.Build(Tokenization);
        Console.WriteLine("GENERATION DONE");
        
        if (DoClear) Console.Clear();

        WriteOutAsm();
    }
    static void WriteOutAsm()
    {
        Console.WriteLine("WRITEING");

        string TokenFormatOutput = "";

        for (int i = 0; i < Tokenization.tokens.Count; i++)
        {
            Token token = Tokenization.tokens[i];

            string Value = token.Value == null ? "NULL" : token.Value; 
            TokenFormatOutput += $"Token = {{{token.Type}, {Value} at line {token.Line}}}\n";
        }

        File.WriteAllText($"{CurrentDirectory}/tokens.txt", TokenFormatOutput);

        string jsonVariablesFormat = "";

        for (int i = 0; i < Generation.Variables.Count; i++)
        {
            Variable variable = Generation.Variables[i];
            jsonVariablesFormat += 
                $"{variable.Name}{{\n" +
                $"\t" + $"Addr = {variable.Address}" + "\n" +
                $"\t" + $"Size = {variable.Size}" + "\n" +
                $"\t" + $"FuncName = {variable.FuncName}" + "\n" +
                $"\t" + $"Settings {{" + "\n" +
                $"\t\t" + $"IsLocal = {variable.IsLocal}" + "\n" +
                $"\t\t" + $"IsPublic = {variable.IsPublic}" + "\n" +
                $"\t\t" + $"IsConst = {variable.IsConst}" + "\n" +
                $"\t\t" + $"IsGlobal = {variable.IsGlobal}" + "\n" +
                $"\t\t" + $"IsProtected = {variable.IsProtected}" + "\n" +
                $"\t\t" + $"IsPtr = {variable.IsPtr}" + "\n" +
                "\t}\n" +
                "}\n";
        }

        File.WriteAllText($"{CurrentDirectory}/Variables.txt", jsonVariablesFormat);

        //string jsonFunctionsFormat = JsonSerializer.Serialize(Generation.functions, new JsonSerializerOptions { WriteIndented = true });
        //File.WriteAllText($"{CurrentDirectory}/Functions.json", jsonFunctionsFormat);

        File.WriteAllText($"{CurrentDirectory}/tokens.txt", TokenFormatOutput);
        File.WriteAllLines(OutputFile, Generation.Assembly_Src);
    }
    private static void Exit(int exitCode = 0)
    {
        Environment.Exit(exitCode);
    }

    static void DecodeArguments(string[] args)
    {
        DecodeInstr(args, "-i", ref InputFile);
        DecodeInstr(args, "-o", ref OutputFile);
        DecodeInstr(args, "-D", ref Generation.Debug, true);
        DecodeInstr(args, "-s", ref DoClear, true);
        DecodeInstr(args, "-I", ref ProgramPath);
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

    static void Conv(ref string file)
    {
        file = file.Replace(".\\", CurrentDirectory + "\\");
        file = file.Replace("./", CurrentDirectory + "\\");
        file = file.Replace("/", "\\");
        return;
    }
}
