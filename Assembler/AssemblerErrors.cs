namespace asm
{
    public class AssemblerErrors
    {
        public Assembler Assembler;
        public void ErrorSyntax(string From)
        {
            Error("19094"); // todo error codes
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("".PadLeft(Assembler.Src[Assembler.LineIndex].Length, '~') + " ");
            Console.WriteLine("");
            Console.ResetColor();
            Console.WriteLine("Error Syntax from " + From);
        }
        public void ErrorRegisterNotFound(string regsiter)
        {
            Error("19094"); // todo error codes
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("".PadLeft(Assembler.Src[Assembler.LineIndex].Length, '~') + " ");
            Console.WriteLine("");
            Console.ResetColor();
            Console.WriteLine("can't find the Register");
        }
        public void ErrorDirInstructionNotFound(string Instr)
        {
            Error("19094"); //todo error codes
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("".PadLeft(Instr.Length, '~') + " ");
            Console.WriteLine("");
            Console.ResetColor();
            Console.WriteLine("can't find the Instruction");
        }
        public void ErrorInstrLength(int ErrorIndex, string[] arg)
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
        public void ErrorCantFindInputFile(string FilePath)
        {
            ErrorOutAssembler("19093"); //todo error codes
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(FilePath);
            Console.ResetColor();
            Console.WriteLine("can't find the input file");
        }
        public void ErrorInstructionNotFound(string Instr)
        {
            Error("19092"); //todo error codes
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("".PadLeft(Instr.Length, '~') + " ");
            Console.WriteLine("");
            Console.ResetColor();
            Console.WriteLine("can't find the Instruction");
        }
        public void ErrorLebleNotFound(string Name)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.Write(" BES-19091 "); //todo error code
            string file = GetFile(Name);
            Console.Write("in " + file + " at line ");
            Console.WriteLine(GetSrcLineNumber(file));
            Console.WriteLine("\t\t" + Assembler.OrgSrc[Assembler.LineIndex]);
            Console.Write("\t\t");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Name);
            Console.WriteLine("\t\t" + "".PadLeft(Name.Length, '~'));
            Console.ResetColor();
            Console.WriteLine("can't find the Lable");
            Assembler.HasError = true;
        }
        public void ErrorVariableNotFound(string Name)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.Write(" BES-19091 "); //todo error code
            Console.WriteLine("in " + Assembler.CurrentFile + " at line " + Assembler.LineNumber);
            Console.WriteLine("\t\t" + Assembler.OrgSrc[Assembler.LineIndex]);
            Console.Write("\t\t");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Name);
            Console.WriteLine("\t\t" + "".PadLeft(Name.Length, '~'));
            Console.ResetColor();
            Console.WriteLine("can't find the Variable");
        }
        private void Error(string code)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.Write(" BES-" + code + " ");
            Console.Write("in " + Assembler.CurrentFile + " at line ");

            Console.WriteLine(Assembler.LineNumber);
            Console.WriteLine("\t\t" + Assembler.Src[Assembler.LineIndex]);
            Console.Write("\t\t");
            Assembler.HasError = true;
        }
        private void ErrorOutAssembler(string code)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.WriteLine(" BES-" + code + " ");
            Console.Write("\t\t");
            Assembler.HasError = true;
        }
        private void ErrorPlus(string code)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error");
            Console.ResetColor();
            Console.Write(" BES-" + code + " ");
            Console.Write("in " + Assembler.CurrentFile + " at line " + Assembler.LineNumber);
            //Console.WriteLine(Assembler.LineNumber + " " + Assembler.Src.Length + " " + Assembler.OrgSrc[Assembler.LineNumber]);

            Console.WriteLine(GetLineNumberPlus(Assembler.Src[Assembler.LineIndex])[0] + " - " + GetLineNumberPlus(Assembler.Src[Assembler.LineIndex])[1]);
            Console.WriteLine("\t\t" + Assembler.OrgSrc[Assembler.LineIndex]);
            Console.Write("\t\t");
        }
        public string GetFile(string HelpSrc)
        {
            if (Program.Files == null) return Assembler.CurrentFile;

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
        public int GetSrcLineNumber(string path)
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
        public int[] GetLineNumberPlus(string OGsrc)
        {
            string[] SrcFile = File.ReadAllText(Assembler.CurrentFile).Split("\r\n");
            for (int i = 0; i < SrcFile.Length; i++)
            {
                if (SrcFile[i] == OGsrc)
                {
                    return new int[] { (int)Math.Clamp(i - 2, 0, int.MaxValue), i + 2 };
                }
            }
            Console.WriteLine("line number error " + OGsrc);
            return Array.Empty<int>();
        }
        public int GetLineNumber(string OGsrc)
        {
            string[] SrcFile = File.ReadAllText(Program.ProgramPath + Assembler.CurrentFile).Split("\r\n");
            for (int i = 0; i < SrcFile.Length; i++)
            {
                if (SrcFile[i] == OGsrc)
                {
                    return i;
                }
            }
            Console.WriteLine("line number error " + OGsrc);
            return -1;
        }
    }
}
