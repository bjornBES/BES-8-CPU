using Compiler;
using Compiler.nodes;
using System.Text.Json;

internal class Program
{
    static string InputFile = "\0\0";
    static string OutputFile = "";
    public static string ProgramPath = "";
    public static string OGPorgramPath = "";

    static bool DoClear = false;

    static Tokenization Tokenization = new Tokenization();
    static Parser Parser = new Parser();
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
        src = src.Replace("\r\n", "\n");
        SrcCode = src.Split(Environment.NewLine);
        Tokenization.Build(src);
        Console.WriteLine("TOKEN DONE");
        NodeProg nodes = Parser.Parse_Prog(Tokenization.tokens.ToArray());
        string StringFormatter = "";

        StringFormatter += "".PadLeft(0, '\t') + $"{{\r\n";
        StringFormatter += "".PadLeft(1, '\t') + $"\"{nodes.GetType().Name}\": {{\r\n";
        FormatNodes(nodes.stmts, ref StringFormatter,2);

        StringFormatter = StringFormatter.Substring(0, StringFormatter.Length - 17);

        StringFormatter += $"}}\r\n";
        StringFormatter += "".PadLeft(3, '\t') + $"}}\r\n";
        StringFormatter += "".PadLeft(2, '\t') + $"}}\r\n";
        StringFormatter += "".PadLeft(1, '\t') + $"}}\r\n";
        StringFormatter += "".PadLeft(0, '\t') + $"}}\r\n";
        
        File.WriteAllText($"{CurrentDirectory}/Builds/Compiler/parserNodes.json", StringFormatter);
        string jsonString = JsonSerializer.Serialize(nodes, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText($"{CurrentDirectory}/Builds/Compiler/parserNodes.txt", jsonString);
        Generation.gen_prog(nodes);
        Console.WriteLine("GENERATION DONE");
        
        if (DoClear) Console.Clear();

        WriteOutAsm();
    }

