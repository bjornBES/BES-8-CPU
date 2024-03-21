using System.Diagnostics.Tracing;
using System.Reflection;

namespace assembler
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
                case TokenType.word:
                case TokenType.byte_:

                case TokenType.const_:
                case TokenType.global:
                case TokenType.local:
                    return ($"{type}").Trim('_');

                case TokenType.int_lit:
                    return "int literal";
                case TokenType.ident:
                    return "identifier";

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
            int line_count = 0;
            string File = "";
        void Tokenize()
        {
            string buf = "";
            while (peek() != '\0')
            {
                buf = "";
                if (peek() == '0' && peek(1) != '\0' && peek(1) == 'x')
                {
                    consume();
                    consume();
                    tokens.Add(new Token() { Type = TokenType.Hex_int_lit, Line = line_count, File = File });
                }
                else if (peek() == '0' && peek(1) != '\0' && peek(1) == 'b')
                {
                    consume();
                    consume();
                    tokens.Add(new Token() { Type = TokenType.Bin_int_lit, Line = line_count, File = File });
                }
                else if (char.IsLetter(peek()) && char.IsUpper(peek()) && char.IsAsciiHexDigit(peek()) && (
                    peek(1) != '\0' && peek(1) == 'h' ||
                    peek(2) != '\0' && peek(2) == 'h' ||
                    peek(3) != '\0' && peek(3) == 'h' ||
                    peek(4) != '\0' && peek(4) == 'h'
                    ))
                {
                    buf += consume();
                    while (peek() != '\0' && (char.IsAsciiHexDigit(peek()) || peek() == '_'))
                    {
                        if (peek() == '_') consume();
                        else
                        {
                            buf += consume();
                        }
                    }
                    tokens.Add(new Token()
                    {
                        Type = TokenType.int_lit,
                        Value = buf,
                        Line = line_count,
                        File = File
                    });
                }
                else if (peek() == '.' && peek(1) != '\0' && peek(1) == '/')
                {
                    while (peek() != Environment.NewLine[0])
                    {
                        buf += consume();
                    }
                    tokens.Add(new Token()
                    {
                        File = File,
                        Line = line_count,
                        Type = TokenType.path,
                        Value = buf
                    });
                }
                else if (peek() == 'R' && peek(1) != '\0' && char.IsDigit(peek(1)))
                {
                    consume();
                    buf = "R";
                    buf += consume();
                    CompareIdent(buf);
                }
                else if (char.IsLetter(peek()) || peek() == '_')
                {
                    buf += consume();
                    while (peek() != '\0' && (char.IsLetter(peek()) || peek() == '_'))
                    {
                        buf += consume();
                    }

                    CompareIdent(buf);
                }
                else if (char.IsAsciiHexDigit(peek()))
                {
                    buf += consume();
                    while (peek() != '\0' && (char.IsAsciiHexDigit(peek()) || peek() == '_'))
                    {
                        if (peek() == '_') consume();
                        else
                        {
                            buf += consume();
                        }
                    }
                    tokens.Add(new Token()
                    {
                        Type = TokenType.int_lit,
                        Value = buf,
                        Line = line_count,
                        File = File
                    });
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
                else if (peek() == '"')
                {
                    consume();
                    while (peek() != '"')
                    {
                        buf += consume();
                    }
                    consume();
                    tokens.Add(new() { Type = TokenType.quotation, Line = line_count, File = File });
                    tokens.Add(new() { Type = TokenType.ident, Value = buf, Line = line_count, File = File });
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
                else if (peek() == '$')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.doller, Line = line_count, File = File });
                }
                else if (peek() == '#')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.hash, Line = line_count, File = File });
                }
                else if (peek() == '_')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.Underscore, Line = line_count, File = File });
                }
                else if (peek() == '%')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.Percent, Line = line_count, File = File });
                }
                else if (peek() == '[')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.open_square, Line = line_count, File = File });
                }
                else if (peek() == ']')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.close_square, Line = line_count, File = File });
                }
                else if (peek() == '.')
                {
                    consume();
                    while (char.IsLetter(peek()))
                    {
                        buf += consume();
                    }
                    if (char.IsWhiteSpace(peek())) consume();
                    CompareIdent(buf);
                }
                else if (peek() == ':')
                {
                    consume();
                    tokens.Add(new() { Type = TokenType.colon, Line = line_count, File = File });
                }
                else if (peek() == ';')
                {
                    consume();
                    while (peek() != '\0' && peek() != Environment.NewLine[0])
                    {
                        consume();
                    }
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

        void CompareIdent(string buf)
        {
            switch (buf.ToLower())
            {
                case "const": tokens.Add(new Token() { Type = TokenType.const_, Line = line_count, File = File }); break;
                case "global": tokens.Add(new Token() { Type = TokenType.global, Line = line_count, File = File }); break;
                case "local": tokens.Add(new Token() { Type = TokenType.local, Line = line_count, File = File }); break;

                // types
                case "byte": tokens.Add(new Token() { Type = TokenType.byte_, Line = line_count, File = File }); break;
                case "db": tokens.Add(new Token() { Type = TokenType.byte_, Line = line_count, File = File }); break;
                case "word": tokens.Add(new Token() { Type = TokenType.word, Line = line_count, File = File }); break;
                case "dw": tokens.Add(new Token() { Type = TokenType.word, Line = line_count, File = File }); break;
                
                case "extern": tokens.Add(new Token() { Type = TokenType.extern_, Line = line_count, File = File }); break;
                case "bits": tokens.Add(new Token() { Type = TokenType.bits, Line = line_count, File = File }); break;
                case "section": tokens.Add(new Token() { Type = TokenType.section, Line = line_count, File = File }); break;
                case "repeat": tokens.Add(new Token() { Type = TokenType.repeat, Line = line_count, File = File }); break;
                case "org": tokens.Add(new Token() { Type = TokenType.org, Line = line_count, File = File }); break;
                case "str": tokens.Add(new Token() { Type = TokenType.str, Line = line_count, File = File }); break;
                case "strz": tokens.Add(new Token() { Type = TokenType.strz, Line = line_count, File = File }); break;
                case "include": tokens.Add(new Token() { Type = TokenType.include, Line = line_count, File = File }); break;

                // keywords

                case "newfile":
                    if (peek(1) != ':') return;
                    buf = "";
                    while (peek() != '\0' && !(char.IsWhiteSpace(peek()) || peek() == Environment.NewLine[0]))
                    {
                        buf += consume();
                    }
                    File = buf;
                    line_count = 0;
                    tokens.Add(new Token()
                    {
                        Type = TokenType.NewFile,
                        File = File,
                        Line = line_count,
                    }) ;
                    break;

                default: tokens.Add(new Token() { Type = TokenType.ident, Value = buf, Line = line_count, File = File }); break;
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
