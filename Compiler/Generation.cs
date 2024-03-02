using System.Reflection.Metadata.Ecma335;

namespace Compiler
{
    public class Generation
    {

        int m_index = 0;
        Token[] m_src;

        bool IsGlobal = false;
        bool IsPublic = false;
        bool IsPointer = false;
        bool IsConst = false;
        bool Call_Func = false;
        int Level = 0;

        public List<Variable> Variables = new List<Variable>();
        public List<string> Assembly_Src = new List<string>();
        public List<string> Const_Src = new List<string>();

        const string Variable_Word = "Var_";

        public bool Debug = false;
        string Func_Name = "";
        const uint ArgumentVariableOffset = 0x11000;
        const uint VariableOffset = 0x12000;
        uint ArgumentVariable = 0;
        uint VariableCount = 0;
        uint RegisterCount = 1;
        uint ArgumentCount = 0;

        public void Build(Tokenization tokenization)
        {
            m_src = tokenization.tokens.ToArray();
            gen();
        }
        void gen()
        {
            Assembly_Src.Add($"mov MB, #2");
            while (peek() != null)
            {
                if(peek(-8) != null && peek(-8).Type == TokenType.ident && peek(-8).Value == "Main" && peek(-1).Type == TokenType.close_paren)
                {
                    Assembly_Src.Add("");
                    Assembly_Src.Add("call [Exit]");
                }
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
                    case TokenType.const_:
                        consume();
                        IsConst = true;
                        break;
                    case TokenType.ptr:
                        consume();
                        IsPointer = true;
                        break;
                    case TokenType.global:
                        consume();
                        IsGlobal = true;
                        break;
                    case TokenType.public_:
                        consume();
                        IsPublic = true;
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
                                Assembly_Src.Add($"; moving Address 0x{Convert.ToString(variable.Address, 16)} into an temp register to return out of the func");
                                Assembly_Src.Add($"mov {GetTempRegister()}, [{Convert.ToString(variable.Address, 16)}h]");
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
                            if(RegisterCount > 1)
                            {
                                RegisterCount--;
                                Assembly_Src.Add($"mov ZX, {GetTempRegister()}");
                            }

                            Assembly_Src.Add($"; returning from {Func_Name} with {ArgumentCount} argsuments");
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
            Assembly_Src.Add($"");
            Assembly_Src.Add($"; Inports");
            Assembly_Src.Add($".include ./Libs/Function.basm");
            Assembly_Src.Add($"");
            Assembly_Src.InsertRange(Assembly_Src.Count, Const_Src);
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
                                Token token = consume();
                                if (peek().Type != TokenType.ident)
                                {
                                    Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                                    Environment.Exit(1);
                                }
                                name = Parse_Expr(out _);
                                gen_Local_Variable(name, FuncName, token.Type, true, token);
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
                    CompilerErrors.ExpectedError(peek(), TokenType.close_paren);
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
                CompilerErrors.ExpectedError(peek(), TokenType.open_paren);
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
                    CompilerErrors.ExpectedError(peek(), TokenType.comma);
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
                CompilerErrors.ExpectedError(peek(), TokenType.open_paren);
            }

            Assembly_Src.Add($"{Environment.NewLine}; Exit");

            consume(); // open_paren

            if (peek().Type == TokenType.int_lit)
            {
                string Exit_Value = Parse_Expr(out _);
                if (peek().Type != TokenType.close_paren)
                {
                    CompilerErrors.ExpectedError(peek(), TokenType.close_paren);
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
                        CompilerErrors.ExpectedError(peek(), TokenType.close_paren);
                    }
                    consume();
                    Assembly_Src.Add($"mov AX, [{Convert.ToString(variable.Address, 16)}h]");
                    Assembly_Src.Add($"int #0");
                }
            }
        }