    static void FormatNodes(NodeStmt[] nodes, ref string StringFormatter, int taps = 0)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].stmt == null) continue;
            StringFormatter += "".PadLeft(0  + taps, '\t') + $"\"{nodes[i].stmt.GetType().Name}\": ";
            StringFormatter += $"{{\r\n";
            if (nodes[i].stmt.GetType() == typeof(NodeStmtFunc))
            {
                NodeStmtFunc stmtFunc = (NodeStmtFunc)nodes[i].stmt;
                StringFormatter += "".PadLeft(1 + taps, '\t') + $"\"{stmtFunc.Name.GetType().Name}\": {{\r\n";

                NodeTermIdent nodeTermIdent = (NodeTermIdent)stmtFunc.Name.var;
                StringFormatter += "".PadLeft(2 + taps, '\t') + $"\"{nodeTermIdent.GetType().Name}\": {{\r\n";

                FormatNodeExpr(nodeTermIdent, ref StringFormatter, taps + 1);

                StringFormatter += "".PadLeft(2 + taps, '\t') + $"}}\r\n";

                StringFormatter += "".PadLeft(1 + taps, '\t') + $"}},\r\n";

                StringFormatter += "".PadLeft(1 + taps, '\t') + $"\"{stmtFunc.ReturnType.GetType().Name}\": {{\r\n";
                StringFormatter += "".PadLeft(2 + taps, '\t') + $"\"ReturnType\":\"{stmtFunc.ReturnType.type.GetType().Name}\"\r\n";
                StringFormatter += "".PadLeft(1 + taps, '\t') + $"}}\r\n";
            }
            else if (nodes[i].stmt.GetType() == typeof(NodeScope))
            {
                NodeScope nodeScope = (NodeScope)nodes[i].stmt;
                FormatNodes(nodeScope.stmts, ref StringFormatter, taps + 1);
            }
            else if (nodes[i].stmt.GetType() == typeof(NodeStmtAssign))
            {
                NodeStmtAssign nodeStmtInt = (NodeStmtAssign)nodes[i].stmt;
                NodeTermIdent nodeTermIdentExpr = (NodeTermIdent)nodeStmtInt.name.var;
                StringFormatter += "".PadLeft(1 + taps, '\t') + $"\"{nodeStmtInt.name.GetType().Name}\": {{\r\n";
                FormatNodeExpr(nodeTermIdentExpr, ref StringFormatter, taps);
                StringFormatter += "".PadLeft(1 + taps, '\t') + $"}},\r\n";

                NodeTermIntLit nodeTermIdentValue = (NodeTermIntLit)nodeStmtInt.value.var;
                StringFormatter += "".PadLeft(1 + taps, '\t') + $"\"{nodeTermIdentValue.int_lit.GetType().Name}\": {{\r\n";
                FormatNodeExpr(nodeTermIdentValue, ref StringFormatter, taps);
                StringFormatter += "".PadLeft(1 + taps, '\t') + $"}}\r\n";
            }
            else if (nodes[i].stmt.GetType() == typeof(NodeStmtReturn))
            {
                NodeStmtReturn stmtReturn = (NodeStmtReturn)nodes[i].stmt;
                StringFormatter += "".PadLeft(1 + taps, '\t') + $"\"{stmtReturn.expr.GetType().Name}\": {{\r\n";

                NodeTermIntLit nodeTermIntLit = (NodeTermIntLit)stmtReturn.expr.var;
                FormatNodeExpr(nodeTermIntLit, ref StringFormatter, taps);
                StringFormatter += "".PadLeft(1 + taps, '\t') + $"}},\r\n";
            }
            StringFormatter += "".PadLeft(0 + taps, '\t') + "},\r\n";
        }
        StringFormatter = StringFormatter.TrimEnd(',');
    }

    static void FormatNodeExpr(NodeTermIdent nodeExpr, ref string StringFormatter, int taps)
    {
        StringFormatter += "".PadLeft(2 + taps, '\t') + $"\"{nodeExpr.ident}\": {{\r\n";
        StringFormatter += "".PadLeft(3 + taps, '\t') + $"\"type\":\"{nodeExpr.ident.Type}\",\r\n";
        StringFormatter += "".PadLeft(3 + taps, '\t') + $"\"Value\":\"{nodeExpr.ident.Value}\",\r\n";
        StringFormatter += "".PadLeft(3 + taps, '\t') + $"\"Line\":\"{nodeExpr.ident.Line}\",\r\n";
        StringFormatter += "".PadLeft(3 + taps, '\t') + $"\"File\":\"{nodeExpr.ident.File.Replace("\\", "/")}\"\r\n";
        StringFormatter += "".PadLeft(2 + taps, '\t') + $"}}\r\n";
    }
    static void FormatNodeExpr(NodeTermIntLit nodeExpr, ref string StringFormatter, int taps)
    {
        StringFormatter += "".PadLeft(2 + taps, '\t') + $"\"{nodeExpr.int_lit.GetType().Name}\": {{\r\n";
        StringFormatter += "".PadLeft(3 + taps, '\t') + $"\"type\":\"{nodeExpr.int_lit.Type}\",\r\n";
        StringFormatter += "".PadLeft(3 + taps, '\t') + $"\"Value\":\"{nodeExpr.int_lit.Value}\",\r\n";
        StringFormatter += "".PadLeft(3 + taps, '\t') + $"\"Line\":\"{nodeExpr.int_lit.Line}\",\r\n";
        StringFormatter += "".PadLeft(3 + taps, '\t') + $"\"File\":\"{nodeExpr.int_lit.File.Replace("\\", "/")}\"\r\n";
        StringFormatter += "".PadLeft(2 + taps, '\t') + $"}}\r\n";
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

        File.WriteAllText($"{CurrentDirectory}/Builds/Compiler/tokens.txt", TokenFormatOutput);

        string jsonVariablesFormat = "";

        for (int i = 0; i < Generation.m_stack_var.Count; i++)
        {
            Variable variable = Generation.m_stack_var[i];
            jsonVariablesFormat += 
                $"{variable.Name}{{\n" +
                $"\t" + $"Addr = {variable.Stack_loc}" + "\n" +
                $"\t" + $"Size = {variable.Size}" + "\n" +
                "}\n";
        }

        File.WriteAllText($"{CurrentDirectory}/Builds/Compiler/Variables.txt", jsonVariablesFormat);

        File.WriteAllLines(OutputFile, Generation.m_output);
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
