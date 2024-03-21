using Compiler.nodes;
using System.Reflection.Metadata.Ecma335;

namespace Compiler
{
    public class Generation
    {
        bool HasMainFunc = false;

        int Level = 0;
        Stack<int> m_scopes = new Stack<int>();
        int m_stack_size = 0;
        const int Pushr_length = 7;

        public List<Variable> m_stack_var = new List<Variable>();
        public Variable[] m_var = new Variable[0x8000];
        public List<Function> m_functions = new List<Function>();
        public List<string> m_output = new List<string>();
        public List<string> Const_Src = new List<string>();

        public bool Debug = false;
        const int VariableOffset = 0x12000;
        int VariableCount = 0;
        int RegisterCount = 1;

        public object gen_expr(NodeExpr nodeExpr)
        {
            if (testType(nodeExpr.var, typeof(NodeTermIdent)))
            {
                NodeTermIdent nodeTermIdent = (NodeTermIdent)nodeExpr.var;
                return nodeTermIdent;
            }
            else if (testType(nodeExpr.var, typeof(NodeTermIntLit)))
            {
                NodeTermIntLit nodeTermIntLit = (NodeTermIntLit)nodeExpr.var;
                return nodeTermIntLit;
            }
            return null;
        }
        void gen_stmtreturn(NodeStmtReturn nodeStmtReturn)
        {
            NodeTermIntLit nodeTermIntLit = (NodeTermIntLit)gen_expr(nodeStmtReturn.expr);
            m_output.Add($"_{m_functions.Last().Name}_R:");
            m_output.Add($"mov ZX, #{nodeTermIntLit.int_lit.Value}");
            m_output.Add($"popr");
            m_output.Add($"mov SP, BP");
            m_output.Add($"pop BP");
            m_output.Add($"ret #0");
        }
        void gen_stmtfunc(NodeStmtFunc nodeStmtFunc)
        {
            NodeTermIdent name = (NodeTermIdent)gen_expr(nodeStmtFunc.Name);
            TokenType returnType = nodeStmtFunc.ReturnType.type;
            m_functions.Add(new Function()
            {
                Name = name.ident.Value,
                RetrunType = returnType,
            });
            
            if(name.ident.Value == "Main")
            {
                HasMainFunc = true;
            }

            m_output.Add($"_{name.ident.Value}:");
            push("BP");
            m_output.Add($"mov BP, SP");
            pushr();
        }

        public void gen_assing(NodeStmtAssign nodeStmtAssign)
        {
            NodeTermIdent name = (NodeTermIdent)gen_expr(nodeStmtAssign.name);
            NodeTermIntLit value = (NodeTermIntLit)gen_expr(nodeStmtAssign.value);
            TokenType type = nodeStmtAssign.type.type;
            TokenType OperatiorType1 = nodeStmtAssign.OperatorToken.tokens[0];
            TokenType OperatiorType2 = nodeStmtAssign.OperatorToken.tokens[1];

            byte size = 0x06;

            switch (type)
            {
                case TokenType.byte_:
                    size = 1;
                    break;
                case TokenType.char_:
                    size = 1;
                    break;
                case TokenType.word:
                    size = 2;
                    break;
                case TokenType.int_:
                    size = 4;
                    break;
                case TokenType.let:
                    size = 4;
                    break;
                default:
                    break;
            }

            if (OperatiorType1 == TokenType.eq)
            {
                if (m_scopes.Count != 0)
                {
                    m_stack_var.Add(new Variable()
                    {
                        Name = name.ident.Value,
                        Stack_loc = m_stack_size,
                        Size = size
                    }) ;
                    m_output.Add($"mov DX, #{value.int_lit.Value}");
                    push("DX");
                }
                else
                {
                    int address = VariableOffset + VariableCount;
                    m_stack_var.Add(new Variable()
                    {
                        Name = name.ident.Value,
                        Stack_loc = address,
                        Size = size
                    });
                    m_output.Add($"mov [{Convert.ToString(address, 16)}h], #{value.int_lit.Value}");
                }
            }
        }

        public void gen_scope(NodeScope nodeScope)
        {
            begin_scope();
            for (int i = 0; i < nodeScope.stmts.Length; i++)
            {
                m_output.Add("");
                gen_stmt(nodeScope.stmts[i]);
            }
            end_scope();
        }

