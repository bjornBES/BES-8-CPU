using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Ccompiler.nodes;
using Microsoft.VisualBasic;

public class Generator
{
    public bool DebugEnabled = false;

    private List<string> m_output = new List<string>();
    private List<string> m_ROdataoutput = new List<string>();

    readonly Encoding defualtEncoder = Encoding.ASCII;

    private int m_read_only_count = 0;
    private int m_label_count = 0;
    private int m_stack_size = 0;
    private int m_basePointer_offset = 0;
    private int m_last_argument_count = 0;
    private int m_temp_register_count = 1;
    private int m_bank = 0;

    Stack<int> m_scopes = new Stack<int>();
    private bool UseBP = false;
    private Variable[] m_variables = new Variable[0x8000]; // 32 kb
    private List<Variable> m_stack_variables = new List<Variable>(0x800);
    private const int m_var_offset = 0x001000;
    public string Main_Func_name = "main";
    const int Pushr_length = 7;

    const string Stack_Pointer = "SP";
    const string Base_Pointer = "BP";
    const string Data_Register = "DX";
    const string Expr_Register = "AX";

    int allocateVariable()
    {
        for (int i = 0; i < m_variables.Length; i++)
        {
            if (m_variables[i].address_loc == null)
            {
                return i;
            }
        }
        return -1;
    }
    int allocateVariable(int size, int length)
    {
        for (int i = 0; i < m_variables.Length; i++)
        {
            if (m_variables[i].address_loc == null)
            {
                byte get_var_index = (byte)i;
                while (true)
                {
                    if (m_variables[get_var_index].address_loc != null)
                    {
                        break;
                    }
                    get_var_index++;
                }

                Variable var = m_variables[get_var_index];
                int StartAddress = Convert.ToInt32(var.address_loc, 16);
                int EndAddress = StartAddress + (var.size * var.length);
                
            }
        }
        return -1;
    }
    int[] allocateVariable(int count)
    {
        List<int> result = new List<int>();
        for (int c = 0; c < count; c++)
        {
            for (int i = 0; i < m_variables.Length; i++)
            {
                if (m_variables[i].address_loc == null)
                {
                    result.Add(i);
                    break;
                }
            }
        }
        return result.ToArray();
    }
    Token gen_expr(NodeExpr nodeExpr, out NodeTerm result)
    {
        if (nodeExpr == null)
        {
            result = null;
            return new Token() { Type = TokenType.none, value = "" };
        }

        if (IsType(nodeExpr.var, typeof(NodeTerm)))
        {
            NodeTerm nodeTerm = GetType<NodeTerm>(nodeExpr.var);
            result = nodeTerm;
            if (IsType(nodeTerm.term, typeof(NodeTermIdent)))
            {
                NodeTermIdent termIdent = GetType<NodeTermIdent>(nodeTerm.term);
                return termIdent.ident;
            }
            else if (IsType(nodeTerm.term, typeof(NodeTermIntLit)))
            {
                NodeTermIntLit termIntLit = GetType<NodeTermIntLit>(nodeTerm.term);
                return termIntLit.int_lit;
            }
            else if (IsType(nodeTerm.term, typeof(NodeTermParen)))
            {
                NodeTermParen termParen = GetType<NodeTermParen>(nodeTerm.term);
                return gen_expr(termParen.expr, out nodeTerm);
            }
            else if (IsType(nodeTerm.term, typeof(NodeTermCharLit)))
            {
                NodeTermCharLit termCharLit = GetType<NodeTermCharLit>(nodeTerm.term);
                termCharLit.C.value = $"'{termCharLit.C.value}'";
                return termCharLit.C;
            }
            else if (IsType(nodeTerm.term, typeof(NodeTermStringLit)))
            {
                NodeTermStringLit nodeTermStringLit = GetType<NodeTermStringLit>(nodeTerm.term);
                string str = $"{nodeTermStringLit.str.value}\0";
                return new Token()
                {
                    column = nodeTermStringLit.str.column,
                    File = nodeTermStringLit.str.File,
                    line = nodeTermStringLit.str.line,
                    Type = TokenType.ident,
                    value = str
                };
            }
            else
            {
                Console.WriteLine($"{nodeExpr.var.GetType().Name} is not good 1");
                Environment.Exit(-1);
            }
        }
        else if (IsType(nodeExpr.var, typeof(NodeBinExpr)))
        {
            result = null;
            gen_bin_expr(GetType<NodeBinExpr>(nodeExpr.var));
            return new Token()
            {
                Type = TokenType.ident,
                value = Expr_Register,
            };
        }
        result = null;
        return new Token()
        {
            Type = TokenType.none,
            value = "",
        };
    }