        void gen_Free()
        {
            if (peek().Type != TokenType.open_paren)
            {
                CompilerErrors.ExpectedError(peek(), TokenType.open_paren);
            }

            consume();
            if (peek().Type == TokenType.ampersand)
            {
                consume();
                string Name = consume().Value;
                Variable variable = get_Variable(GetVariableName(Name));
                if (variable.IsPtr == true)
                {
                    CompilerErrors.ErrorVariablePointer(Name, peek(), variable);
                    //Console.WriteLine($"ERORR {TokenType.ampersand} at line {peek().Line}");
                    //Console.WriteLine($"{variable.Name} is a Pointer");
                    //Environment.Exit(1);
                }
                gen_FreeVariable(variable);
            }
            else
            {
                string Name = consume().Value;
                Variable variable = get_Variable(GetVariableName(Name));
                if (variable.IsPtr != true)
                {
                    CompilerErrors.ErrorVariableNotPointer(Name, peek(), variable);
                    //Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)} at line {peek().Line}");
                    //Console.WriteLine($"{variable.Name} is not a Pointer");
                    //Environment.Exit(1);
                }
                gen_FreeVariable(variable);
            }

            if (peek().Type != TokenType.close_paren)
            {
                CompilerErrors.ExpectedError(peek(), TokenType.close_paren);
            }
            consume();
        }

        void gen_FreeVariable(Variable variable, bool force = false)
        {
            Assembly_Src.Add("");
            if (force)
            {
                Assembly_Src.Add($"mov [{Convert.ToString(variable.Address, 16)}h], #0");
                if(variable.FuncName == "")
                {
                    VariableCount--;
                }
                else
                {
                    ArgumentVariable--;
                }
                Variables.Remove(variable);
            }
            else
            {
                if (variable.IsProtected == false)
                {
                    Assembly_Src.Add($"mov [{Convert.ToString(variable.Address, 16)}h], #0");
                    if (variable.FuncName == "")
                    {
                        VariableCount--;
                    }
                    else
                    {
                        ArgumentVariable--;
                    }
                    Variables.Remove(variable);
                }
            }
        }

