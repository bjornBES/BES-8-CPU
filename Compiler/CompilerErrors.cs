namespace Compiler
{
    public static class CompilerErrors
    {
        public static void ErrorCantFindInputFile(string FilePath)
        {
        }
        public static void ErrorSameName(string file, Token token, string Name)
        {
            //Error("201230", file, token.Line);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Systax error: ");
            Console.ResetColor();
            Console.WriteLine($"at line {token.Line} A variable or function named '{Name}' is already defined");
            Console.WriteLine("");
            Environment.Exit(1);
        }
        public static void ErrorVariableNotFound(string Name)
        {
        }

        public static void SyntaxError(string file, Token token, TokenType expectedToken)
        {
            //Error("201230", file, token.Line);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Systax error: ");
            Console.ResetColor();

            Console.WriteLine($"at line {token.Line} Expected an {Tokenization.to_string(expectedToken)}");
            Console.WriteLine("");
            Environment.Exit(1);
        }

        static void WriteDown(string[] src, int count, int startingIndex)
        {
            int writing = 0;
            for (int i = startingIndex; i < src.Length && writing != count; i++)
            {
                if (!string.IsNullOrEmpty(src[i]))
                {
                    writing++;
                    Console.WriteLine(src[i]);
                }
            }
        }

        private static void Error(string code, string File, int LineNumber)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.Write(" BES-" + code + " ");
            Console.WriteLine("in " + File + " at line " + LineNumber);
        }
        private static int GetSrcLineNumber(string path)
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
