namespace Compiler
{
    public static class CompilerErrors
    {
        public static void ErrorCantFindInputFile(string FilePath)
        {
        }
        public static void ErrorInstructionNotFound(string Instr)
        {
        }
        public static void ErrorVariableNotFound(string Name)
        {
        }
        private static void Error(string code)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.Write(" BES-" + code + " ");
            //Console.Write("in " + Assembler.CurrentFile + " at line " + Assembler.LineNumber);

            //Console.WriteLine(GetLineNumber(Assembler.Src[Assembler.LineIndex]));
            //Console.WriteLine("\t\t" + Assembler.Src[Assembler.LineIndex]);
            Console.Write("\t\t");
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
