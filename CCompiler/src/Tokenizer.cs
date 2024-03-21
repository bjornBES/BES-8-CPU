using System;
using System.Collections.Generic;

class Tokenizer
{
    private string m_src;
    private int m_index;

    public static string To_String(TokenType type)
    {
        switch (type)
        {
            case TokenType.int_lit: return "int literal";
            case TokenType.ident: return "identifier";

            case TokenType.auto:
            case TokenType.break_:
            case TokenType.char_:
            case TokenType.case_:
            case TokenType.const_:
            case TokenType.continue_:
            case TokenType.default_:
            case TokenType.do_:
            case TokenType.double_:
            case TokenType.else_:
            case TokenType.enum_:
            case TokenType.extern_:
            case TokenType.float_:
            case TokenType.for_:
            case TokenType.goto_:
            case TokenType.int_:
            case TokenType.if_:
            case TokenType.long_:
            case TokenType.return_:
            case TokenType.register:
            case TokenType.short_:
            case TokenType.signed:
            case TokenType.sizeof_:
            case TokenType.static_:
            case TokenType.struct_:
            case TokenType.switch_:
            case TokenType.typeof_:
            case TokenType.typedef:
            case TokenType.union:
            case TokenType.unsigned:
            case TokenType.void_:
            case TokenType.volatile_:
            case TokenType.while_:
            case TokenType.define:
            case TokenType.include:
            case TokenType.ifdef:
            case TokenType.ifndef:
            case TokenType.endif:
            case TokenType.defined:
            case TokenType.undef:
            case TokenType.error:
            case TokenType.warning:
            case TokenType.line:
            case TokenType.pragma:
            case TokenType.once:
                return $"`{type.ToString().TrimEnd('_')}`";

            case TokenType.semi: return "`;`";
            case TokenType.open_paren: return "`(`";
            case TokenType.close_paren: return "`)`";
            case TokenType.open_curly: return "`{`";
            case TokenType.close_curly: return "`}`";
            case TokenType.plus: return "`+`";
            case TokenType.star: return "`*`";
            case TokenType.minus: return "`-`";
            case TokenType.fslash: return "`/`";
            case TokenType.bslash: return "`\\`";
            case TokenType.eq: return "`=`";
            case TokenType.comma: return "`,`";
            case TokenType.hex_ident:return "`0x`";
            case TokenType.bin_ident:return "`0b`";
            case TokenType.right_arrow:return "`>`";
            case TokenType.left_arrow:return "`<`";
            case TokenType.open_square:return "`[`";
            case TokenType.close_square:return "`]`";
            case TokenType.modulo:return "`%`";
            case TokenType.Increment:return "`++`";
            case TokenType.Decrement:return "`--`";
            default:
                return "NULL";
        }
    }

