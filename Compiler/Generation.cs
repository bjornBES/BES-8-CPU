using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace Compiler
{
    public class Generation
    {
        public List<string> AsmSrc = new ();
        public List<Function> functions = new();
        public List<Variable> Variables = new();
        public List<string> Names = new();
        int m_index = 0;
        int level = 0;
        const uint VariabelAddrOffset = 0x20000;
        uint VariabelAddr = 0;
        List<Token> m_src = new ();
        public void Build(Tokenization tokenization)
        {
            AsmSrc.Add("; word");
            AsmSrc.Add("push #800h");
            AsmSrc.Add("; byte");
            AsmSrc.Add("push #0");
            AsmSrc.Add("call [_main]");
            AsmSrc.Add("add SP #2");
            AsmSrc.Add("");

            m_src = tokenization.tokens;

            while (peek() != null)
            {
                AsmSrc.Add("");
                //Console.WriteLine($"Token = {{{peek().Type}, {peek().Value} at line {peek().Line}}}\n");
                if (peek().Type == TokenType.func)
                {
                    FoFunc(consume());
                }
                else if (peek().Type == TokenType.word)
                {
                    consume();
                    if (peek().Type == TokenType.ident)
                    {
                        string Name = consume().Value;
                        if (peek().Type == TokenType.eq)
                        {
                            consume();
                            bool IsPointed = peek().Type == TokenType.star;
                            bool GetAddr = peek().Type == TokenType.ampersand && IsPointed != true;
                            if (IsPointed) consume();
                            if (GetAddr) consume();
                            if (peek().Type == TokenType.ident)
                            {
                                string ToValue = consume().Value;
                                AsmSrc.Add("; making a new variabel called " + Name + " = " + ToValue + " that is pointed " + IsPointed);
                                AsmSrc.Add("push MB");
                                AsmSrc.Add("mov MB #2");
                                if (level == 0)
                                {
                                    if (IsPointed)
                                    {
                                        AsmSrc.Add($"mov [{NewVariabel(Name, false)}] [{GetVariabel(ToValue)}]");
                                    }
                                    else if (GetAddr)
                                    {
                                        AsmSrc.Add($"mov [{NewVariabel(Name, false)}] #{GetVariabel(ToValue)}");
                                    }
                                    else
                                    {
                                        AsmSrc.Add($"mov [{NewVariabel(Name, false)}] [{GetVariabel(ToValue)}]");
                                    }
                                }
                                else
                                {
                                    if (IsPointed)
                                    {
                                        AsmSrc.Add($"mov [{NewLocalVariabel(Name, false)}] [{GetVariabel(ToValue)}]");
                                    }
                                    else if (GetAddr)
                                    {
                                        AsmSrc.Add($"mov [{NewLocalVariabel(Name, false)}] #{GetVariabel(ToValue)}");
                                    }
                                    else
                                    {
                                        AsmSrc.Add($"mov [{NewLocalVariabel(Name, false)}] [{GetVariabel(ToValue)}]");
                                    }
                                }
                                AsmSrc.Add("pop MB");
                            }
                        }
                        else if (peek().Type == TokenType.open_paren)
                        {
                            // func
                        }
                    }
                }
                else if (peek().Type == TokenType.ident)
                {
                    string name = consume().Value;
                    if(peek().Type == TokenType.open_paren)
                    {
                        consume();
                        Function function = GetFunction(name);

                        if(function == null)
                        {
                            Console.WriteLine("ERROR");
                            //TODO ERROR
                        }

                        if (function.arguments.Count == 0)
                        {
                            AsmSrc.Add(function.CallFunc());
                        }
                    }
                }
                else if (peek().Type == TokenType.close_curly)
                {
                    if (level == 1)
                    {
                        AsmSrc.Add("popr");
                        AsmSrc.Add("rts");
                    }
                    level--;
                    consume();
                }
                else if (peek().Type == TokenType.port)
                {
                    AsmSrc.Add("; calling the ports");
                    DoPorts();
                }
                else
                {
                    Console.WriteLine("NOPE ON " + consume().Type);
                }
                if(Console.KeyAvailable == true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape) break;
                }
            }
        }

        private void FoFunc(Token token)
        {
            if (peek().Type == TokenType.ident)
            {
                string Name = consume().Value;
                if(IsCheakNames(Name) == true) CompilerErrors.ErrorSameName("", peek(), Name);
                if (peek().Type == TokenType.open_paren)
                {
                    consume();

                    Argument[] args = new Argument[0];
                    if (peek().Type != TokenType.close_paren)
                    {
                        args = DoArgs();
                        Names.Add(Name);
                        AsmSrc.Add($"_{Name}:");
                        AsmSrc.Add($"pushr");
                        AsmSrc.Add($"mov ZX SP");
                        AsmSrc.Add($"add SP #8");
                        AsmSrc.Add("push MB");
                        AsmSrc.Add("mov MB #2");
                        for (int i = 0; i < args.Length; i++)
                        {
                            AsmSrc.Add($"; {i} {args[i].Name} as {args[i].type.Type} is pointer {args[i].IsPointer}");
                            uint addr = NewArgumentVariabel(args[i].Name, Name, args[i].IsPointer);
                            AsmSrc.Add($"pop [{addr}] ; {args[i].Name}");
                        }
                        AsmSrc.Add("pop MB");
                        AsmSrc.Add($"mov SP ZX");
                    }
                    else
                    {
                        Names.Add(Name);
                        AsmSrc.Add($"_{Name}:");
                        AsmSrc.Add($"pushr");
                    }
                    consume();
                    functions.Add(new Function()
                    {
                        arguments = args.ToList(),
                        Name = Name,
                    });

                    level++;
                    if (peek().Type != TokenType.open_curly)
                    {
                        CompilerErrors.SyntaxError("", peek(), TokenType.open_curly);
                        // TODO ERROR
                    }
                    consume();
                }
            }
        }

        private Function GetFunction(string name)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                if (functions[i].Name == name)
                {
                    return functions[i];
                }
            }
            return null;
        }

        private uint NewVariabel(string name, bool isPointer)
        {
            if (IsCheakNames(name)) CompilerErrors.ErrorSameName("", peek(), name);
            Names.Add(name);
            uint Addr = VariabelAddrOffset + VariabelAddr++;
            Variables.Add(new Variable()
            {
                Name = name,
                IsPointer = isPointer,
                Size = 1,
                Addr = Addr,
            });
            return Addr;
        }
        private uint NewLocalVariabel(string name, bool isPointer)
        {
            if (IsCheakNames(name)) CompilerErrors.ErrorSameName("", peek(), name);
            Names.Add(name);
            uint Addr = VariabelAddrOffset + 0x30000 + VariabelAddr++;
            Variables.Add(new Variable()
            {
                Name = name,
                IsLocal = true,
                IsPointer = isPointer,
                Size = 1,
                Addr = Addr,
            });
            return Addr;
        }
        private uint NewArgumentVariabel(string name, string funcName, bool isPointer)
        {
            if (IsCheakNames(name)) CompilerErrors.ErrorSameName("", peek(), name);
            Names.Add(name);
            uint Addr = VariabelAddrOffset + 0x40000 + VariabelAddr++;
            Variables.Add(new Variable()
            {
                Name = name,
                FuncName = funcName,
                IsArgument = true,
                IsLocal = true,
                IsPointer = isPointer,
                Size = 1,
                Addr = Addr,
            });
            return Addr;
        }

        public uint GetVariabel(string name)
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].Name == name)
                {
                    return Variables[i].Addr;
                }
            }
            return 0;
        }

        private void DoPorts()
        {
            consume();
            if(peek().Type == TokenType.period)
            {
                consume();
                if(peek().Type == TokenType.ident)
                {
                    Token token = consume();
                    switch (token.Value.ToLower())
                    {
                        case "cout":
                            if (peek().Type == TokenType.open_paren)
                            {
                                consume();
                                string value = Expected_Arg(TokenType.char_);
                                consume();

                                AsmSrc.Add($"; Calling Cout func with 1 arg");
                                AsmSrc.Add($"push #'{value}'");
                                AsmSrc.Add($"call [_cout]");
                                AsmSrc.Add($"add SP #1");
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private string Expected_Arg(TokenType Expected_Token)
        {
            string Result = "NULL";
            switch (Expected_Token)
            {
                case TokenType.byte_:
                    break;
                case TokenType.char_:
                    if (peek().Type == TokenType.apostrophe)
                    {
                        consume();
                        if(peek(1).Type == TokenType.apostrophe)
                        {
                            Result = consume().Value;
                            consume();
                        }
                    }
                    break;
                case TokenType.word:
                    break;
                case TokenType.let:
                    break;
            }

            return Result;
        }

        private Argument[] DoArgs()
        {
            List<Argument> arg = new List<Argument>();

            while (peek().Type != TokenType.close_paren)
            {
                Console.WriteLine($"Token = {{{peek().Type}, {peek().Value} at line {peek().Line}}}");
                switch (peek().Type)
                {
                    case TokenType.byte_:
                        consume();
                        if (peek().Type == TokenType.star)
                        {
                            consume();
                            //pointer
                            if (peek().Type == TokenType.ident)
                            {
                                arg.Add(new Argument()
                                {
                                    Name = consume().Value,
                                    IsPointer = true,
                                    type = new Token()
                                    {
                                        Type = TokenType.byte_
                                    }
                                });
                            }
                        }
                        else
                        {
                            // non pointer
                            if (peek().Type == TokenType.ident)
                            {
                                arg.Add(new Argument()
                                {
                                    Name = consume().Value,
                                    IsPointer = false,
                                    type = new Token()
                                    {
                                        Type = TokenType.byte_
                                    }
                                });
                            }
                        }
                        break;
                    case TokenType.word:
                        consume();
                        if(peek().Type == TokenType.star)
                        {
                            consume();
                            //pointer
                            if (peek().Type == TokenType.ident)
                            {
                                arg.Add(new Argument()
                                {
                                    Name = consume().Value,
                                    IsPointer = true,
                                    type = new Token()
                                    {
                                        Type = TokenType.word
                                    }
                                });
                            }
                        }
                        else
                        {
                            // non pointer
                            if (peek().Type == TokenType.ident)
                            {
                                arg.Add(new Argument()
                                {
                                    Name = consume().Value,
                                    IsPointer = false,
                                    type = new Token()
                                    {
                                        Type = TokenType.word
                                    }
                                });
                            }
                        }
                        break;
                    case TokenType.let:
                        break;
                        case TokenType.comma: consume(); break;
                    default:
                        Console.WriteLine("ERROR");
                        break;
                }
            }

            return arg.ToArray();
        }

        private bool IsCheakNames(string name)
        {
            if(!Names.Contains(name)) return false;
            return true;
        }

        Token peek(int offset = 0)
        {
            if (m_index + offset >= m_src.Count)
            {
                return null;
            }
            return m_src[m_index + offset];
        }

        Token consume()
        {
            return m_src[m_index++];
        }
    }
}
