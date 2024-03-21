namespace assembler
{
    public static class AssemblerErrors
    {
        public static void LabelNotFound(string labelName)
        {
            Error("E00005");
            Console.WriteLine($"Label was not found in the project {labelName}");
            Console.WriteLine($"try to see if the label is local or not global");
            Exit(-1);
        }
        public static void ErrorUnexpectedToken(Token token)
        {
            Error("E00001", token);
            Console.WriteLine("0");

            Console.WriteLine($"Unexpected {token.Type}");
            Exit(-1);
        }
        public static void ErrorExpectedToken(Token token, TokenType ExpectedToken)
        {
            Error("E00002", token);
            Console.WriteLine("0");

            Console.WriteLine($"Expected {ExpectedToken}");
            Exit(-1);
        }

        public static void ErrorBitSize(Token token)
        {
            Error("E00003", token);
            Console.WriteLine("0");

            Console.WriteLine("The bit size is over 16 bits");
            Exit(-1);
        }
        public static void ErrorCantFindInputFile(string FilePath)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(FilePath);
            Console.ResetColor();
            Console.WriteLine("can't find the input file");
            Exit(-1);
        }
        public static void ErrorInstructionNotFound(Token Instr)
        {
            Error("E00004", Instr); //todo error codes
            Console.WriteLine("0");

            FormatText(ConsoleColor.Red, Instr.Value + " ");
            FormatText(ConsoleColor.Red, "".PadLeft(Instr.Value.Length, '~') + " ");
            Console.WriteLine("can't find the Instruction");
            Exit(-1);
        }
        private static void Error(string code, Token token)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.Write(" BES-" + code + " ");
            Console.Write($"in {token.File}:{token.Line}:");
        }
        private static void Error(string code)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.WriteLine(" BES-" + code + " ");
        }
        static void FormatText(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        static void Exit(int exitCode)
        {
            Environment.Exit(exitCode);
        }
    }
}