    int column_count = 1;
        List<Token> tokens = new List<Token>();
    public Token[] tokenize(string src = "")
    {
        string fileName = "";
        m_src = src;
        string buf = "";
        int line_count = 1;

        while (peek().HasValue)
        {
            buf = "";
            if (peek() == '0' && peek(1).HasValue && peek(1) == 'x')
            {
                consume();
                consume();
                tokens.Add(new Token() { Type = TokenType.hex_ident, line = line_count, column = column_count, File = fileName});
            }
            else if (peek() == '0' && peek(1).HasValue && peek(1) == 'b')
            {
                consume();
                consume();
                tokens.Add(new Token() { Type = TokenType.bin_ident , line = line_count, column = column_count, File = fileName });
            }
            else if (peek() == '+' && peek(1).HasValue && peek(1) == '+')
            {
                consume();
                consume();
                tokens.Add(new Token() { Type = TokenType.Increment, line = line_count, column = column_count, File = fileName });
            }
            else if (peek() == '-' && peek(1).HasValue && peek(1) == '-')
            {
                consume();
                consume();
                tokens.Add(new Token() { Type = TokenType.Decrement, line = line_count, column = column_count, File = fileName });
            }
            else if (char.IsLetter(peek().Value))
            {
                while (peek().HasValue && (char.IsLetterOrDigit(peek().Value) || peek() == '_'))
                {
                    buf += consume();
                }
                switch (buf)
                {
                    case "auto":        tokens.Add(new Token() { Type = TokenType.auto,         line = line_count, column = column_count, File = fileName, });break;
                    case "break":       tokens.Add(new Token() { Type = TokenType.break_,       line = line_count, column = column_count, File = fileName, });break;
                    case "case":        tokens.Add(new Token() { Type = TokenType.case_,        line = line_count, column = column_count, File = fileName, });break;
                    case "char":        tokens.Add(new Token() { Type = TokenType.char_,        line = line_count, column = column_count, File = fileName, });break;
                    case "const":       tokens.Add(new Token() { Type = TokenType.const_,       line = line_count, column = column_count, File = fileName, });break;
                    case "continue":    tokens.Add(new Token() { Type = TokenType.continue_,    line = line_count, column = column_count, File = fileName, });break;
                    case "default":     tokens.Add(new Token() { Type = TokenType.default_,     line = line_count, column = column_count, File = fileName, });break;
                    case "do":          tokens.Add(new Token() { Type = TokenType.do_,          line = line_count, column = column_count, File = fileName, });break;
                    case "double":      tokens.Add(new Token() { Type = TokenType.double_,      line = line_count, column = column_count, File = fileName, });break;
                    case "else":        tokens.Add(new Token() { Type = TokenType.else_,        line = line_count, column = column_count, File = fileName, });break;
                    case "enum":        tokens.Add(new Token() { Type = TokenType.enum_,        line = line_count, column = column_count, File = fileName, });break;
                    case "extern":      tokens.Add(new Token() { Type = TokenType.extern_,      line = line_count, column = column_count, File = fileName, });break;
                    case "float":       tokens.Add(new Token() { Type = TokenType.float_,       line = line_count, column = column_count, File = fileName, });break;
                    case "for":         tokens.Add(new Token() { Type = TokenType.for_,         line = line_count, column = column_count, File = fileName, });break;
                    case "goto":        tokens.Add(new Token() { Type = TokenType.goto_,        line = line_count, column = column_count, File = fileName, });break;
                    case "if":          tokens.Add(new Token() { Type = TokenType.if_,          line = line_count, column = column_count, File = fileName, });break;
                    case "int":         tokens.Add(new Token() { Type = TokenType.int_,         line = line_count, column = column_count, File = fileName, });break;
                    case "long":        tokens.Add(new Token() { Type = TokenType.long_,        line = line_count, column = column_count, File = fileName, });break;
                    case "register":    tokens.Add(new Token() { Type = TokenType.register,     line = line_count, column = column_count, File = fileName, });break;
                    case "return":      tokens.Add(new Token() { Type = TokenType.return_,      line = line_count, column = column_count, File = fileName, });break;
                    case "short":       tokens.Add(new Token() { Type = TokenType.short_,       line = line_count, column = column_count, File = fileName, });break;
                    case "signed":      tokens.Add(new Token() { Type = TokenType.signed,       line = line_count, column = column_count, File = fileName, });break;
                    case "sizeof":      tokens.Add(new Token() { Type = TokenType.sizeof_,      line = line_count, column = column_count, File = fileName, });break;
                    case "static":      tokens.Add(new Token() { Type = TokenType.static_,      line = line_count, column = column_count, File = fileName, });break;
                    case "struct":      tokens.Add(new Token() { Type = TokenType.struct_,      line = line_count, column = column_count, File = fileName, });break;
                    case "switch":      tokens.Add(new Token() { Type = TokenType.switch_,      line = line_count, column = column_count, File = fileName, });break;
                    case "typeof":      tokens.Add(new Token() { Type = TokenType.typeof_,      line = line_count, column = column_count, File = fileName, });break;
                    case "typedef":     tokens.Add(new Token() { Type = TokenType.typedef,      line = line_count, column = column_count, File = fileName, });break;
                    case "union":       tokens.Add(new Token() { Type = TokenType.union,        line = line_count, column = column_count, File = fileName, });break;
                    case "unsigned":    tokens.Add(new Token() { Type = TokenType.unsigned,     line = line_count, column = column_count, File = fileName, });break;
                    case "void":        tokens.Add(new Token() { Type = TokenType.void_,        line = line_count, column = column_count, File = fileName, });break;
                    case "volatile":    tokens.Add(new Token() { Type = TokenType.volatile_,    line = line_count, column = column_count, File = fileName, });break;
                    case "while":       tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    
                    case "define":      tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "ifdef":       tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "ifndef":      tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "elif":        tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "endif":       tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "defined":     tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "undef":       tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "error":       tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "warning":     tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "line":        tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    case "pargma":      tokens.Add(new Token() { Type = TokenType.while_,       line = line_count, column = column_count, File = fileName, });break;
                    
                    case "once":        tokens.Add(new Token() { Type = TokenType.once,         line = line_count, column = column_count, File = fileName, });break;

                    case "filename_TYPE":
                        buf = "";
                        while (peek().HasValue && peek().Value != '\n')
                        {
                            buf += consume();
                        }
                        fileName = buf;
                        tokens.Add(new Token() { Type = TokenType.NewFileToken, line = line_count, column = column_count, File = fileName, value = buf, });
                        break;

                    default:            tokens.Add(new Token() { Type = TokenType.ident,        line = line_count, column = column_count, File = fileName, value = buf});break;
                }
            }
            else if (char.IsDigit(peek().Value))
            {
                while (peek().HasValue && char.IsDigit(peek().Value))
                {
                    buf += consume();
                }
                tokens.Add(new Token()
                {
                    Type = TokenType.int_lit,
                    line = line_count, column = column_count, File = fileName,
                    value = buf,
                });
            }
            else if (peek() == '/' && peek(1).HasValue && peek(1) == '/')
            {
                consume();
                consume();
                while (peek().HasValue && peek() != '\n')
                {
                    consume();
                }
            }
            else if (peek() == '/' && peek(1).HasValue && peek(1) == '*')
            {
                consume();
                consume();
                while (peek().HasValue && !(peek() == '*' && peek(1).HasValue && peek(1) == '/'))
                {
                    consume();
                }
                consume();
                consume();
            }
            else if (char.IsSeparator(peek().Value))
            {
                consume();
            }
            else
            {
                switch (peek())
                {
                    case ';':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.semi, line = line_count, column = column_count, File = fileName, });
                        break;
                    case ',':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.comma, line = line_count, column = column_count, File = fileName, });
                        break;
                    case ':':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.colon, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '\\':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.bslash, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '>':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.right_arrow, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '<':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.left_arrow, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '&':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.ampersand, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '.':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.period, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '!':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.exclamation, line = line_count, column = column_count, File = fileName, });
                        break;

                    case '(':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.open_paren, line = line_count, column = column_count, File = fileName, });
                        break;
                    case ')':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.close_paren, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '{':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.open_curly, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '}':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.close_curly, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '[':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.open_square, line = line_count, column = column_count, File = fileName, });
                        break;
                    case ']':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.close_square, line = line_count, column = column_count, File = fileName, });
                        break;

                    case '"':
                        consume();
                        while (peek().HasValue && peek() != '"')
                        {
                            buf += consume();
                        }
                        consume();
                        tokens.Add(new Token() { Type = TokenType.double_quotes, line = line_count, column = column_count, File = fileName, });
                        tokens.Add(new Token() { Type = TokenType.ident, line = line_count, column = column_count, File = fileName, value = buf });
                        tokens.Add(new Token() { Type = TokenType.double_quotes, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '\'':
                        consume();
                        buf += consume();
                        if(peek() != '\'')
                        {
                            Environment.Exit(-1);
                        }
                        consume();
                        tokens.Add(new Token() { Type = TokenType.single_quotes, line = line_count, column = column_count, File = fileName, });
                        tokens.Add(new Token() { Type = TokenType.ident, line = line_count, column = column_count, File = fileName, value = buf });
                        tokens.Add(new Token() { Type = TokenType.single_quotes, line = line_count, column = column_count, File = fileName, });
                        break;

                    case '+':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.plus, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '*':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.star, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '-':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.minus, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '/':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.fslash, line = line_count, column = column_count, File = fileName, });
                        break;
                    case '%':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.modulo, line = line_count, column = column_count, File = fileName, });
                        break;

                    case '=':
                        consume();
                        tokens.Add(new Token() { Type = TokenType.eq, line = line_count, column = column_count, File = fileName, });
                        break;

                    case '\n':
                        column_count = 0;
                        line_count++;
                        consume();
                        break;

                    default:
                        Console.WriteLine("Invalid Token " + peek());
                        Environment.Exit(-1);
                        break;
                }
            }
        }

        return tokens.ToArray();
    }

    private char? peek(int offset = 0)
    {
        if (offset + m_index >= m_src.Length)
        {
            return null;
        }
        return m_src[m_index + offset];
    }

    private char consume()
    {
        column_count++;
        return m_src[m_index++];
    }
}