    void gen_bin_expr(NodeBinExpr nodeBinExpr)
    {
            string leftItem;
            string rightItem;
        if (IsType(nodeBinExpr.expr, typeof(NodeBinExprAdd)))
        {
            NodeBinExprAdd nodeBinExprAdd = GetType<NodeBinExprAdd>(nodeBinExpr.expr);
            m_output.Add($"; add {gen_expr(nodeBinExprAdd.lhs, out _).value}, {gen_expr(nodeBinExprAdd.rhs, out _).value}");
            if(get_StackVariable(gen_expr(nodeBinExprAdd.lhs, out _).value) != null)
            {
                leftItem = $"[{Stack_Pointer} + {getStackLoc(gen_expr(nodeBinExprAdd.lhs, out _).value)}]";
            }
            else
            {
                leftItem = $"#{gen_expr(nodeBinExprAdd.lhs, out _).value}";
            }
            if (get_StackVariable(gen_expr(nodeBinExprAdd.rhs, out _).value) != null)
            {
                rightItem = $"[{Stack_Pointer} + {getStackLoc(gen_expr(nodeBinExprAdd.rhs, out _).value)}]";
            }
            else
            {
                rightItem = $"#{gen_expr(nodeBinExprAdd.rhs, out _).value}";
            }
            m_output.Add($"mov {Expr_Register}, {leftItem}");
            m_output.Add($"add {Expr_Register}, {rightItem}");
            //return termIdent.ident;
        }
        else if (IsType(nodeBinExpr.expr, typeof(NodeBinExprSub)))
        {
            NodeBinExprSub nodeBinExprSub = GetType<NodeBinExprSub>(nodeBinExpr.expr);
            m_output.Add($"; sub {gen_expr(nodeBinExprSub.lhs, out _).value}, {gen_expr(nodeBinExprSub.rhs, out _).value}");
            if (get_StackVariable(gen_expr(nodeBinExprSub.lhs, out _).value) != null)
            {
                leftItem = $"[{Stack_Pointer} + {getStackLoc(gen_expr(nodeBinExprSub.lhs, out _).value)}]";
            }
            else
            {
                leftItem = $"#{gen_expr(nodeBinExprSub.lhs, out _).value}";
            }
            if (get_StackVariable(gen_expr(nodeBinExprSub.rhs, out _).value) != null)
            {
                rightItem = $"[{Stack_Pointer} + {getStackLoc(gen_expr(nodeBinExprSub.rhs, out _).value)}]";
            }
            else
            {
                rightItem = $"#{gen_expr(nodeBinExprSub.rhs, out _).value}";
            }
            m_output.Add($"mov {Expr_Register}, {leftItem}");
            m_output.Add($"sub {Expr_Register}, {rightItem}");
            //return termIntLit.int_lit;
        }
        else if (IsType(nodeBinExpr.expr, typeof(NodeBinExprMulti)))
        {
            NodeBinExprMulti nodeBinExprSub = GetType<NodeBinExprMulti>(nodeBinExpr.expr);
            m_output.Add($"; mul {gen_expr(nodeBinExprSub.lhs, out _).value}, {gen_expr(nodeBinExprSub.rhs, out _).value}");
            if (get_StackVariable(gen_expr(nodeBinExprSub.lhs, out _).value) != null)
            {
                leftItem = $"[{Stack_Pointer} + {getStackLoc(gen_expr(nodeBinExprSub.lhs, out _).value)}]";
            }
            else
            {
                leftItem = $"#{gen_expr(nodeBinExprSub.lhs, out _).value}";
            }
            if (get_StackVariable(gen_expr(nodeBinExprSub.rhs, out _).value) != null)
            {
                rightItem = $"[{Stack_Pointer} + {getStackLoc(gen_expr(nodeBinExprSub.rhs, out _).value)}]";
            }
            else
            {
                rightItem = $"#{gen_expr(nodeBinExprSub.rhs, out _).value}";
            }
            m_output.Add($"mov {Expr_Register}, {leftItem}");
            m_output.Add($"mul {Expr_Register}, {rightItem}");
            //return termIntLit.int_lit;
        }
        else if (IsType(nodeBinExpr.expr, typeof(NodeBinExprDiv)))
        {
            NodeBinExprDiv nodeBinExprSub = GetType<NodeBinExprDiv>(nodeBinExpr.expr);
            m_output.Add($"; div {gen_expr(nodeBinExprSub.lhs, out _).value}, {gen_expr(nodeBinExprSub.rhs, out _).value}");
            if (get_StackVariable(gen_expr(nodeBinExprSub.lhs, out _).value) != null)
            {
                leftItem = $"[{Stack_Pointer} + {getStackLoc(gen_expr(nodeBinExprSub.lhs, out _).value)}]";
            }
            else
            {
                leftItem = $"#{gen_expr(nodeBinExprSub.lhs, out _).value}";
            }
            if (get_StackVariable(gen_expr(nodeBinExprSub.rhs, out _).value) != null)
            {
                rightItem = $"[{Stack_Pointer} + {getStackLoc(gen_expr(nodeBinExprSub.rhs, out _).value)}]";
            }
            else
            {
                rightItem = $"#{gen_expr(nodeBinExprSub.rhs, out _).value}";
            }
            m_output.Add($"mov {Expr_Register}, {leftItem}");
            m_output.Add($"div {Expr_Register}, {rightItem}");
            //return termIntLit.int_lit;
        }
        else
        {
            Console.WriteLine($"{nodeBinExpr.GetType().Name} is not good 2");
            Environment.Exit(-1);
        }
    }

