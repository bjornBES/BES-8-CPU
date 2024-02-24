using System.Reflection;

namespace Compiler
{
    public class Generation
    {

        int m_index = 0;
        Token[] m_src;

        bool IsGlobal = false;
        bool IsPointer = false;
        bool Call_Func = false;
        int Level = 0;

        public List<Variable> Variables = new List<Variable>();
        public List<string> Assembly_Src = new List<string>();

        const string Variable_Word = "Var_";

        bool Debug = true;
        string Func_Name = "";
        const uint ArgumentVariableOffset = 0x11000;
        const uint VariableOffset = 0x12000;
        uint ArgumentVariable = 0;
        uint VariableCount;
        uint RegisterCount;
        uint ArgumentCount;

        public void Build(Tokenization tokenization)
        {
            m_src = tokenization.tokens.ToArray();
            gen();
        }
        void gen()
        {
            Assembly_Src.Add("push #30");
            Assembly_Src.Add("push #20");
            Assembly_Src.Add("push #10");
            Assembly_Src.Add("call [Func_main]");
            Assembly_Src.Add("add SP, #3");
            while (peek() != null)
            {
                switch (peek().Type)
                {
                    case TokenType.func:
                        consume();
                        if (peek().Type == TokenType.ident)
                        {
                            if (peek(1).Type == TokenType.open_paren)
                            {
                                gen_Func(TokenType.func);
                            }
                        }
                        break;
                    case TokenType.ptr:
                        consume();
                        IsPointer = true;
                        break;
                    case TokenType.global:
                        consume();
                        IsGlobal = true;
                        break;
                    case TokenType.word:
                        consume();
                        if (peek().Type == TokenType.ident)
                        {
                            if (peek(1).Type == TokenType.open_paren)
                            {
                                gen_Func(TokenType.word);
                            }
                            else if (peek(1).Type == TokenType.eq)
                            {
                                genAssingVariable(TokenType.word, true);
                            }
                        }
                        break;

                    case TokenType.return_:
                        consume();
                        if (peek().Type == TokenType.ident)
                        {
                            string VariableName = GetVariableName(consume().Value);
                            if (isVariable(VariableName) == true)
                            {
                                Variable variable = get_Variable(VariableName);
                                Assembly_Src.Add($"");
                                Assembly_Src.Add($"mov R{RegisterCount}, [{variable.Address}]");
                                RegisterCount++;
                            }
                        }
                        break;

                    case TokenType.ident:
                        if (isVariable(GetVariableName(peek().Value)) == true)
                        {
                            string VariableName = GetVariableName(peek().Value);
                            Variable variable = get_Variable(VariableName);
                            if (peek(1).Type == TokenType.eq)
                            {
                                genAssingVariable(TokenType.ident, true);
                            }
                            else
                            {
                                goto default;
                            }
                        }
                        else if (peek(1).Type == TokenType.open_paren)
                        {
                            gen_CallFunc();
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case TokenType.open_curly:
                        Level++;
                        consume();
                        break;
                    case TokenType.close_curly:
                        if (Level == 1)
                        {
                            Variable[] variables = get_Local_Variables(Func_Name);

                            for (int i = 0; i < variables.Length; i++)
                            {
                                gen_FreeVariable(variables[i], true);
                            }

                            Level--;
                            Assembly_Src.Add($"{Environment.NewLine}; close_curly");
                            Assembly_Src.Add("popr");
                            if(RegisterCount > 0)
                            {
                                RegisterCount--;
                                Assembly_Src.Add($"mov ZX R{RegisterCount}");
                            }
                            Assembly_Src.Add($"ret #{ArgumentCount}");
                        }
                        Call_Func = false;
                        consume();
                        break;

                    case TokenType.exit:
                        consume();
                        gen_Exit();
                        break;
                    case TokenType.free:
                        consume();
                        gen_Free();
                        break;
                    default:
                        Console.WriteLine($"skip {Tokenization.to_string(consume().Type)}");
                        break;
                }
            }
        }

        void gen_Func(TokenType ReturnType)
        {
            ArgumentCount = 0;
            if (peek().Type == TokenType.ident)
            {
                string FuncName = Parse_Expr(out _);
                Func_Name = FuncName;
                if (peek().Type == TokenType.open_paren)
                {
                    consume();
                    Assembly_Src.Add($"{Environment.NewLine}; func return type is {Tokenization.to_string(ReturnType)}");
                    Assembly_Src.Add($"Func_{FuncName}:");
                    Assembly_Src.Add($"pushr");
                }
                if (peek().Type != TokenType.close_paren)
                {
                    Assembly_Src.Add("mov ZX, SP");
                    Assembly_Src.Add("add SP, #8");
                    while (peek().Type != TokenType.close_paren)
                    {
                        string name;
                        switch (peek().Type)
                        {
                            case TokenType.char_:
                            case TokenType.byte_:
                            case TokenType.word:
                            case TokenType.let:
                            case TokenType.int_:
                                TokenType tokenType = consume().Type;
                                if (peek().Type != TokenType.ident)
                                {
                                    Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                                    Environment.Exit(1);
                                }
                                name = Parse_Expr(out _);
                                gen_Local_Variable(name, FuncName, tokenType, true);
                                if (peek().Type == TokenType.close_paren) break;
                                if (peek().Type != TokenType.comma)
                                {
                                    Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                                    Environment.Exit(1);
                                }
                                consume();
                                break;
                            case TokenType.ptr:
                                break;
                            default:
                                break;
                        }
                        ArgumentCount++;
                    }
                    Assembly_Src.Add("mov SP, ZX");
                }
                if (peek().Type != TokenType.close_paren)
                {
                    Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                    Environment.Exit(1);
                }
                consume();
            }
        }

        void gen_CallFunc()
        {
            Call_Func = true;
            string funcName = consume().Value;
            CallFunc(funcName);
        }
        void gen_CallFunc(string funcName)
        {
            CallFunc(funcName);
        }

        void CallFunc(string funcname)
        {
            List<string> args = new List<string>();
            if (peek().Type != TokenType.open_paren)
            {
                Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                Environment.Exit(1);
            }
            consume();
            while (peek().Type != TokenType.close_paren)
            {
                if (peek().Type == TokenType.int_lit)
                {
                    string value = "#" + consume().Value;
                    args.Add(value);
                }

                if (peek().Type == TokenType.close_paren) break;

                if (peek().Type != TokenType.comma)
                {
                    Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                    Environment.Exit(1);
                }
                consume();
            }

            consume();

            for (int i = args.Count - 1; i > -1; i--)
            {
                Assembly_Src.Add($"push {args[i]}");
            }

            Assembly_Src.Add($"call [Func_{funcname}]");
        }

        void gen_Exit()
        {
            if (peek().Type != TokenType.open_paren)
            {
                Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                Environment.Exit(1);
            }

            Assembly_Src.Add($"{Environment.NewLine}; Exit");

            consume(); // open_paren

            if (peek().Type == TokenType.int_lit)
            {
                string Exit_Value = Parse_Expr(out _);
                if (peek().Type != TokenType.close_paren)
                {
                    Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                    Environment.Exit(1);
                }
                consume();
                Assembly_Src.Add($"mov AX, #{Exit_Value}");
                Assembly_Src.Add($"int #0");
            }
            else if (peek().Type == TokenType.ident)
            {
                if (isVariable(GetVariableName(peek().Value)) == true)
                {
                    string VariableName = GetVariableName(consume().Value);
                    Variable variable = get_Variable(VariableName);
                    if (peek().Type != TokenType.close_paren)
                    {
                        Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                        Environment.Exit(1);
                    }
                    consume();
                    Assembly_Src.Add($"mov AX, [{variable.Address}]");
                    Assembly_Src.Add($"int #0");
                }
            }
        }

        void gen_Free()
        {
            if (peek().Type != TokenType.open_paren)
            {
                Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                Environment.Exit(1);
            }

            consume();
            if (peek().Type == TokenType.ampersand)
            {
                consume();
                string Name = consume().Value;
                Variable variable = get_Variable(GetVariableName(Name));
                if (variable.IsPtr == true)
                {
                    Console.WriteLine($"ERORR {TokenType.ampersand} at line {peek().Line}");
                    Console.WriteLine($"{variable.Name} is a Pointer");
                    Environment.Exit(1);
                }
                gen_FreeVariable(variable);
            }
            else
            {
                string Name = consume().Value;
                Variable variable = get_Variable(GetVariableName(Name));
                if (variable.IsPtr != true)
                {
                    Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)} at line {peek().Line}");
                    Console.WriteLine($"{variable.Name} is not a Pointer");
                    Environment.Exit(1);
                }
                gen_FreeVariable(variable);
            }

