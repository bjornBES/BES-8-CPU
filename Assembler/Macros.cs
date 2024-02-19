namespace assembler
{
    public class Macros
    {
        public string Name;
        public List<string> Args;
        public string[] Instrs;
        public uint PCOffset;

        public void InsertMacro(ref string[] SRC, ref int PC)
        {
            Console.WriteLine("MAKEING");
            int EndIndex = 0;
            for (int i = 0; i < SRC.Length; i++)
            {
                if (SRC[i].StartsWith('&'))
                {
                    EndIndex = i + 1;
                    break;
                }
            }

            if (EndIndex == 0) return;

            List<string> PreSrc = SRC.Take(new Range(new Index(0), new Index(EndIndex))).ToList();
            string[] Rest = SRC.Take(new Range(new Index(EndIndex), new Index(SRC.Length))).ToList().ToArray();
            string[] args = PreSrc[EndIndex - 1].Split(' ', 2)[1].Trim('%').Split(' ');
            PreSrc.RemoveAt(EndIndex - 1);
            if(args.Length != Args.Count)
            {
                Console.WriteLine("OK");
                // TODO Error length is not the same
            }

            for (int i = 0; i < Args.Count; i++)
            {
                for (int c = 0; c < args.Length; c++)
                {
                    char CharIdent = args[c].First();
                    if (CharIdent == '#')
                    {
                        if (Args[i].Contains('i'))
                        {
                            string[] instrs = (string[])Instrs.Clone();
                            for (int a = 0; a < instrs.Length; a++)
                            {
                                if (instrs[a].Contains(Args[i]))
                                {
                                    instrs[a] = instrs[a].Replace("%" + Args[i], args[c]).Trim('%');
                                    PreSrc.AddRange(instrs);
                                }
                            }
                            continue;
                        }
                    }
                    else
                    {
                        if (Args[i].Contains('r'))
                        {
                            string[] instrs = (string[])Instrs.Clone();
                            for (int a = 0; a < instrs.Length; a++)
                            {
                                if (instrs[a].Contains(Args[i]))
                                {
                                    instrs[a] = instrs[a].Replace("%" + Args[i], args[c]).Trim('%');
                                    PreSrc.AddRange(instrs);
                                }
                            }
                            continue;
                        }
                    }
                }
            }

            Args.AddRange(args);

            PreSrc.AddRange(Rest);

            SRC = PreSrc.ToArray();

            File.WriteAllLines(Program.CurrentDirectory + "/a.txt", PreSrc);
            File.WriteAllLines(Program.CurrentDirectory + "/b.txt", Rest);
            File.WriteAllLines(Program.CurrentDirectory + "/c.txt", Args);
            PC = 0;
        }
    }
}