        void genAssingVariable(TokenType type, bool Local)
        {
            string Name = Parse_Expr(out _);
            if (peek().Type == TokenType.eq)
            {
                consume();
                string value = Parse_Expr(out Token Token);
                if (IsConst == true)
                {
                    gen_const_Variable(Name, Token, type, IsPointer, value);
                    IsConst = false;
                }
                else if (value != "" && type == TokenType.ident)
                {
                    if (Level > 0)
                    {
                            Assembly_Src.Add($"");
                        if (value.StartsWith('[') && value.EndsWith(']') &&
                            Token.Type == TokenType.ident && value[1] != Variable_Word[0] &&
                            !IsDigit(value.TrimEnd(']').TrimStart('[')))
                        {
                            Name = Name.TrimEnd(']').TrimStart('[');
                            value = value.TrimEnd(']').TrimStart('[');
                            Assembly_Src.Add($"");
                            Assembly_Src.Add($"mov [{Convert.ToString(Convert.ToInt32(Name), 16)}h], [{Convert.ToString(Convert.ToInt32(value), 16)}h]");
                        }
                        else if (Token.Type == TokenType.int_lit)
                        {
                            Name = Name.TrimEnd(']').TrimStart('[');
                            Assembly_Src.Add($"");
                            Assembly_Src.Add($"mov [{Convert.ToString(Convert.ToInt32(Name), 16)}h], #{value}");
                        }
                        else if (Token.Type == TokenType.ident && IsDigit(value))
                        {
                            Name = Convert.ToString(Convert.ToUInt32(Name.TrimEnd(']').TrimStart('[')), 16);
                            Assembly_Src.Add($"; [{Name}h] == #{value}h");
                            Assembly_Src.Add($"mov [{Name}h], #{value}h");
                        }
                        else if (Token.Type == TokenType.ident && IsDigit(value.TrimEnd(']').TrimStart('[')))
                        {
                            Name = Convert.ToString(Convert.ToUInt32(Name.TrimEnd(']').TrimStart('[')), 16);
                            value = Convert.ToString(Convert.ToUInt32(value.TrimEnd(']').TrimStart('[')), 16);
                            Assembly_Src.Add($"; [{Name}h] == [{value}h]");
                            Assembly_Src.Add($"mov [{Name}h], [{value}h]");
                        }
                    }
                }
                else if (Local && type != TokenType.ident)
                {
                    if (peek(0).Type == TokenType.open_paren)
                    {
                        gen_CallFunc(value);
                        if (Level > 0)
                        {
                            gen_Local_Variable(Name, Func_Name, type, false, Token, RegisterValue: "ZX");
                        }
                        else
                        {
                            if (IsGlobal == true)
                            {
                                gen_global_Variable(Name, type, IsPointer, RegisterValue: "ZX");
                                IsGlobal = false;
                            }
                            else if (IsPublic == true)
                            {
                                gen_public_Variable(Name, type, IsPointer, RegisterValue: "ZX");
                                IsPublic = false;
                            }
                            else
                            {
                                gen_Local_Variable(Name, Func_Name, type, false, Token, RegisterValue: "ZX");
                            }
                        }
                        if (IsPointer) IsPointer = false;
                    }
                    else if (value.StartsWith("[V"))
                    {
                        if (Level > 0)
                        {
                            gen_Local_Variable(Name, Func_Name, type, false, Token, value.TrimStart('[').TrimEnd(']'));
                        }
                        else
                        {
                            if (IsGlobal == true)
                            {
                                gen_global_Variable(Name, type, IsPointer, value.TrimStart('[').TrimEnd(']'));
                                IsGlobal = false;
                            }
                            else if (IsPublic == true)
                            {
                                gen_public_Variable(Name, type, IsPointer, value.TrimStart('[').TrimEnd(']'));
                                IsPublic = false;
                            }
                            else
                            {
                                gen_Local_Variable(Name, Func_Name, type, false, Token, value.TrimStart('[').TrimEnd(']'));
                            }
                        }
                        if (IsPointer) IsPointer = false;
                    }
                    else if (Token.Type == TokenType.int_lit)
                    {
                        if (Level > 0)
                        {
                            gen_Local_Variable(Name, Func_Name, type, false, Token, value);
                        }
                        else
                        {
                            if (IsGlobal == true)
                            {
                                gen_global_Variable(Name, type, IsPointer, value);
                                IsGlobal = false;
                            }
                            else if (IsPublic == true)
                            {
                                gen_public_Variable(Name, type, IsPointer, value);
                                IsPublic = false;
                            }
                            else
                            {
                                gen_Local_Variable(Name, Func_Name, type, false, Token, value);
                            }
                        }
                        if (IsPointer) IsPointer = false;
                    }
                    else
                    {
                        bool IsAddress;
                        if (value.Length == 0)
                        {
                            IsAddress = false;
                        }
                        else
                        {
                            IsAddress = IsDigit(value);
                        }

                        if (IsAddress == false)
                        {
                            if (Level > 0)
                            {
                                gen_Local_Variable(Name, Func_Name, type, false, Token);
                            }
                            else
                            {
                                if (IsGlobal == true)
                                {
                                    gen_global_Variable(Name, type, IsPointer);
                                    IsGlobal = false;
                                }
                                else if (IsPublic == true)
                                {
                                    gen_public_Variable(Name, type, IsPointer);
                                    IsPublic = false;
                                }
                                else
                                {
                                    gen_Local_Variable(Name, Func_Name, type, false, Token);
                                }
                            }
                        }
                        else
                        {
                            value += "h";
                            if (Level > 0)
                            {
                                gen_Local_Variable(Name, Func_Name, type, false, Token, value);
                            }
                            else
                            {
                                if (IsGlobal == true)
                                {
                                    gen_global_Variable(Name, type, IsPointer, value);
                                    IsGlobal = false;
                                }
                                else if (IsPublic == true)
                                {
                                    gen_public_Variable(Name, type, IsPointer, value);
                                    IsPublic = false;
                                }
                                else
                                {
                                    gen_Local_Variable(Name, Func_Name, type, false, Token, value);
                                }
                            }
                        }
                        if (IsPointer) IsPointer = false;
                    }
                }
                else if (Token.Type == TokenType.open_curly)
                {
                    if(type == TokenType.ident)
                    {
                        Name = Name.TrimStart('[').TrimEnd(']');
                        Name = "[" + Convert.ToString(Convert.ToUInt32(Name), 16) + "h]";
                        Assembly_Src.Add("");
                        Assembly_Src.Add($"pop {GetTempRegister()}");
                        Assembly_Src.Add($"mov {Name}, {GetTempRegister()}");
                    }
                    else
                    {

                    }
                }
                else
                {
                    CompilerErrors.ExpectedExpressionError(peek());
                }
            }
            else
            {
                CompilerErrors.ExpectedError(peek(), TokenType.eq);
            }
        }