    void gen_Operater_jump(RelationalOperater operater, string label)
    {
        switch (operater)
        {
            case RelationalOperater.equality:
                m_output.Add($"jne [{label}]");
                break;
            case RelationalOperater.inequality:
                m_output.Add($"je [{label}]");
                break;
        }
    }

    void gen_scope(NodeScope nodeScope)
    {
        if (DebugEnabled == true)
        {
            m_output.Add($"; {{");
        }
        begin_scope();
        for (int i = 0; i < nodeScope.ScopeStmts.Length; i++)
        {
            gen_stmt(nodeScope.ScopeStmts[i]);
        }
        if (m_scopes.Count == 1)
        {
            m_output.Add($"mov ZX, #FFFFFFh");
        }
        end_scope();
        if (DebugEnabled == true)
        {
            m_output.Add($"; }}");
        }
        m_output.Add($"");
    }

    void gen_if_pred(NodeIfPred pred, string end_label)
    {
        if (IsType(pred.var, typeof(NodeIfPredElif)))
        {
            NodeIfPredElif nodeIfPredElif = GetType<NodeIfPredElif>(pred.var);
            m_output.Add($"; else if");
            m_output.Add($"cmp " +
                         $"{get_variable_stack(gen_expr(nodeIfPredElif.TermComparer.lhs, out _).value)}, " +
                         $"{get_variable_stack(gen_expr(nodeIfPredElif.TermComparer.rhs, out _).value)}");
            string label = create_label();
            gen_Operater_jump(nodeIfPredElif.TermComparer.operater, label);
            gen_scope(nodeIfPredElif.Scope);
            m_output.Add($"jmp [{end_label}]");
            if(pred != null)
            {
                m_output.Add($"{label}:");
                gen_if_pred(nodeIfPredElif.pred, end_label);
            }
        }
        else if (IsType(pred.var, typeof(NodeIfPredElse))) 
        {
            NodeIfPredElse nodeIfPredElse = GetType<NodeIfPredElse>(pred.var);
            m_output.Add($"; else");
            gen_scope(nodeIfPredElse.Scope);
        }
    }

