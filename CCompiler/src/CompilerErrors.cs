using System;

public static class CompilerErrors
{
    public static void Erorr_Expected_Token(Token token, TokenType expectedToken)
    {
        Error("00000", $"Expected {token}", token);
        Console.WriteLine(expectedToken.ToString());
        Environment.Exit(-1);
    }
    public static void Erorr_Unexpected_Token(Token token, TokenType unexpectedToken)
    {
        Error("00000", $"Expected {token}", token);
        Console.WriteLine(unexpectedToken.ToString());
        Environment.Exit(-1);
    }
    public static void Erorr_Unexpected_Operator(Token token)
    {
        Error("00001", $"Unexpected {token}", token);
        Environment.Exit(-1);
    }
    private static void Error(string errorCode, string msg, Token token)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("BEC:" + errorCode + " ");
        Console.ResetColor();
        Console.WriteLine(token.FormatPath());
        Console.WriteLine(msg);
    }
}