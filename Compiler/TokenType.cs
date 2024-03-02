namespace Compiler
{
    public enum TokenType
    {
        int_lit,
        ident,

        port,

        func,
        ptr,
        byte_,
        char_,
        word,
        int_,
        let,

        return_,
        class_,
        inc,
        if_,
        elif,
        else_,

        const_,
        global, // it is global beteen files
        public_, // it is public beteen calsses
        local, // it is local to a func

        exit,
        free,

        Hex_int_lit,
        Bin_int_lit,

        quotation,
        apostrophe,
        greater_then,
        less_then,
        ampersand,
        exclamation,
        comma,
        period,
        colon,
        semi,
        open_paren,
        close_paren,
        eq,
        plus,
        star,
        minus,
        fslash,
        bslash,
        open_curly,
        close_curly,
    }
}