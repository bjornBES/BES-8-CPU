using System.Threading;

public enum TokenType
{
    none,
    NewFileToken,

    hex_ident,
    bin_ident,

    int_lit,
    ident,

    // keywords
    auto,
    break_,
    case_,
    char_,
    const_,
    continue_,
    default_,
    do_,
    double_,
    else_,
    enum_,
    extern_,
    float_,
    for_,
    goto_,
    if_,
    int_,
    long_,
    register,
    return_,
    short_,
    signed,
    sizeof_,
    static_,
    struct_,
    switch_,
    typeof_,
    typedef,
    union,
    unsigned,
    void_,
    volatile_,
    while_,

    // preprocessor directives
    define,
    include,
    ifdef,
    ifndef,
    elif,
    endif,
    defined,
    undef,
    error,
    warning,
    line,
    pragma,

    // pragma 
    once,

    // Special Symbols
    semi,
    comma,
    colon,
    bslash,
    right_arrow,
    left_arrow,
    ampersand,
    period,
    exclamation,

    // String Punctuation
    open_paren,
    close_paren,
    open_curly,
    close_curly,
    open_square,
    close_square,

    //Quotation Marks
    double_quotes,
    single_quotes,

    // Operators
    plus,
    star,
    minus,
    fslash,
    modulo,

    // inc/dec operators
    Increment,
    Decrement,

    // Assignment
    eq,
}
public static class DataSizes
{
    // in 24 bits
    // there is 2 bytes

    /// <summary>
    /// 1 word
    /// </summary>
    public const byte CHARSIZE = 1;

    /// <summary>
    /// 1 word
    /// </summary>
    public const byte SHORTSIZE = 1;
    
    /// <summary>
    /// 2 word
    /// </summary>
    public const byte INTSIZE = 2;
    /// <summary>
    /// 3 word
    /// </summary>
    public const byte LONGINTSIZE = 3;
    
    /// <summary>
    /// 2 words
    /// </summary>
    public const byte FLOATSIZE = 2;
    
    /// <summary>
    /// 3 word
    /// </summary>
    public const byte POINTERSIZE = 3;
}