using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic
{
    public class BasicInt
    {
        public string[] Asm = new string[0x8000 + 1];
        public required string DebugFile;

        string src = "";
        public void Build(string[] Src)
        {
            for (int i = 0; i < Src.Length; i++)
            {
                if (string.IsNullOrEmpty(Src[i])) continue;

                string Number = Src[i].Split(' ', 2)[0];
                string Instr = Src[i].Split(' ', 2)[1];
                string Args = "";
                if(Instr.Split(" ").Length > 1)
                {
                    Args = Instr.Split(" ", 2)[1];
                }
                Instr = Src[i].Split(' ', 3)[1];

                // decode expr
                string buf = "";
                if (Args.Split(' ').Length > 1)
                {
                    for (int a = 0; a < Args.Length; a++)
                    {
                        if (char.IsDigit(Args[a]))
                        {
                            buf += Args[a];
                        }
                        else if (Args[a] == '+')
                        {
                            buf += Args[a];
                        }
                        else if (Args[a] == '(')
                        {
                            buf += Args[a];
                        }
                        else if (Args[a] == ')')
                        {
                            buf += Args[a];
                        }
                        else if (Args[a] == '-')
                        {
                            buf += Args[a];
                        }
                        else if (Args[a] == '*')
                        {
                            buf += Args[a];
                        }
                        else if (Args[a] == '/')
                        {
                            buf += Args[a];
                        }
                        else if (char.IsWhiteSpace(Args[a])) continue;
                        else
                        {
                            break;
                        }
                        continue;
                    }
                }
                if (buf != "")
                {
                    src += buf;
                    int StartIndex = 0;
                    int ComIndex = 3;
                    Console.WriteLine("BUF "+ buf);
                    for (int a = StartIndex; a < buf.Length; a++)
                    {
                        if (ComIndex == 3 && buf[a] == '(')
                        {
                            StartIndex = a;
                            a = StartIndex + 1;
                            ComIndex = 2;
                        }
                        if(ComIndex == 2)
                        {
                            if (buf[a] == '*')
                            {
                                int Right = int.Parse(buf[a + 1].ToString());
                                int Left = int.Parse(buf[a - 1].ToString());
                                string Result = (Left * Right).ToString();
                                src += Result;
                                buf = buf.Remove(a, 1);
                                buf = buf.Remove(a + 1, 1);
                                buf = buf.Remove(a - 1, 1);
                                buf.Insert(a, Result);
                                a = 0;
                                StartIndex = 0;
                            }
                            if (buf[a] == '/')
                            {
                                int Right = int.Parse(buf[a + 1].ToString());
                                int Left = int.Parse(buf[a - 1].ToString());
                                string Result = (Left / Right).ToString();
                                src += Result;
                                buf = buf.Remove(a, 1);
                                buf = buf.Remove(a + 1, 1);
                                buf = buf.Remove(a - 1, 1);
                                buf.Insert(a, Result);
                                a = 0;
                                StartIndex = 0;
                            }
                        }
                        if (ComIndex == 1)
                        {
                            if (buf[a] == '-')
                            {
                                int Right = int.Parse(buf[a + 1].ToString());
                                int Left = int.Parse(buf[a - 1].ToString());
                                string Result = (Left - Right).ToString();
                                src += Result;
                                buf = buf.Remove(a, 1);
                                buf = buf.Remove(a + 1, 1);
                                buf = buf.Remove(a - 1, 1);
                                buf.Insert(a, Result);
                                a = 0;
                                StartIndex = 0;
                            }
                            if (buf[a] == '+')
                            {
                                int Right = int.Parse(buf[a + 1].ToString());
                                int Left = int.Parse(buf[a - 1].ToString());
                                string Result = (Left + Right).ToString();
                                src += Result;
                                buf = buf.Remove(a, 1);
                                buf = buf.Remove(a + 1, 1);
                                buf = buf.Remove(a - 1, 1);
                                buf.Insert(a, Result);
                                a = 0;
                                StartIndex = 0;
                            }
                        }
                        if (buf[a] == ')')
                        {
                            buf = buf.Remove(a, 1);
                            buf = buf.Remove(StartIndex, 1);
                            ComIndex = 3;
                            a = 0;
                            StartIndex = 0;
                        }

                        if (a == buf.Length - 1)
                        {
                            a = StartIndex;
                            ComIndex--;
                        }
                    }
                }

                if (Instr.ToUpper().StartsWith("HOME")) src +=      Number.PadLeft(4, '0') + ": CLS" + "\r\n";
                if (Instr.ToUpper().StartsWith("LET")) src +=       Number.PadLeft(4, '0') + ": VAR " + Args + "\r\n";
                if (Instr.ToUpper().StartsWith("PRINT")) src +=     Number.PadLeft(4, '0') + ": PRINT " + Args + "\r\n";
                if (Instr.ToUpper().StartsWith("GOSUB")) src +=     Number.PadLeft(4, '0') + ": CALL " + Args + "\r\n";
                if (Instr.ToUpper().StartsWith("GOTO")) src +=      Number.PadLeft(4, '0') + ": JMP " + Args + "\r\n";
                if (Instr.ToUpper().StartsWith("RETURN")) src +=    Number.PadLeft(4, '0') + ": RET " + "\r\n";
            }
            File.WriteAllText(DebugFile, src);
        }
    }
}
