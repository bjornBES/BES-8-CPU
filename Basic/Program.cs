namespace Basic;
public class Program
{
    public static string InputFile = "\0\0";
    public static string OutputFile = "";
    public static string DebugFile = "";
    public static void Main(string[] args)
    {
        DecodeArguments(args);
        if (InputFile == "\0\0") Exit(1);
        if (OutputFile == "") OutputFile = "a.out";

        InputFile = InputFile.Replace(".\\", "");
        InputFile = InputFile.Replace("./", "");
        InputFile = InputFile.Replace("/", "\\");

        string OutputPath = Environment.CurrentDirectory + "\\" + OutputFile;
        string InputPath = Environment.CurrentDirectory + "\\" + InputFile;
        string DebugPath = Environment.CurrentDirectory + "\\" + DebugFile;

        if (File.Exists(InputPath) == false) Exit(1);
        if (File.Exists(OutputPath) == false) File.Create(OutputPath, 100).Close();

        // go in to the assembler

        string[] src = File.ReadAllText(InputPath).Split("\r\n");
        BasicInt basic = new BasicInt()
        {
            DebugFile = DebugPath,
        };
        basic.Build(src);
        string outputString = "";
        //string BinStr = "";
        Console.WriteLine("starter");
        for (int i = 0; i < basic.Asm.Length; i++)
        {
            if (string.IsNullOrEmpty(basic.Asm[i]) == false)
            {
                //Console.WriteLine(assembler.MCcode[i]);
                ushort Value = Convert.ToUInt16(basic.Asm[i].Trim(), 2);
                outputString += Convert.ToString(Value, 16).PadLeft(4, '0') + "|";
            }
        }
        File.WriteAllText(OutputPath, outputString);
    }

    private static void Exit(int exitCode = 0)
    {
        Environment.Exit(exitCode);
    }

    static void DecodeArguments(string[] args)
    {
        DecodeInstr(args, "-i", ref InputFile);
        DecodeInstr(args, "-o", ref OutputFile);
        DecodeInstr(args, "-D", ref DebugFile);
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
}