        public void gen_stmt(NodeStmt nodeStmt)
        {
            if (nodeStmt.stmt == null) return;
            if (testType(nodeStmt.stmt, typeof(NodeStmtFunc)))
            {
                gen_stmtfunc((NodeStmtFunc)nodeStmt.stmt);
            }
            else if (testType(nodeStmt.stmt, typeof(NodeStmtReturn)))
            {
                gen_stmtreturn((NodeStmtReturn)nodeStmt.stmt);
            }
            else if (testType(nodeStmt.stmt, typeof(NodeScope)))
            {
                gen_scope((NodeScope)nodeStmt.stmt);
            }
            else if (testType(nodeStmt.stmt, typeof(NodeStmtAssign)))
            {
                gen_assing((NodeStmtAssign)nodeStmt.stmt);
            }
        }

        public void gen_prog(NodeProg nodeProg)
        {
            m_output.Add("call [_Main]");

            for (int i = 0; i < nodeProg.stmts.Length; i++)
            {
                gen_stmt(nodeProg.stmts[i]);
            }

            if(HasMainFunc == false)
            {
                Console.WriteLine("program needs Main func");
                Environment.Exit(0);
            }
        }
        bool testType(Type stmtType, Type testedType)
        {
            if (testedType == stmtType)
            {
                return true;
            }
            return false;
        }
        bool testType(object stmtType, Type testedType)
        {
            if (testedType == stmtType.GetType())
            {
                return true;
            }
            return false;
        }

        void push(string reg)
        {
            m_output.Add($"push {reg}");
            m_stack_size++;
        }

        void pop(string reg)
        {
            m_output.Add($"pop {reg}");
            m_stack_size--;
        }

        void pushr()
        {
            m_output.Add($"pushr");
            m_stack_size += Pushr_length;
        }

        void popr()
        {
            m_output.Add($"popr");
            m_stack_size -= Pushr_length;
        }

        void begin_scope()
        {
            m_output.Add($"; scope {m_scopes.Count}");
            m_scopes.Push(m_stack_var.Count);
        }

        void end_scope()
        {
            m_output.Add($"; end scope {m_scopes.Count - 1}");
            int pop_count = (m_stack_var.Count - m_scopes.Last());
            if (pop_count != 0)
            {
                m_output.Add($"add sp, #{pop_count}");
            }
            m_stack_size -= pop_count;
            for (int i = 0; i < pop_count; i++)
            {
                m_stack_var.RemoveAt(i);
            }
            m_scopes.Pop();
        }

