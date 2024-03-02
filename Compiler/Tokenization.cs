using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Tokenization
    {
        public List<Token> tokens = new List<Token>();

        string m_src;
        int m_index;

        public static string to_string(TokenType type)
        {
            return $"`{ToString(type)}`";
        }
        public static string ToString(TokenType type)
        {
            switch (type)
            {
                case TokenType.exit:
                case TokenType.free:

                case TokenType.inc:

                case TokenType.func:
                case TokenType.ptr:
                case TokenType.let:
                case TokenType.word:
                case TokenType.class_:
                case TokenType.byte_:
                case TokenType.char_:
                case TokenType.int_:

                case TokenType.if_:
                case TokenType.elif:
                case TokenType.else_:

                case TokenType.const_:
                case TokenType.public_:
                case TokenType.global:
                case TokenType.local:
                case TokenType.return_:
                    return ($"{type}").Trim('_');

                case TokenType.int_lit:
                    return "int literal";
                case TokenType.ident:
                    return "identifier";

                case TokenType.port:
                    return ($"{type}").Trim('_');

                case TokenType.quotation: return "\"";
                case TokenType.apostrophe: return "\'";
                case TokenType.less_then: return "<";
                case TokenType.greater_then: return ">";
                case TokenType.ampersand: return "&";
                case TokenType.exclamation: return "!";
                case TokenType.comma: return ",";
                case TokenType.period: return ".";
                case TokenType.colon: return ":";
                case TokenType.semi: return ";";
                case TokenType.open_paren: return "(";
                case TokenType.close_paren: return ")";
                case TokenType.eq: return "=";
                case TokenType.plus: return "+";
                case TokenType.star: return "*";
                case TokenType.minus: return "-";
                case TokenType.fslash: return "/";
                case TokenType.bslash: return "\\";
                case TokenType.open_curly: return "{";
                case TokenType.close_curly: return "}";
                case TokenType.Hex_int_lit: return "0x";
                case TokenType.Bin_int_lit: return "0b";
            }
            return "NULL?";
        }

        public static int bin_prec(TokenType type)
        {
            switch (type)
            {
                case TokenType.minus:
                case TokenType.plus:
                    return 0;
                case TokenType.fslash:
                case TokenType.star:
                    return 1;
                default:
                    return -1;
            }
        }

        public void Build(string src)
        {
            m_src = src;
            Tokenize();
        }
        void Tokenize()
        {
            string File = "";
            string buf = "";
            int line_count = 0;
            while (peek() != '\0')
            {
                buf = "";
                if (peek() == '0' && peek(1) != '\0' && peek(1) == 'x')
                {
                    consume();
                    consume();
                    tokens.Add(new Token() { Type = TokenType.Hex_int_lit, Line = line_count , File = File });
                }
                if (peek() == '0' && peek(1) != '\0' && peek(1) == 'b')
                {
                    consume();
                    consume();
                    tokens.Add(new Token() { Type = TokenType.Bin_int_lit, Line = line_count , File = File });
                }
                else if (char.IsLetter(peek()))
                {
                    buf += consume();
                    while (peek() != '\0' && char.IsLetter(peek()))
                    {
                        buf += consume();
                    }

                    switch (buf.ToLower())
                    {
                        // func keywords
                        case "exit": tokens.Add(new Token() { Type = TokenType.exit, Line = line_count, File = File }); break;
                        case "free": tokens.Add(new Token() { Type = TokenType.free, Line = line_count, File = File }); break;
                        
                        case "const": tokens.Add(new Token() { Type = TokenType.const_, Line = line_count, File = File }); break;
                        case "global": tokens.Add(new Token() { Type = TokenType.global, Line = line_count , File = File }); break;
                        case "public": tokens.Add(new Token() { Type = TokenType.public_, Line = line_count , File = File }); break;
                        case "local": tokens.Add(new Token() { Type = TokenType.local, Line = line_count , File = File }); break;
                        
                        // types
                        case "byte": tokens.Add(new Token() { Type = TokenType.byte_, Line = line_count , File = File }); break;
                        case "word": tokens.Add(new Token() { Type = TokenType.word, Line = line_count , File = File }); break;
                        case "let": tokens.Add(new Token() { Type = TokenType.word, Line = line_count , File = File }); break;
                        case "int": tokens.Add(new Token() { Type = TokenType.int_, Line = line_count , File = File }); break;
                        case "ptr": tokens.Add(new Token() { Type = TokenType.ptr, Line = line_count , File = File }); break;

                        // keywords
                        case "return": tokens.Add(new Token() { Type = TokenType.return_, Line = line_count , File = File }); break;
                        case "func": tokens.Add(new Token() { Type = TokenType.func, Line = line_count , File = File }); break;
                        case "class": tokens.Add(new Token() { Type = TokenType.class_, Line = line_count , File = File }); break;
                        case "inc": tokens.Add(new Token() { Type = TokenType.inc, Line = line_count , File = File }); break;
                        case "if": tokens.Add(new Token() { Type = TokenType.if_, Line = line_count , File = File }); break;
                        case "elif": tokens.Add(new Token() { Type = TokenType.elif, Line = line_count , File = File }); break;
                        case "else": tokens.Add(new Token() { Type = TokenType.else_, Line = line_count , File = File }); break;

                        // func classes
                        case "port": tokens.Add(new Token() { Type = TokenType.port, Line = line_count , File = File }); break;
                        
                        case "file":
                            line_count--;
                            if (peek() == ':') consume();
                            else continue;
                            buf = "";
                            while (peek() != '\0' && !(char.IsWhiteSpace(peek()) || peek() == Environment.NewLine[0]))
                            {
                                buf += consume();
                            }
                            File = buf;
                            break;

                        default: tokens.Add(new Token() { Type = TokenType.ident, Value = buf, Line = line_count, File = File }); break;
                    }
                }
                else if (char.IsDigit(peek()))
                {
                    buf += consume();
                    while (peek() != '\0' && char.IsDigit(peek()))
                    {
                        buf += consume();
                    }
                    tokens.Add(new Token() { Type = TokenType.int_lit, Line = line_count, Value = buf , File = File });
                }
                else if (peek() == '/' && peek(1) != '\0' && peek(1) == '/')
                {
                    consume();
                    consume();
                    while (peek() != '\0' && peek() != '\n')
                    {
                        consume();
                    }
                }
                else if (peek() == '/' && peek(1) != '\0' && peek(1) == '*')
                {
                    consume();
                    consume();
                    while (peek() != '\0')
                    {
                        if (peek() == '*' && peek(1) != '\0' && peek(1) == '/')
                        {
                            break;
                        }
                        consume();
                    }
                    if (peek() != '\0')
                    {
                        consume();
                    }
                    if (peek() != '\0')
                    {
                        consume();
                    }
                }
                else if (peek() == '"')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.quotation, Line = line_count, File = File });
                }
                else if (peek() == '\'')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.apostrophe, Line = line_count, File = File });
                }
                else if (peek() == '<')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.less_then, Line = line_count, File = File });
                }
                else if (peek() == '>')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.greater_then, Line = line_count, File = File });
                }
                else if (peek() == '&')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.ampersand, Line = line_count, File = File });
                }
                else if (peek() == '!')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.exclamation, Line = line_count, File = File });
                }
                else if (peek() == ',')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.comma, Line = line_count, File = File });
                }
                else if (peek() == '.')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.period, Line = line_count, File = File });
                }
                else if (peek() == ':')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.colon, Line = line_count, File = File });
                }
                else if (peek() == ';')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.semi, Line = line_count, File = File });
                }
                else if (peek() == '(')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.open_paren, Line = line_count, File = File });
                }
                else if (peek() == ')')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.close_paren, Line = line_count, File = File });
                }
                else if (peek() == '=')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.eq, Line = line_count, File = File });
                }
                else if (peek() == '+')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.plus, Line = line_count, File = File });
                }
                else if (peek() == '*')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.star, Line = line_count, File = File });
                }
                else if (peek() == '-')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.minus, Line = line_count, File = File });
                }
                else if (peek() == '/')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.fslash, Line = line_count, File = File });
                }
                else if (peek() == '\\')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.bslash, Line = line_count, File = File });
                }
                else if (peek() == '{')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.open_curly, Line = line_count, File = File });
                }
                else if (peek() == '}')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.close_curly, Line = line_count, File = File });
                }
                else if (peek() == Environment.NewLine[0])
                {
                    consume();
                    line_count++;
                }
                else if (char.IsWhiteSpace(peek()))
                {
                    consume();
                }
                else
                {
                    Console.WriteLine("Invalid token " + peek());
                    Environment.Exit(1);
                }
            }
        }
        char peek(int offset = 0)
        {
            if (m_index + offset >= m_src.Length)
            {
                return '\0';
            }
            return m_src[m_index + offset];
        }

        char consume()
        {
            return m_src[m_index++];
        }
    }
}