    void gen_stmt(NodeStmt nodeStmt)
    {
        if (IsType(nodeStmt, typeof(NodeStmtFunc)))
        {
            NodeStmtFunc nodeStmtFunc = GetType<NodeStmtFunc>(nodeStmt);
            if (DebugEnabled == true)
            {
                m_output.Add($"; {Tokenizer.To_String(nodeStmtFunc.ReturnType)} {gen_expr(nodeStmtFunc.Func_name, out _).value} with {nodeStmtFunc.arguments.Length} argument");
            }
            m_last_argument_count = nodeStmtFunc.arguments.Length;
            UseBP = true;
            m_output.Add($"_{gen_expr(nodeStmtFunc.Func_name, out _).value}:");
            push("BP");
            m_output.Add($"mov BP, SP");
            pushr();
            for (int i = 0; i < nodeStmtFunc.arguments.Length; i++)
            {
                gen_stmt(nodeStmtFunc.arguments[i]);
            }
            UseBP = false;
            switchBank(1);
        }
        else if (IsType(nodeStmt, typeof(NodeScope)))
        {
            NodeScope nodeScope = GetType<NodeScope>(nodeStmt);
            gen_scope(nodeScope);
        }
        else if (IsType(nodeStmt, typeof(NodeStmtFuncCall)))
        {
            NodeStmtFuncCall nodeStmtFuncCall = GetType<NodeStmtFuncCall>(nodeStmt);
            if (DebugEnabled == true)
            {
                m_output.Add($"; calling {gen_expr(nodeStmtFuncCall.FuncName, out _).value} with {nodeStmtFuncCall.arguments.Length} arguments");
            }
            for (int i = 0; i < nodeStmtFuncCall.arguments.Length; i++)
            {
                m_output.Add($"push #{gen_expr(nodeStmtFuncCall.arguments[i], out _).value}");
            }
            m_output.Add($"call [_{gen_expr(nodeStmtFuncCall.FuncName, out _).value}]");
        }
        else if (IsType(nodeStmt, typeof(NodeStmtIf)))
        {
            NodeStmtIf nodeStmtIf = GetType<NodeStmtIf>(nodeStmt);
            string label = create_label();

            if(DebugEnabled)
            {
                string Operater;
                if (nodeStmtIf.TermComparer.operater == RelationalOperater.equality)
                {
                    Operater = "==";
                }
                else if (nodeStmtIf.TermComparer.operater == RelationalOperater.inequality)
                {
                    Operater = "!=";
                }
                else
                {
                    Operater = "";
                }

                m_output.Add($"; if ({gen_expr(nodeStmtIf.TermComparer.lhs, out _).value} {Operater} {gen_expr(nodeStmtIf.TermComparer.rhs, out _).value})");
            }

            m_output.Add($"cmp " +
                $"{get_variable_stack(gen_expr(nodeStmtIf.TermComparer.lhs, out _).value)}, " +
                $"{get_variable_stack(gen_expr(nodeStmtIf.TermComparer.rhs, out _).value)}");
            gen_Operater_jump(nodeStmtIf.TermComparer.operater, label);
            gen_scope(nodeStmtIf.Scope);
            if(nodeStmtIf.pred != null)
            {
                string end_label = create_label();
                m_output.Add($"jmp [{end_label}]");
                m_output.Add($"{label}:");
                gen_if_pred(nodeStmtIf.pred, end_label);
                m_output.Add($"{end_label}:");
            }
            else
            {
                m_output.Add(label + ":");
            }
        }
        else if (IsType(nodeStmt, typeof(NodeStmtReturn)))
        {
            NodeStmtReturn nodeStmtReturn = GetType<NodeStmtReturn>(nodeStmt);
            if (DebugEnabled == true)
            {
                m_output.Add($"; returning with {gen_expr(nodeStmtReturn.expr, out _)}");
            }
            popr();
            pop("BP");
            m_output.Add($"mov ZX, #{gen_expr(nodeStmtReturn.expr, out _).value}");
            returnFromFunc();
            m_output.Add($"");
        }
        else if (IsType(nodeStmt, typeof(NodeStmtInt)))
        {
            NodeStmtInt nodeStmtInt = GetType<NodeStmtInt>(nodeStmt);
            if (DebugEnabled == true)
            {
                m_output.Add($"; int {gen_expr(nodeStmtInt.Name, out _).value}");
            }
            Token Name = gen_expr(nodeStmtInt.Name, out _);
            Token Value = gen_expr(nodeStmtInt.Value, out NodeTerm nodeTerm);
            gen_variable(DataSizes.INTSIZE, Name, Value, 1, nodeTerm);
        }
        else if (IsType(nodeStmt, typeof(NodeStmtChar)))
        {
            NodeStmtChar nodeStmtChar = GetType<NodeStmtChar>(nodeStmt);
            if (DebugEnabled == true)
            {
                m_output.Add($"; char {gen_expr(nodeStmtChar.Name, out _).value}");
            }
            Token Name = gen_expr(nodeStmtChar.Name, out _);
            Token Value = gen_expr(nodeStmtChar.Value, out NodeTerm nodeTerm);
            gen_variable(DataSizes.CHARSIZE, Name, Value, 1, nodeTerm);
        }
        else if (IsType(nodeStmt, typeof(NodeStmtFuncPrototype)))
        {
        }
        else
        {
            Console.WriteLine(nodeStmt.GetType().Name + " is not there");
        }
        m_output.Add("");
    }

