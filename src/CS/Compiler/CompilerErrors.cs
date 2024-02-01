namespace Compiler
{
    public static class CompilerErrors
    {
        public static void ErrorCantFindInputFile(string FilePath)
        {
        }
        public static void ErrorSameName(string file, string[] src, int index, int linenumber, string Name)
        {
            Error("201230", file, linenumber);

            Console.WriteLine($"{src.Length} {index + 2} {index} {index + 1}");


            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(src[index]);
            Console.ResetColor();

            if(src.Length > index + 1)
                Console.WriteLine(src[index + 1]);
            if(src.Length > index + 2)
                Console.WriteLine(src[index + 2]);

            Console.WriteLine($"names all ready ex");
            Environment.Exit(1);
        }
        public static void ErrorVariableNotFound(string Name)
        {
        }

        public static void SyntaxError(string file, string[] src, int index, int linenumber)
        {
            Error("201230", file, linenumber);

            Console.WriteLine($"{src.Length} {index} {index + 1} {index + 2} {src[index]} {src[index].Split(' ').Length}");

            Console.WriteLine("A".PadLeft(src[index].Split(' ')[0].Length - 1, '-'));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(src[index]);
            Console.ResetColor();
            Console.WriteLine("".PadLeft(src[index].Length - 1, '~'));

            WriteDown(src, 2, index + 1);

            Console.WriteLine($"Systax error");
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