        /*
        void gen_Func(TokenType ReturnType)
        {
            ArgumentCount = 0;
            if (//peek().Type == TokenType.ident)
            {
                string FuncName = Parse_Expr(out _);
                Func_Name = FuncName;
                if (//peek().Type == TokenType.open_paren)
                {
                    //consume();
                    Assembly_Src.Add($"{Environment.NewLine}; func return type is {Tokenization.to_string(ReturnType)}");
                    Assembly_Src.Add($"Func_{FuncName}:");
                    Assembly_Src.Add($"pushr");
                }
                if (//peek().Type != TokenType.close_paren)
                {
                    Assembly_Src.Add("mov ZX, SP");
                    Assembly_Src.Add("add SP, #8");
                    while (//peek().Type != TokenType.close_paren)
                    {
                        string name;
                        switch (//peek().Type)
                        {
                            case TokenType.char_:
                            case TokenType.byte_:
                            case TokenType.word:
                            case TokenType.let:
                            case TokenType.int_:
                                Token token = //consume();
                                if (//peek().Type != TokenType.ident)
                                {
                                    Console.WriteLine($"ERORR {Tokenization.to_string(//peek().Type)}");
                                    Environment.Exit(1);
                                }
                                name = Parse_Expr(out _);
                                gen_Local_Variable(name, FuncName, token.Type, true, token);
                                if (//peek().Type == TokenType.close_paren) break;
                                if (//peek().Type != TokenType.comma)
                                {
                                    Console.WriteLine($"ERORR {Tokenization.to_string(//peek().Type)}");
                                    Environment.Exit(1);
                                }
                                //consume();
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
                if (//peek().Type != TokenType.close_paren)
                {
                    CompilerErrors.ExpectedError(//peek(), TokenType.close_paren);
                }
                //consume();
            }
        }

        void gen_CallFunc()
        {
            Call_Func = true;
            string funcName = //consume().Value;
            CallFunc(funcName);
        }
        void gen_CallFunc(string funcName)
        {
            //CallFunc(funcName);
        }

        void CallFunc(string funcname)
        {
            List<string> args = new List<string>();
            if (//peek().Type != TokenType.open_paren)
            {
                CompilerErrors.ExpectedError(//peek(), TokenType.open_paren);
            }
            //consume();
            while (//peek().Type != TokenType.close_paren)
            {
                if (//peek().Type == TokenType.int_lit)
                {
                    string value = "#" + //consume().Value;
                    args.Add(value);
                }

                if (//peek().Type == TokenType.close_paren) break;

                if (//peek().Type != TokenType.comma)
                {
                    CompilerErrors.ExpectedError(//peek(), TokenType.comma);
                }
                //consume();
            }

            //consume();

            for (int i = args.Count - 1; i > -1; i--)
            {
                Assembly_Src.Add($"push {args[i]}");
            }

            Assembly_Src.Add($"call [Func_{funcname}]");
        }

        void gen_Exit()
        {
            if (//peek().Type != TokenType.open_paren)
            {
                CompilerErrors.ExpectedError(//peek(), TokenType.open_paren);
            }

            Assembly_Src.Add($"{Environment.NewLine}; Exit");

            //consume(); // open_paren

            if (//peek().Type == TokenType.int_lit)
            {
                string Exit_Value = Parse_Expr(out _);
                if (//peek().Type != TokenType.close_paren)
                {
                    CompilerErrors.ExpectedError(//peek(), TokenType.close_paren);
                }
                //consume();
                Assembly_Src.Add($"mov AX, #{Exit_Value}");
                Assembly_Src.Add($"int #0");
            }
            else if (//peek().Type == TokenType.ident)
            {
                if (isVariable(GetVariableName(//peek().Value)) == true)
                {
                    string VariableName = GetVariableName(//consume().Value);
                    Variable variable = get_Variable(VariableName);
                    if (//peek().Type != TokenType.close_paren)
                    {
                        CompilerErrors.ExpectedError(//peek(), TokenType.close_paren);
                    }
                    //consume();
                    Assembly_Src.Add($"mov AX, [{Convert.ToString(variable.Address, 16)}h]");
                    Assembly_Src.Add($"int #0");
                }
            }
        }

        void gen_Free()
        {
            if (//peek().Type != TokenType.open_paren)
            {
                CompilerErrors.ExpectedError(//peek(), TokenType.open_paren);
            }

            //consume();
            if (//peek().Type == TokenType.ampersand)
            {
                //consume();
                string Name = //consume().Value;
                Variable variable = get_Variable(GetVariableName(Name));
                if (variable.IsPtr == true)
                {
                    CompilerErrors.ErrorVariablePointer(Name, //peek(), variable);
                    //Console.WriteLine($"ERORR {TokenType.ampersand} at line {//peek().Line}");
                    //Console.WriteLine($"{variable.Name} is a Pointer");
                    //Environment.Exit(1);
                }
                gen_FreeVariable(variable);
            }
            else
            {
                string Name = //consume().Value;
                Variable variable = get_Variable(GetVariableName(Name));
                if (variable.IsPtr != true)
                {
                    CompilerErrors.ErrorVariableNotPointer(Name, //peek(), variable);
                    //Console.WriteLine($"ERORR {Tokenization.to_string(//peek().Type)} at line {//peek().Line}");
                    //Console.WriteLine($"{variable.Name} is not a Pointer");
                    //Environment.Exit(1);
                }
                gen_FreeVariable(variable);
            }

            if (//peek().Type != TokenType.close_paren)
            {
                CompilerErrors.ExpectedError(//peek(), TokenType.close_paren);
            }
            //consume();
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
            if (//peek().Type == TokenType.eq)
            {
                //consume();
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
                    if (//peek(0).Type == TokenType.open_paren)
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
                    CompilerErrors.ExpectedExpressionError(//peek());
                }
            }
            else
            {
                CompilerErrors.ExpectedError(//peek(), TokenType.eq);
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

            Console.WriteLine($"ERORR can't find variable {name} at line {//peek().Line}");
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
            while (//peek().Type != TokenType.close_curly)
            {
                //consume();
                End_Index++;
            }
            m_index = Save_m_index;

            if (//peek().Type != TokenType.open_curly)
            {
                Console.WriteLine($"ERORR {Tokenization.to_string(//peek().Type)}");
                Environment.Exit(1);
            }
            //consume();

            Assembly_Src.Add("");
            Assembly_Src.Add("push ZX");
            Assembly_Src.Add("push AX");
            Assembly_Src.Add($"");
            
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
        */
    }
}