        string Parse_Expr(out Token token)
        {
            switch (peek().Type)
            {
                case TokenType.ident:
                    if (isVariable(GetVariableName(peek().Value)))
                    {
                        Variable variable = get_Variable(GetVariableName(peek().Value));
                        if (variable.IsConst == true)
                        {
                            token = consume();
                            return $"[{variable.Name}]";
                        }
                        else
                        {
                            token = consume();
                            return $"[{Convert.ToString(variable.Address)}]";
                        }
                    }
                    else if (!isVariable(GetVariableName(peek().Value)))
                    {
                        token = peek();
                        return consume().Value;
                    }
                    token = peek();
                    return "";
                case TokenType.ampersand:
                    consume();
                    if (isVariable(GetVariableName(peek().Value)))
                    {
                        Variable variable = get_Variable(GetVariableName(peek().Value));
                        token = consume();
                        return $"{Convert.ToString(variable.Address, 16)}";
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
                    token = peek();
                    CompileExpression(Debug);
                    return "";
                default:
                    CompilerErrors.ExpectedExpressionError(peek());
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

        public void gen_const_Variable(string name, Token tokenType, TokenType type, bool Pointer, string value)
        {
            uint VariableAddress = VariableOffset + VariableCount;
            VariableCount++;
            Assembly_Src.Add($"");
            Assembly_Src.Add($"; const {Tokenization.to_string(type)} {Variable_Word}{name} = {value}");
            if (value != "")
            {
                Const_Src.Add($"; const {Tokenization.to_string(type)} {name} = {value} at line {tokenType.Line}");
                Const_Src.Add($"{Variable_Word}{name}:");
                Const_Src.Add($".word {value}");
            }
            else
            {
                CompilerErrors.ExpectedError(tokenType, TokenType.int_lit);
            }
            NewVariable(name, VariableAddress, false, false, true, true, Pointer, tokenType.Type, "", true);
            Variables.Add(new Variable()
            {
                Name = GetVariableName(name),
                Address = VariableAddress,
                IsLocal = false,
                IsConst = true,
                IsPublic = false,
                IsGlobal = true,
                Size = GetSize(tokenType.Type),
                IsProtected = true,
                IsPtr = Pointer,
            });
        }
        public void gen_global_Variable(string name, TokenType tokenType, bool Pointer, string value = "", string RegisterValue = "")
        {
            uint VariableAddress = VariableOffset + VariableCount;
            VariableCount++;
            Assembly_Src.Add($"");
            Assembly_Src.Add($"; global type Var_{name} = [SP]");
            if (value != "")
            {
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], #{value}");
            }
            else if (RegisterValue != "")
            {
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], {RegisterValue}");
            }
            else
            {
                Assembly_Src.Add($"inc SP");
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], [SP]");
            }

            NewVariable(name, VariableAddress, false, false, true, false, Pointer, tokenType, "", false);
        }