    void gen_variable(int size, Token Name, Token Value, int length, NodeTerm nodeTerm)
    {
        switchBank(1);
        string V_name = Name.value;
        const char ReplacerChar = '!';
        string V_value = "";
        string ValueData = "";
        if (nodeTerm == null)
        {
            if (UseBP == true)
            {
                // argument
                int stack_addr = m_basePointer_offset;
                m_basePointer_offset++;
                m_output.Add($"; {Name.value} with size {size} as argument");
                m_stack_variables.Add(new Variable()
                {
                    address_loc = Convert.ToString(stack_addr, 16),
                    length = -1,
                    Pointers_Deep = 0,
                    name = Name.value,
                    size = -1,
                    UseBP = true,
                });
                return;
            }
        }
        else if (IsType(nodeTerm.term, typeof(NodeTermIntLit)))
        {
            V_value = $"#{ReplacerChar}";
            ValueData = Value.value;
        }
        else if (IsType(nodeTerm.term, typeof(NodeTermIdent)))
        {
            if (Value.Type == TokenType.ident && Value.value == Expr_Register)
            {
                ValueData = Expr_Register;
                V_value = ReplacerChar.ToString();
            }
            else if (Value.Type == TokenType.ident && get_StackVariable(Value.value) != null)
            {
                V_value = $"[BP + {ReplacerChar}]";
                ValueData = getArgumentLoc(Value.value).ToString();
            }
            else if (get_Variable(Value.value).HasValue)
            {
                Variable variable = get_Variable(Value.value).Value;
                V_value = $"[{ReplacerChar}]h";
                ValueData = Convert.ToString(Convert.ToInt32(variable.address_loc), 16);
            }
        }
        else if (IsType(nodeTerm.term, typeof(NodeTermCharLit)))
        {
            V_value = $"#{ReplacerChar}h";
            ValueData = Convert.ToString(defualtEncoder.GetBytes(Value.value.Trim('\''))[0], 16);
        }
        else if (IsType(nodeTerm.term, typeof(NodeTermStringLit)))
        {
            string Label = $"_{V_name}";
            m_ROdataoutput.Add($"{Label}:");
            m_ROdataoutput.Add($".str \"{Value.value}\"");
            V_value = $"[{ReplacerChar}]";
            ValueData = Label;
        }
        else
        {
            Console.WriteLine("you messed up");
            Environment.Exit(-1);
        }
        int allocatedMemory;
        string memoryAddress;
        string data;
        ValueData = ValueData.PadLeft(2, '0');
        if (get_scope() == 0)
        {
            // global
            allocatedMemory = allocateVariable();
            for (int memoryOffset = 0; memoryOffset < size; memoryOffset += 2)
            {
                data = ConvertToWord(defualtEncoder.GetBytes(ValueData), memoryOffset);
                V_value = V_value.Replace(ReplacerChar.ToString(), data);
                memoryAddress = Convert.ToString(allocatedMemory + m_var_offset + (memoryOffset / 2), 16);
                m_output.Add($"mov [{memoryAddress}h], {V_value}");
                m_variables[allocatedMemory + (memoryOffset / 2)] = new Variable()
                {
                    address_loc = memoryAddress,
                    name = V_name,
                    UseBP = false,
                    size = size,
                    length = length - (memoryOffset / 2),
                    Pointers_Deep = 0
                };
            }
            return;
        }
        data = ConvertToWord(defualtEncoder.GetBytes(ValueData));
        V_value = V_value.Replace(ReplacerChar.ToString(), data);
        allocatedMemory = m_stack_size;
        memoryAddress = Convert.ToString(allocatedMemory, 16);
        m_output.Add($"mov {Data_Register}, {V_value}");
        pushDR();
        m_stack_variables.Add(new Variable()
        {
            address_loc = memoryAddress,
            name = V_name,
            UseBP = false,
            size = size,
            length = length,
            Pointers_Deep = 0
        });
    }

    public string[] gen_prog(NodeProg nodeProg)
    {
        m_output.Add($"push XL ; sectors");
        m_output.Add($"push XH ; disk");
        m_output.Add($"call [_{Main_Func_name}]");
        m_output.Add($"mov AX, ZX");
        m_output.Add($"int #FBh");
        m_output.Add($"");

        m_variables.Initialize();

        for (int i = 0; i < nodeProg.NodeStmt.Length; i++)
        {
            gen_stmt(nodeProg.NodeStmt[i]);
        }

        return m_output.ToArray();
    }