            if (peek().Type != TokenType.close_paren)
            {
                Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                Environment.Exit(1);
            }
            consume();
        }

        void gen_FreeVariable(Variable variable, bool force = false)
        {
            if (force)
            {
                Assembly_Src.Add("");
                Assembly_Src.Add($"mov {variable.Address}, #0");
                Variables.Remove(variable);
                VariableCount--;
            }
            else
            {
                if (variable.IsProtected == false)
                {
                    Assembly_Src.Add("");
                    Assembly_Src.Add($"mov {variable.Address}, #0");
                    Variables.Remove(variable);
                }
                VariableCount--;
            }
        }

        void genAssingVariable(TokenType type, bool Local)
        {
            string Name = Parse_Expr(out _);
            if (peek().Type == TokenType.eq)
            {
                consume();
                string value = Parse_Expr(out Token Token);
                if (value != "" && type == TokenType.ident)
                {
                    if (Level > 0)
                    {
                        if (value.StartsWith('[') && value.EndsWith(']') && Token.Type == TokenType.ident)
                        {
                            Assembly_Src.Add($"mov {Name}, {value}");
                        }
                        else if (Token.Type == TokenType.int_lit)
                        {
                            Assembly_Src.Add($"mov [{Name}], #{value}");
                        }
                    }
                }
                else if (Local && type != TokenType.ident)
                {
                    bool pointer = (peek(-5) != null && peek(-5).Type == TokenType.ptr) || IsPointer;
                    if (peek(0).Type == TokenType.open_paren)
                    {
                        gen_CallFunc(value);
                        Assembly_Src.Add($"push ZX");
                        if (Level > 0)
                        {
                            gen_Local_Variable(Name, Func_Name, type, false);
                        }
                        else
                        {
                            if(IsGlobal == false)
                            {
                                gen_public_Variable(Name, type, pointer);
                            }
                            else
                            {
                                gen_global_Variable(Name, type, pointer);
                                IsGlobal = false;
                            }
                        }
                    }
                    else if (Token.Type == TokenType.int_lit)
                    {
                        if (Level > 0)
                        {
                            gen_Local_Variable(Name, Func_Name, type, false, value);
                        }
                        else
                        {
                            if (IsGlobal == false)
                            {
                                gen_public_Variable(Name, type, pointer, value);
                            }
                            else
                            {
                                gen_global_Variable(Name, type, pointer, value);
                                IsGlobal = false;
                            }
                        }
                    }
                    else
                    {
                        if (Level > 0)
                        {
                            gen_Local_Variable(Name, Func_Name, type, false);
                        }
                        else
                        {
                            if (IsGlobal == false)
                            {
                                gen_public_Variable(Name, type, pointer);
                            }
                            else
                            {
                                gen_global_Variable(Name, type, pointer);
                                IsGlobal = false;
                            }
                        }
                    }
                }
                else
                {
                    //
                }
            }
        }

