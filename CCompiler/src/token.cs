public struct Token
{
    public TokenType Type;    
    public string value;
    public string File;
    public int line;
    public int column;

    public string FormatPath()
    {
        return $"{File}:{line}:{column}";
    }

    public int? BinPrec()
    {
        switch (Type)
        {
            case TokenType.star:
            case TokenType.fslash:
                return 2;
            case TokenType.plus:
            case TokenType.minus:
                return 1;
            default:
                return null;
        }
    }

    public override string ToString()
    {
        switch (Type)
        {
            case TokenType.int_lit:return "int literal";
            case TokenType.ident:return "identifier";

            case TokenType.return_:
            case TokenType.int_:
            case TokenType.char_:
            case TokenType.short_:
                return $"`{Type.ToString().TrimEnd('_')}`";
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
            default:
                return "NULL";
        }
    }
}