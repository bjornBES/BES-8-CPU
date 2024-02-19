namespace assembler
{
    public partial class Assembler
    {
        public static class AssemblerErrors
        {
            public static void ErrorSyntax(string From)
            {
                Error("19094"); // todo error codes
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("".PadLeft(OrgSrc[LineIndex].Length, '~') + " ");
                Console.WriteLine("");
                Console.ResetColor();
                Console.WriteLine("Error Syntax from " + From);
            }
            public static void ErrorSyntax(string line, string where)
            {
                Error("19094"); // todo error codes
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("".PadLeft(OrgSrc[LineIndex].Length, '~') + " ");
                Console.WriteLine("");
                Console.ResetColor();
                Console.WriteLine("Error Syntax case " + line + " at " + where);
            }
            public static void ErrorRegisterNotFound(string regsiter)
            {
                Error("19094"); // todo error codes
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("".PadLeft(OrgSrc[LineIndex].Length, '~') + " ");
                Console.WriteLine("");
                Console.ResetColor();
                Console.WriteLine("can't find the Register");
            }
            public static void ErrorDirInstructionNotFound(string Instr)
            {
                Error("19094"); //todo error codes
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("".PadLeft(Instr.Length + 1, '~') + " ");
                Console.WriteLine("");
                Console.ResetColor();
                Console.WriteLine("can't find the Instruction");
            }
            public static void ErrorInstrLength(int ErrorIndex, string[] arg)
            {
                Error("19093"); //todo error codes
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("".PadLeft(ErrorIndex, '~') + " ");
                for (int i = 0; i < arg.Length; i++)
                {
                    Console.Write("".PadLeft(arg[i].Length, '~') + " ");
                }
                Console.WriteLine("");
                Console.ResetColor();
                Console.WriteLine("the arguments are not right length");
            }
            public static void ErrorArgIsNull()
            {
                Error("19093"); //todo error codes

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("");
                Console.ResetColor();
                Console.WriteLine("the argument is null empty");
                Environment.Exit(1);
            }
            public static void ErrorCantFindInputFile(string FilePath)
            {
                ErrorOutAssembler("19093"); //todo error codes
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(FilePath);
                Console.ResetColor();
                Console.WriteLine("can't find the input file");
            }
            public static void ErrorInstructionNotFound(string Instr)
            {
                Error("19092"); //todo error codes
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("".PadLeft(Instr.Length, '~') + " ");
                Console.WriteLine("");
                Console.ResetColor();
                Console.WriteLine("can't find the Instruction");
            }
            public static void ErrorLebleNotFound(string Name)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error");
                Console.ResetColor();
                Console.Write(" BES-19091 "); //todo error code
                string file = GetFile(Name);
                Console.Write("in " + file + " at line ");
                Console.WriteLine(GetSrcLineNumber(file));
                Console.WriteLine("\t\t" + OrgSrc[LineIndex]);
                Console.Write("\t\t");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Name);
                Console.WriteLine("\t\t" + "".PadLeft(Name.Length, '~'));
                Console.ResetColor();
                Console.WriteLine("can't find the Lable");
                HasError = true;
            }
            public static void ErrorVariableNotFound(string Name)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error");
                Console.ResetColor();
                Console.Write(" BES-19091 "); //todo error code
                Console.WriteLine("in " + CurrentFile + " at line " + LineIndex);
                Console.WriteLine("\t\t" + OrgSrc[LineIndex]);
                Console.Write("\t\t");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Name);
                Console.WriteLine("\t\t" + "".PadLeft(Name.Length, '~'));
                Console.ResetColor();
                Console.WriteLine("can't find the Variable");
            }
            private static void Error(string code)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error");
                Console.ResetColor();
                Console.Write(" BES-" + code + " ");
                Console.Write("in " + CurrentFile + " at line ");

                Console.WriteLine(LineIndex);
                Console.WriteLine("\t\t" + OrgSrc[LineIndex]);
                Console.Write("\t\t");
                HasError = true;
            }
            private static void ErrorOutAssembler(string code)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error");
                Console.ResetColor();
                Console.WriteLine(" BES-" + code + " ");
                Console.Write("\t\t");
                HasError = true;
            }
            public static string GetFile(string HelpSrc)
            {
                if (Program.Files == null) return CurrentFile;

                for (int f = 0; f < Program.Files.Count; f++)
                {
                    string[] src = File.ReadAllText(Program.Files[f].FullName).Split("\r\n");
                    for (int i = 0; i < src.Length; i++)
                    {
                        if (src[i].Contains(HelpSrc)) return Program.Files[f].FullName;
                    }
                }
                Console.WriteLine("ERROR get files " + HelpSrc);
                return "";
            }
            public static int GetSrcLineNumber(string path)
            {
                int Line = 0;
                string[] SrcFile = File.ReadAllText(path).Split("\r\n");
                for (int i = 0; i < SrcFile.Length; i++)
                {
                    if (string.IsNullOrEmpty(SrcFile[i])) { Line++; }
                    else if (SrcFile[i].StartsWith('.')) { Line++; }
                    else if (SrcFile[i].StartsWith('$')) { Line++; }
                    else if (SrcFile[i].EndsWith(':')) { Line++; }
                    else Line++;
                }
                return Line;
            }
        }
    }
}