    bool IsType(object obj1, Type type)
    {
        if(obj1 == null || type == null) return false;
        return obj1.GetType() == type;
    }
    T GetType<T>(object obj)
    {
        if (obj is T)
        {
            return (T)obj;
        }
        else
        {
            throw new InvalidCastException($"Programmer Erorr: Cannot cast {obj.GetType().Name} to type {typeof(T).Name}", -1);
        }
    }

    string ConvertToWord(byte[] bytes, int offset = 0)
    {
        string word = defualtEncoder.GetString(bytes.ToList().GetRange(offset, 2).ToArray()).PadLeft(4, '0');
        return word;
    }

    Variable? get_Variable(string name)
    {
        for (int i = 0; i < m_stack_variables.Count; i++)
        {
            if(name == m_stack_variables[i].name)
            {
                return m_stack_variables[i];
            }
        }
        for (int i = 0; i < m_variables.Length; i++)
        {
            if (name == m_variables[i].name)
            {
                return m_variables[i];
            }
        }
        return null;
    }
    void returnFromFunc()
    {
        m_output.Add($"ret #{m_last_argument_count}");
    }
    int getArgumentLoc(string name)
    {
        return (Convert.ToInt32(get_StackVariable(name), 16) + m_basePointer_offset) - 1;
    }
    int getStackLoc(string name)
    {
        return Convert.ToInt32(get_StackVariable(name), 16) - m_stack_size - 1;
    }
    string get_StackVariable(string name)
    {
        for (int i = 0; i < m_stack_variables.Count; i++)
        {
            if (m_stack_variables[i].name == name)
            {
                return m_stack_variables[i].address_loc;
            }
        }
        return null;
    }

    string get_variable_stack(string name)
    {
        for (int i = 0; i < m_stack_variables.Count; i++)
        {
            if (m_stack_variables[i].name == name)
            {
                if(m_stack_variables[i].UseBP == true)
                {
                    return $"[{Base_Pointer} + {getArgumentLoc(name)}]";
                }
                else
                {
                    return $"[{Stack_Pointer} - {getStackLoc(name)}]";
                }
            }
        }
        return "";
    }

    void set_ReadOnly_Data(string name, int data, int size)
    {
        switchBank(0);
        m_ROdataoutput.Add($".org {Convert.ToString(0xDF0000 + m_read_only_count, 16)}");
        m_ROdataoutput.Add($"_RO_{name}:");
        m_ROdataoutput.Add($".word {data}");
        m_read_only_count += size;
        switchBank(1);
    }

    int get_scope()
    {
        return m_scopes.Count;
    }

    void push(string reg)
    {
        m_output.Add($"push {reg}");
        m_stack_size--;
    }

    void pushDR()
    {
        m_output.Add($"push {Data_Register}");
        m_stack_size--;
    }

    void pop(string reg)
    {
        m_output.Add($"pop {reg}");
        m_stack_size++;
    }

    void pushr()
    {
        m_output.Add($"pushr");
        m_stack_size -= Pushr_length;
    }

    void popr()
    {
        m_output.Add($"popr");
        m_stack_size += Pushr_length;
    }

    void begin_scope()
    {
        m_output.Add($"; scope {m_scopes.Count}");
        m_scopes.Push(allocateVariable());
    }

    void end_scope()
    {
        m_output.Add($"; end scope {m_scopes.Count - 1}");
        if (m_scopes.Count == 1)
        {
            popr();
            pop("BP");
            returnFromFunc();
        }
        int pop_count = allocateVariable() - m_scopes.Last();
        m_stack_size -= pop_count;
        for (int i = 0; i < pop_count; i++)
        {
            m_variables[i].address_loc = null;
        }
        m_scopes.Pop();
    }

    private void switchBank(byte bank)
    {
        if (m_bank != bank)
        {
            m_output.Add($"mov MB, #{bank}");
            m_bank = bank;
        }
    }
    private string create_label()
    {
        return $"label_{m_label_count++}";
    }
    private string create_label_linenumber(NodeStmt stmt)
    {
        return $"label_L{stmt.Token.line}:";
    }
    private string GetTempRegister()
    {
        return $"R{m_temp_register_count}";
    }
}