        public void gen_public_Variable(string name, TokenType tokenType, bool Pointer, string value = "", string RegisterValue = "")
        {
            uint VariableAddress = VariableOffset + VariableCount;
            VariableCount++;
            Assembly_Src.Add($"");
            Assembly_Src.Add($"; public type Var_{name} = [SP]");
            if (value != "")
            {
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], #{value}");
            }
            else if (RegisterValue != "")
            {
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], {RegisterValue}");
            }
            else
            {
                Assembly_Src.Add($"inc SP");
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], [SP]");
            }
            NewVariable(name, VariableAddress, false, true, false, false, Pointer, tokenType, "", false);
        }
        public void gen_Local_Variable(string name, string func_name, TokenType tokenType, bool Args, Token TokenLine, string value = "", string RegisterValue = "")
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
            if (isVariable(value) == true)
            {
                Variable ConstVariable = get_Variable(value);
                Assembly_Src.Add($"; local type Var_{name} = {ConstVariable.Name}");
                Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], [{ConstVariable.Name}]");
            }
            else
            {
                Assembly_Src.Add($"; local type Var_{name} = [SP]");
                if (value != "")
                {
                    Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], #{value}");
                }
                else if (RegisterValue != "")
                {
                    Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], {RegisterValue}");
                }
                else
                {
                    Assembly_Src.Add($"inc SP");
                    Assembly_Src.Add($"mov [{Convert.ToString(VariableAddress, 16)}h], [SP]");
                }
            }
            NewVariable(name, VariableAddress, true, false, false, false, false, tokenType, func_name, Args);
        }
        void NewVariable(string name, uint Address, bool IsLocal, bool IsPublic, bool IsGlobal, bool IsConst, bool IsPtr, TokenType type, string func_name, bool IsProtected)
        {
            Variables.Add(new Variable()
            {
                Name = GetVariableName(name),
                Address = Address,
                IsLocal = IsLocal,
                IsPublic = IsPublic,
                IsGlobal = IsGlobal,
                IsConst = IsConst,
                IsPtr = IsPtr,
                Size = GetSize(type),
                FuncName = func_name,
                IsProtected = IsProtected,
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
            
            int ExprOperator = 2;
            int Save_m_index = m_index;
            int End_Index = m_index;
            while (peek().Type != TokenType.close_curly)
            {
                consume();
                End_Index++;
            }
            m_index = Save_m_index;

            if (peek().Type != TokenType.open_curly)
            {
                Console.WriteLine($"ERORR {Tokenization.to_string(peek().Type)}");
                Environment.Exit(1);
            }
            consume();

            Assembly_Src.Add("");
            Assembly_Src.Add("push ZX");
            Assembly_Src.Add("push AX");
            Assembly_Src.Add($"");
            uint Address;
            string name;
            int Times = 0;
            Stack<int> stack = new Stack<int>();
            StartAgain:
            while (peek().Type != TokenType.close_curly)
            {
                Token token = peek();
                switch (token.Type)
                {
                    case TokenType.int_lit:
                    case TokenType.Hex_int_lit:
                    case TokenType.Bin_int_lit:
                        if (Times == 0)
                        {
                            Assembly_Src.Add($"push #{Parse_Expr(out _)}");
                            Assembly_Src.Add($"");
                        }
                        else
                        {
                            consume();
                        }
                        break;
                    case TokenType.ident:
                        if (Times == 0)
                        {
                            name = peek().Value;
                            Address = Convert.ToUInt32(Parse_Expr(out _).TrimStart('[').TrimEnd(']'));
                            if (debug)
                            {
                                Assembly_Src.Add($"; {name}");
                            }
                            Assembly_Src.Add($"push [{Convert.ToString(Address, 16)}h]");
                            Assembly_Src.Add($"");
                        }
                        else
                        {
                            consume();
                        }
                        break;
                    case TokenType.close_paren:
                        if(Times == 0 && stack.Count == 0)
                        {
                            if (debug)
                            {
                                Assembly_Src.Add($"; {token.Type}");
                            }
                            ExprOperator = 1;
                        }
                        if (Times == 0 && stack.Count > 0)
                        {
                            m_index = stack.Pop();
                            ExprOperator--;
                        }
                            consume();
                        break;
                    case TokenType.open_paren:
                        if (Times == 0)
                        {
                            if (debug)
                            {
                                Assembly_Src.Add($"; {token.Type}");
                            }
                            stack.Push(m_index + 1);
                            ExprOperator = 1;
                        }
                            consume();
                        break;
                    case TokenType.minus:
                    case TokenType.plus:
                        if (ExprOperator == 0)
                        {
                            if (debug)
                            {
                                Assembly_Src.Add($"; {token.Type} {ExprOperator}");
                            }
                            consume();
                            if (Times == 0)
                            {
                                if (peek().Type == TokenType.ident)
                                {
                                    name = peek().Value;
                                    Address = Convert.ToUInt32(Parse_Expr(out _).TrimStart('[').TrimEnd(']'));
                                    if (debug)
                                    {
                                        Assembly_Src.Add($"; {name}");
                                    }
                                    Assembly_Src.Add($"push [{Convert.ToString(Address, 16)}h]");
                                }
                                else
                                {
                                    Assembly_Src.Add($"push #{Parse_Expr(out _)}");
                                }
                            }
                            else
                            {
                                consume();
                            }
                            Assembly_Src.Add($"");
                            Assembly_Src.Add($"pop {Register}");
                            Assembly_Src.Add($"pop {ExpressionRegister}");
                            if (token.Type == TokenType.plus)
                            {
                                Assembly_Src.Add($"add {ExpressionRegister}, {Register}");
                            }
                            if (token.Type == TokenType.minus)
                            {
                                Assembly_Src.Add($"sub {ExpressionRegister}, {Register}");
                            }
                            Assembly_Src.Add($"push {ExpressionRegister}");
                            Assembly_Src.Add($"");
                        }
                        else
                        {
                            consume();
                        }
                        break;
                    case TokenType.fslash:
                    case TokenType.star:
                        if (ExprOperator == 1)
                        {
                            if (debug)
                            {
                                Assembly_Src.Add($"; {token.Type} {ExprOperator}");
                            }
                            consume();
                            if (Times == 0)
                            {
                                if (peek().Type == TokenType.ident)
                                {
                                    name = peek().Value;
                                    Address = Convert.ToUInt32(Parse_Expr(out _).TrimStart('[').TrimEnd(']'));
                                    if (debug)
                                    {
                                        Assembly_Src.Add($"; {name}");
                                    }
                                    Assembly_Src.Add($"push [{Convert.ToString(Address, 16)}h]");
                                }
                                else
                                {
                                    Assembly_Src.Add($"push #{Parse_Expr(out _)}");
                                }
                            }
                            else
                            {
                                consume();
                            }
                            Assembly_Src.Add($"");
                            Assembly_Src.Add($"pop {Register}");
                            Assembly_Src.Add($"pop {ExpressionRegister}");
                            if (token.Type == TokenType.star)
                            {
                                Assembly_Src.Add($"mul {ExpressionRegister}, {Register}");
                            }
                            if (token.Type == TokenType.fslash)
                            {
                                Assembly_Src.Add($"mul {ExpressionRegister}, {Register}");
                            }
                            Assembly_Src.Add($"push {ExpressionRegister}");
                            Assembly_Src.Add($"");
                        }
                        else
                        {
                            consume();
                        }
                        break;

                    default:
                        CompilerErrors.ExpectedExpressionError(peek());
                        break;
                }
            }

            if(Times == 0 || Times == 1)
            {
                if(stack.Count > 0)
                {
                    m_index = Save_m_index;
                    ExprOperator--;
                    consume();
                }
                Times++;
                m_index = Save_m_index;
                ExprOperator--;
                consume();
                goto StartAgain;
            }

            if (peek().Type != TokenType.close_curly)
            {
                CompilerErrors.ExpectedError(peek(), TokenType.close_curly);
            }
            consume();

            Assembly_Src.Add($"pop {GetTempRegister()}");
            Assembly_Src.Add("pop AX");
            Assembly_Src.Add("pop ZX");
            Assembly_Src.Add($"push {GetTempRegister()}");
        }

        string GetTempRegister()
        {
            return $"R{RegisterCount}";
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
        bool IsDigit(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsDigit(str[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}