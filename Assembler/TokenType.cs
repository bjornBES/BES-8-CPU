using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assembler
{
    public enum TokenType
    {
        int_lit,
        ident,
        path,

        byte_,
        word,

        extern_,
        bits,
        section,
        repeat,
        org,
        str,
        strz,
        NewFile,
        include,

        const_,
        global, // it is global beteen files
        local, // it is local to a file

        Hex_int_lit,
        Bin_int_lit,

        Underscore,
        hash,
        doller,
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
        open_square,
        close_square,
        eq,
        plus,
        star,
        minus,
        fslash,
        bslash,
        open_curly,
        close_curly,
        Percent,
    }
}