        string Parse_Expr(out Token token)
        {
            switch (peek().Type)
            {
                case TokenType.ident:
                    if (isVariable(GetVariableName(peek().Value)))
                    {
                        token = peek();
                        return $"[{Convert.ToString(get_Variable(GetVariableName(consume().Value)).Address)}]";
                    }
                    else if (!isVariable(GetVariableName(peek().Value)))
                    {
                        token = peek();
                        return consume().Value;
                    }
                    token = peek();
                    return "";
                case TokenType.int_lit:
                    token = peek();
                    return consume().Value;
                case TokenType.Bin_int_lit:
                    consume();
                    if (peek().Type != TokenType.int_lit)
                    {

                    }
                    token = peek();
                    return Convert.ToString(Convert.ToUInt32(consume().Value, 2));
                case TokenType.Hex_int_lit:
                    consume();
                    if (peek().Type != TokenType.int_lit)
                    {

                    }
                    token = peek();
                    return Convert.ToString(Convert.ToUInt32(consume().Value, 16));
                case TokenType.open_curly:
                    CompileExpression(Debug);
                    token = peek();
                    return "";
                default:
                    token = peek();
                    return "";
            }
        }

        bool isVariable(string name)
        {
            return get_VariableNoErrors(name) != null;
        }

        Variable get_Variable(string name)
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].Name == name)
                {
                    return Variables[i];
                }
            }

            Console.WriteLine($"ERORR can't find variable {name} at line {peek().Line}");
            Environment.Exit(1);
            return null;
        }
        Variable get_VariableNoErrors(string name)
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].Name == name)
                {
                    return Variables[i];
                }
            }

            return null;
        }
        Variable[] get_Local_Variables(string funcName)
        {
            List<Variable> vars = new();
            for (int i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].FuncName == funcName)
                {
                    vars.Add(Variables[i]);
                }
            }
            if (vars.Count == 0)
            {
                Console.WriteLine($"ERORR can't find variables in {funcName}");
                Environment.Exit(1);
                return null;
            }
            return vars.ToArray();
        }

        public void gen_global_Variable(string name, TokenType tokenType, bool Pointer, string value = "")
        {
            uint VariableAddress = VariableOffset + VariableCount;
            VariableCount++;
            Assembly_Src.Add($"");
            Assembly_Src.Add($"; global type Var_{name} = [SP]");
            if (value == "")
            {
                Assembly_Src.Add($"inc SP");
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], [SP]");
            }
            else
            {
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], #{value}");
            }

            Variables.Add(new Variable()
            {
                Name = GetVariableName(name),
                Address = VariableAddress,
                IsLocal = false,
                IsPublic = false,
                IsGlobal = true,
                Size = GetSize(tokenType),
                IsProtected = false,
                IsPtr = Pointer,
            });
        }

        public void gen_public_Variable(string name, TokenType tokenType, bool Pointer, string value = "")
        {
            uint VariableAddress = VariableOffset + VariableCount;
            VariableCount++;
            Assembly_Src.Add($"");
            Assembly_Src.Add($"; public type Var_{name} = [SP]");
            if (value == "")
            {
                Assembly_Src.Add($"inc SP");
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], [SP]");
            }
            else
            {
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], #{value}");
            }

            Variables.Add(new Variable()
            {
                Name = GetVariableName(name),
                Address = VariableAddress,
                IsLocal = false,
                IsPublic = true,
                IsGlobal = false,
                Size = GetSize(tokenType),
                IsProtected = false,
                IsPtr = Pointer,
            });
        }
        public void gen_Local_Variable(string name, string func_name, TokenType tokenType, bool Args, string value = "")
        {
            uint VariableAddress;
            if (Args)
            {
                VariableAddress = ArgumentVariableOffset + ArgumentVariable;
                ArgumentVariable++;
            }
            else
            {
                VariableAddress = VariableOffset + VariableCount;
                VariableCount++;
            }
                Assembly_Src.Add($"");
                Assembly_Src.Add($"; local type Var_{name} = [SP]");
            if (value == "")
            {
                Assembly_Src.Add($"inc SP");
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], [SP]");
            }
            else
            {
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], #{value}");
            }

            Variables.Add(new Variable()
            {
                Name = GetVariableName(name),
                Address = VariableAddress,
                IsLocal = true,
                IsPublic = false,
                IsGlobal = false,
                Size = GetSize(tokenType),
                FuncName = func_name,
                IsProtected = Args,
            });
        }

        uint GetSize(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.ptr:
                    return 1;
                case TokenType.byte_:
                    return 1;
                case TokenType.word:
                    return 1;
                case TokenType.int_:
                    return 1;
                case TokenType.let:
                    return 1;
                default:
                    return 0x8000;
            }
        }

        void CompileExpression(bool debug = false)
        {
            const string ExpressionRegister = "ZX";
            const string Register = "AX";
            const string AtSP = "[SP]";

            if (peek().Type != TokenType.open_curly)
            {
                Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                Environment.Exit(1);
            }
            consume();

            Assembly_Src.Add("");
            Assembly_Src.Add("push ZX");
            Assembly_Src.Add("push AX");
            Assembly_Src.Add("push #0 ; result buffer");

            while (peek().Type != TokenType.close_curly)
            {
                Token token = peek();
                switch (token.Type)
                {
                    case TokenType.int_lit:
                    case TokenType.Hex_int_lit:
                    case TokenType.Bin_int_lit:
                        Assembly_Src.Add($"push #{Parse_Expr(out _)}");
                        break;
                    case TokenType.ident:
                        Assembly_Src.Add($"push {Parse_Expr(out _)}");
                        break;
                    case TokenType.open_paren:
                        break;
                    case TokenType.close_paren:
                        break;
                    case TokenType.plus:
                        consume();
                        if (peek().Type == TokenType.ident)
                        {
                            Assembly_Src.Add($"push {Parse_Expr(out _)}");
                        }
                        else
                        {
                            Assembly_Src.Add($"push #{Parse_Expr(out _)}");
                        }

                        Assembly_Src.Add($"pop {Register}");
                        Assembly_Src.Add($"pop {ExpressionRegister}");
                        Assembly_Src.Add($"add {ExpressionRegister}, {Register}");
                        Assembly_Src.Add($"add {AtSP}, {ExpressionRegister}");

                        break;
                    case TokenType.star:
                        consume();
                        break;
                    case TokenType.minus:
                        consume();
                        break;
                    case TokenType.fslash:
                        consume();
                        break;
                    default:
                        break;
                }
            }

            if (peek().Type != TokenType.close_curly)
            {
                Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                Environment.Exit(1);
            }
            consume();

            Assembly_Src.Add($"pop R{RegisterCount} ; result buffer");
            Assembly_Src.Add("pop AX");
            Assembly_Src.Add("pop ZX");
            Assembly_Src.Add($"push R{RegisterCount}");
        }
        string GetVariableName(string name)
        {
            return $"{Variable_Word}{name}";
        }

        Token peek(int offset = 0)
        {
            if (m_index + offset >= m_src.Length || m_index + offset < 0)
            {
                return null;
            }
            else
            {
                return m_src[m_index + offset];
            }
        }
        Token consume()
        {
            return m_src[m_index++];
        }
    }
}