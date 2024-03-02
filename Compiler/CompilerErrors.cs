using System.ComponentModel;
using System.Linq.Expressions;

namespace Compiler
{
    public static class CompilerErrors
    {
        public static void ErrorCantFindInputFile(string FilePath)
        {
        }
        public static void ErrorSameName(Token token, string Name)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Error("201230", token);
            Console.WriteLine("0");

            ColorTextFormat("Systax error: ", ConsoleColor.Red);

            Console.WriteLine($"A variable or function named '{Name}' is already defined");
            Console.WriteLine("");
            Environment.Exit(1);
        }
        public static void ErrorVariableNotFound(string Name)
        {
            Console.Write($"Systax error: Variable not found");
        }
        public static void ErrorVariablePointer(string VariableName, Token token, Variable variable)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Error("201230", token);
            GetCol(TokenType.ampersand, token);

            ColorTextFormat("Systax error: ", ConsoleColor.Red);


            Console.WriteLine($"{VariableName} Is a pointer. A pointers address can't be referenced using {Tokenization.to_string(TokenType.ampersand)}");
            Console.WriteLine("\t\t" + Program.SrcCode[SrcLineNumber]);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\t\t" + "".PadLeft(Program.SrcCode[SrcLineNumber].IndexOf('&'), ' ') + "~");
            Console.ResetColor();
        }
        public static void ErrorVariableNotPointer(string VariableName, Token token, Variable variable)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Error("201230", token);

            ColorTextFormat("Systax error: ", ConsoleColor.Red);

            Console.WriteLine($"{VariableName} Is a pointer");
        }
        public static void SyntaxError()
        {
            Console.Write($"Systax error: ");
        }
        public static void ExpectedError(Token token, TokenType expectedToken)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Error("201230", token);
            GetCol(expectedToken, token);

            ColorTextFormat("Systax error: ", ConsoleColor.Red);

            Console.WriteLine($"Expected an {Tokenization.to_string(expectedToken)}");
            Console.WriteLine("");
            Environment.Exit(1);
        }
        public static void ExpectedExpressionError(Token token)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Error("201230", token);
            Console.WriteLine("0");

            ColorTextFormat("Systax error: ", ConsoleColor.Red);

            Console.WriteLine($"Expected an Expression");
            Console.WriteLine("");
            Environment.Exit(1);
        }
        public static void ExpectedError(Token token)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Error("201230", token);
            Console.WriteLine("0");

            ColorTextFormat("Systax error: ", ConsoleColor.Red);

            Console.WriteLine($"Expected an {Tokenization.to_string(token.Type)}");
            Console.WriteLine("");
            Environment.Exit(1);
        }
        public static void ExpectedSomeThingError(Token token)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Error("201230", token);
            Console.WriteLine("0");

            ColorTextFormat("Systax error: ", ConsoleColor.Red);

            Console.WriteLine($"Expected char");
            Console.WriteLine("");
            Environment.Exit(1);
        }
        public static void UnexpectedError(Token token)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Error("201230", token);
            Console.WriteLine("0");
            ColorTextFormat("Systax error: ", ConsoleColor.Red);

            Console.WriteLine($"Expected an {Tokenization.to_string(token.Type)}");
            Console.WriteLine("");
            Environment.Exit(1);
        }

        private static int GetSrcLineNumber(Token token)
        {
            return token.Line + 1;
        }
        private static void ColorTextFormat(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }
        private static void GetCol(TokenType expectedToken, Token token)
        {
            int SrcLineNumber = GetSrcLineNumber(token);
            Console.WriteLine($"{Program.SrcCode[SrcLineNumber].IndexOf(Tokenization.ToString(expectedToken))}");
        }
        private static void Error(string code, Token token)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.Write(" BES-" + code + " ");
            Console.Write($"in {token.File}:{token.Line}:");
        }
    }
}
