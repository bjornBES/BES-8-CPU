using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Compiler
{
    public class Interpreter
    {
        /// <summary>
        /// this space is for Variables
        /// </summary>
        public const uint VariableAddrOffset = 0x10500;
        public uint VariableAddrCounter = 0;

        public bool UsingPorts = false;

        List<Variables> variables = new();

        public const string Comment = "//";

        const string MainFuncName = "Main";

        bool hasMainfunc = false;
        List<string> Funcs = new List<string>();
        List<string> Names = new List<string>();
        int LabelJumpCounter;
        string LabelJumpStr = "LableJump_";

        public List<string> Src;
        public List<string> AsmSrc = new();
        public void Build(CompilerSettings settings, string[] src)
        {
            string StartCode =
                $"call [{MainFuncName}]\n" +
                "mov AX #20h\n" +
                "int #10h\n" +
                "halt #0\n";
            AsmSrc.Add(StartCode);

            Src = src.ToList();

            RemoveAllComment();
            RemoveAllWhiteSpaces();

            UnPackMacros();

            for (int i = 1; i < Src.Count; i++)
            {
                //Console.WriteLine($"DEBUG INFO {Src[i]} {i}");
                BuildCommand(Src[i], i, src);

                Allocate();

                AsmSrc.Add("");
            }

            if (hasMainfunc == false)
            {
                // todo error has no Main func
            }

            if (UsingPorts)
                AsmSrc.Add($".include Libs/Ports.Basm");
        }

        bool InComment = false;
        int Level = 0;
        void BuildCommand(string Command, int index, string[] src)
        {
            int LineNumber = index;
            string Instr = Command.Split(' ', 2)[0];
            string[] Args = Array.Empty<string>();
            if (Command.Split(' ', 2).Length >= 2) Args = Command.Split(' ', 2)[1].Split(' ');

            if (string.IsNullOrEmpty(src[index])) return;

            if (!InComment)
            {
                switch (Instr)
                {
                    case "func":
                        string Name = Args[0].Split('(')[0];

                        CheakForSameName(Name, index, src, LineNumber);

                        Names.Add(Name);

                        if (Name == MainFuncName) hasMainfunc = true;
                        if (Args.Length == 1)
                        {
                            // 0 args
                            Funcs.Add(Name);
                            Name += ":";
                            AsmSrc.Add($"; func {Name.Trim(':')} with {Args.Length - 1} args");
                            AsmSrc.Add($"; no values beening pushed at {LineNumber}");
                            AsmSrc.Add(Name);
                            AsmSrc.Add($"pushr");
                        }
                        else if (Args.Length > 1)
                        {
                            // X args todo
                            Funcs.Add(Name);
                            Name += ":";
                            AsmSrc.Add(Name);
                            AsmSrc.Add($"pushr");
                            AsmSrc.Add($"mov ZX SP");
                            AsmSrc.Add($"add SP #8");
                            AsmSrc.Add($"pop X");
                            AsmSrc.Add($"mov SP ZX");
                        }
                        else
                        {
                            // TODO error
                            Environment.Exit(1);
                        }
                        return;
                    case "class":

                        return;

                    case "ptr":

                        return;

                    case "inc":
                        if(Args.Length > 1)
                        {
                        }
                        return;

                    case "byte":
                        if (Args.Length > 1)
                        {
                            string VariableName = Args[0];


                            CheakForSameName(VariableName, index, src, LineNumber);

                            Names.Add(VariableName);
                            string Value = "";
                            if (Args.Length > 2)
                            {
                                if (Args[1] == "=")
                                {
                                    Value = Args[2];
                                }
                                else
                                {
                                    // TODO ERROR
                                    Environment.Exit(1);
                                }
                            }
                            variables.Add(new Variables()
                            {
                                Name = VariableName,
                                Value = Value,
                                Size = 1
                            });
                            AsmSrc.Add($"; Variable at {LineNumber} Name = {VariableName} and the value is {Value}");
                        }
                        return;
                }
                // not keywords
                switch (Instr)
                {
                    case "{":
                        Level++;
                        //AsmSrc.Add($"; func start here at {LineNumber}");
                        return;
                    case "}":
                        if(Level == 1)
                        {
                            AsmSrc.Add($"popr");
                            AsmSrc.Add($"rts");
                        }
                        if(Level > 0 )
                        {
                            AsmSrc.Add($"{LabelJumpStr}{LabelJumpCounter}:");
                            LabelJumpCounter++;
                        }
                        Level--;
                        //AsmSrc.Add($"; func ends here at {LineNumber}");
                        return;
                }
            }
            // funcs
            if (Instr.Contains("exit"))
            {
                string Value;

                if (Instr.Contains(' '))
                {
                    Value = Instr.Split(' ', 2)[1];
                    Value = Value.TrimStart('(');
                    Value = Value.TrimEnd(')');
                }
                else
                {
                    Value = Instr.Split('(', 2)[1];
                    Value = Value.TrimEnd(')');
                }

                Variables variables = IsVariable(Value);
                if (variables != null)
                {
                    AsmSrc.Add($"; Exit with variable {variables.Name}");
                    AsmSrc.Add($"push MB");
                    AsmSrc.Add($"mov MB #3h");
                    AsmSrc.Add($"mov BX [{Convert.ToString(variables.Addr, 16)}h]");
                    AsmSrc.Add($"pop MB");
                    for (int i = 0; i < Level - 1; i++)
                    {
                        AsmSrc.Add("rts");
                    }
                    AsmSrc.Add($"popr");
                    AsmSrc.Add($"rts");
                }
            }
            else if (Instr.Contains("port"))
            {
                UsingPorts = true;
                AsmSrc.Add($"; {Src[index]}");

                string PreFuncArgs = Instr.Split('.')[1];


                for (int i = 0; i < Args.Length; i++)
                {
                    PreFuncArgs += Args[i];
                }

                string FuncName = PreFuncArgs.Split('(')[0];

                PreFuncArgs = PreFuncArgs.Replace(FuncName, "");
                PreFuncArgs = PreFuncArgs.TrimEnd(')');
                PreFuncArgs = PreFuncArgs.TrimStart('(');

                string[] funcArgs = PreFuncArgs.Split(",");

                bool RightUseVariabel = false;
                bool LeftUseVariabel = false;

                for (int i = funcArgs.Length - 1; i + 1 > 0; i--)
                {
                    if (IsVariable(funcArgs[i]) != null)
                    {
                        if (i == 0) LeftUseVariabel = true;
                        if (i == 1) RightUseVariabel = true;
                        Variables variables = IsVariable(funcArgs[i]);
                        funcArgs[i] = "" + variables.Addr;
                    }
                    else
                    {
                        funcArgs[i] = funcArgs[i];
                    }
                }

                switch (FuncName)
                {
                    case "Out":
                        AsmSrc.Add($"; Calling the Out Func from the Ports Rigth {RightUseVariabel} Left {LeftUseVariabel}");
                        if(RightUseVariabel == true)
                        {
                            AsmSrc.Add($"mov MB #2");
                            AsmSrc.Add($"push [{funcArgs[1]}]");
                            AsmSrc.Add($"mov MB #0");
                        }
                        else
                        {
                            AsmSrc.Add($"push #{funcArgs[1]}");
                        }
                        if (LeftUseVariabel == true)
                        {
                            AsmSrc.Add($"mov MB #2");
                            AsmSrc.Add($"push [{funcArgs[0]}]");
                            AsmSrc.Add($"mov MB #0");
                        }
                        else
                        {
                            AsmSrc.Add($"push #{funcArgs[0]}");
                        }
                        AsmSrc.Add($"call [Func_Port_Out]");
                        AsmSrc.Add($"add SP #2");
                        break;
                    default:
                        break;
                }
            }
            else if (Instr.StartsWith("free"))
            {
                AsmSrc.Add($"; {Src[index]}");
                string PreExpr = "";

                if (Instr.Contains('('))
                {
                    PreExpr += Instr.Trim(')').Split('(')[1] + " ";
                }

                int i;
                for (i = 0; i < Args.Length; i++)
                {
                    PreExpr += Args[i].TrimEnd(')').TrimStart('(') + " ";
                }

                AsmSrc.Add($"; {PreExpr}");

                string[] expr = PreExpr.Split(' ');
                if (expr[0].StartsWith('&'))
                {
                    if (IsVariable(expr[0]) != null)
                    {
                        Variables variables = IsVariable(expr[0]);
                        AsmSrc.Add($"mov [{variables.Addr}] #0");
                        FreeVariable(variables);
                    }
                }
            }
            else if (Instr.StartsWith("if"))
            {
                AsmSrc.Add($"; {Src[index]}");
                string PreExpr = "";

                if (Instr.Contains('('))
                {
                    PreExpr += Instr.Split('(')[1] + " ";
                }

                int i;
                for (i = 0; i < Args.Length; i++)
                {
                    PreExpr += Args[i].Trim(')').TrimStart('(') + " ";
                }

                string[] expr = PreExpr.Split(' ');

                if (IsVariable(expr[0]) != null)
                {
                    Variables variables = IsVariable(expr[0]);
                    AsmSrc.Add($"mov AX [{variables.Addr}]");
                }
                else
                {
                    AsmSrc.Add($"mov AX #{expr[0]}");
                }

                if (IsVariable(expr[2]) != null)
                {
                    Variables variables = IsVariable(expr[2]);
                    AsmSrc.Add($"mov BX [{variables.Addr}]");
                }
                else
                {
                    AsmSrc.Add($"mov BX #{expr[2]}");
                }

                int level = 0;
                int labelcounter = LabelJumpCounter;
                for (i = index + 2; i < src.Length; i++)
                {
                    if (src[i].Contains("{"))
                    {
                        labelcounter++;
                        level++;
                    }
                    else if (src[i].Contains("}"))
                    {
                        if (level != 0) level--;
                        else
                        {
                            break;
                        }
                    }
                }

                AsmSrc.Add("cmp AX BX");
                if (expr[1] == "==")
                {
                    AsmSrc.Add($"jne [{LabelJumpStr}{labelcounter}]");
                }
                else if (expr[1] == "!=")
                {
                    AsmSrc.Add($"jme [{LabelJumpStr}{labelcounter}]");
                }
                else if (expr[1] == "<")
                {
                    AsmSrc.Add($"jmg [{LabelJumpStr}{labelcounter}]");
                }
                else if (expr[1] == ">")
                {
                    AsmSrc.Add($"jml [{LabelJumpStr}{labelcounter}]");
                }
                else if (expr[1] == "<=")
                {
                    AsmSrc.Add($"jml [{LabelJumpStr}{labelcounter}]");
                    AsmSrc.Add($"jne [{LabelJumpStr}{labelcounter}]");
                }
                else if (expr[1] == ">=")
                {
                    AsmSrc.Add($"jmg [{LabelJumpStr}{labelcounter}]");
                    AsmSrc.Add($"jne [{LabelJumpStr}{labelcounter}]");
                }
            }
            else if (Instr.StartsWith("//"))
            {
                return;
            }
            else if (Instr.Contains("/*")) InComment = true;
            else if (Instr.Contains("*/")) InComment = false;
            else if (IsVariable(Instr) != null)
            {
                AsmSrc.Add($"; {Src[index]} expr");


                Variables variables = IsVariable(Instr);
                if (Args[0] == "=")
                {
                    //Console.WriteLine($"DEBUG {Src[index]} {Args.Length}");
                    if (Args[1] == variables.Name && Args.Length > 3 && (Args[2] == "+" || Args[2] == "-" || Args[2] == "*" || Args[2] == "/"))
                    {
                        List<string> Temp = Args.ToList();
                        Temp.RemoveAt(0);
                        Args = Temp.ToArray();
                        AsmSrc.Add("; EXPR");
                        CompileExpression(Args, false);
                    }
                    else if (Args[1] == variables.Name && Args.Length < 4)
                    {
                        AsmSrc.Add($"mov [{variables.Addr}] [{variables.Addr}]");
                    }
                    else if (Args.Length < 5)
                    {
                        // set
                        if (IsVariable(Args[1]) != null)
                        {
                            Variables ToVar = IsVariable(Args[1]);
                            AsmSrc.Add($"mov [{variables.Addr}] [{ToVar.Addr}]");
                            NewValueVariable(variables.Name, ToVar.Value);
                        }
                        else
                        {
                            AsmSrc.Add($"mov [{variables.Addr}] #{Args[1]}");
                            NewValueVariable(variables.Name, Args[1]);
                        }
                    }
                }
            }
            else if (InComment) return;
            else
            {
                CompilerErrors.SyntaxError("", Src.ToArray(), index, LineNumber);
            }
        }

        private void FreeVariable(Variables variable)
        {
            if(variables.Contains(variable))
            {
                variables.Remove(variable);
            }
        }

        private List<Token> Tokenize(string[] expression)
        {
            List<Token> tokens = new List<Token>();

            foreach (var element in expression)
            {
                if (int.TryParse(element, out _))
                {
                    tokens.Add(new Token(TokenType.Number, element));
                }
                else if (element == "+" || element == "-" || element == "*" || element == "/")
                {
                    tokens.Add(new Token(TokenType.Operator, element));
                }
                else if (element == "(")
                {
                    tokens.Add(new Token(TokenType.LeftParenthesis, element));
                }
                else if (element == ")")
                {
                    tokens.Add(new Token(TokenType.RightParenthesis, element));
                }
                else
                {
                    // Treat anything else as a variable
                    tokens.Add(new Token(TokenType.Variable, element));
                }
            }

            return tokens;
        }

        private void GenerateAssemblyCode(List<Token> tokens, bool debug)
        {
            Stack<string> stack = new Stack<string>();

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                if (debug)
                Console.WriteLine(i + " " + token.Type + " " + token.Value);
                switch (token.Type)
                {
                    case TokenType.Number:
                        stack.Push($"mov AX #{token.Value}");
                        break;

                    case TokenType.Variable:
                        // Assuming variables are memory locations, adjust as needed
                        stack.Push($"mov AX [{IsVariable(token.Value).Addr}]");
                        break;

                    case TokenType.Operator:
                        if (i + 1 < tokens.Count)
                        {
                            Token nextToken = tokens[i + 1];

                            if (nextToken.Type == TokenType.Number || nextToken.Type == TokenType.Variable)
                            {
                                // Push the value of the next token onto the stack
                                stack.Push(nextToken.Type == TokenType.Number
                                    ? $"mov AX #{nextToken.Value}"
                                    : $"mov AX &{(IsVariable(nextToken.Value) == null ? $"NULL" : $"{ IsVariable(nextToken.Value).Addr}")}");
                            }
                            else
                            {
                                throw new InvalidOperationException($"Invalid token following the operator {token.Value}.");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Missing operand after the operator {token.Value}.");
                        }

                        if (stack.Count < 2)
                        {
                            throw new InvalidOperationException($"Not enough operands for the operator {token.Value}.");
                        }

                        // Splitting the code into sections
                        if (debug)
                        {
                            Console.WriteLine($"Processing {token.Value} operator...");
                            Console.WriteLine($"Operand 1: {stack.Peek()}");
                            AsmSrc.Add($"; Operand 1: {stack.Peek()}");
                        }
                        operationCode += $"{stack.Pop()}\n";
                        operationCode += "push AX\n";
                        if (debug)
                        {
                            Console.WriteLine($"Operand 2: {stack.Peek()}");
                            AsmSrc.Add($"; Operand 2: {stack.Peek()}");
                        }

                        if(token.Value == "+")
                        {
                            operationCode += $"{stack.Pop()}\nadd [SP] AX\n";
                        }
                        else if (token.Value == "-")
                        {
                            operationCode += $"{stack.Pop()}\nsub [SP] AX\n";
                        }
                        else if (token.Value == "/")
                        {
                            operationCode += $"{stack.Pop()}\ndiv [SP] AX\n";
                        }
                        else if (token.Value == "*")
                        {
                            operationCode += $"{stack.Pop()}\nmul [SP] AX\n";
                        }
                        else
                        {
                            //todo error
                        }
                        operationCode += "pop AX\n";

                        if (debug)
                        {
                            AsmSrc.Add($"; {token.Value} operation");
                            AsmSrc.Add(operationCode);
                        }
                        break;

                    case TokenType.LeftParenthesis:
                        stack.Push("push AX");
                        break;

                    case TokenType.RightParenthesis:
                        if (stack.Count < 1)
                        {
                            throw new InvalidOperationException("Mismatched parentheses.");
                        }

                        string storedRegister = stack.Pop();
                        string popCode = $"{storedRegister}\npop AX";
                        stack.Push(popCode);

                        if (debug)
                        {
                            AsmSrc.Add("; Right Parenthesis");
                            AsmSrc.Add(popCode);
                        }
                        break;
                }
            }


            if (stack.Count != 1)
            {
                throw new InvalidOperationException("Invalid expression.");
            }

            // Move the final result to AX register
            string finalResultCode = stack.Pop();
            AsmSrc.Add(finalResultCode);

            if (debug)
            {
                AsmSrc.Add("; Final Result");
                AsmSrc.Add(finalResultCode);
            }
        }

        private string operationCode = "";  // Added to store the intermediate operation code

        public void CompileExpression(string[] expression, bool debug = false)
        {
            // Push all registers onto the stack
            AsmSrc.Add("push AX");
            AsmSrc.Add("push BX");
            AsmSrc.Add("push CX");
            AsmSrc.Add("push DX");
            AsmSrc.Add("push X");
            AsmSrc.Add("push Y");

            // Tokenize the expression
            List<Token> tokens = Tokenize(expression);

            // Generate assembly code
            GenerateAssemblyCode(tokens, debug);

            // Pop all registers from the stack
            AsmSrc.Add("pop Y");
            AsmSrc.Add("pop X");
            AsmSrc.Add("pop DX");
            AsmSrc.Add("pop CX");
            AsmSrc.Add("pop BX");
            AsmSrc.Add("pop AX");

        }

        private void CheakForSameName(string name, int index, string[] src, int linenumber)
        {
            for (int i = 0; i < Names.Count; i++)
            {
                if (Names[i] == name)
                {
                    CompilerErrors.ErrorSameName("", src, index, linenumber, name);
                }
            }
        }

        Variables IsVariable(string Name)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                //Console.WriteLine($"Index: {i} there is {variables[i].Name} == {Name}");
                if (variables[i].Name == Name.TrimStart('&'))
                {
                    return variables[i];
                }
            }

            return null;
        }
        void NewValueVariable(string Name, string value)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Name == Name) variables[i].Value = value;
            }
        }

        void UnPackMacros()
        {
            for (int i = 0; i < Src.Count; i++)
            {
                if (Src[i].Contains("++"))
                {
                    string name = Src[i].Split("++")[0];
                    Src[i] = $"{name} = {name} + 1";
                }
                if (Src[i].Contains("--"))
                {
                    string name = Src[i].Split("--")[0];
                    Src[i] = $"{name} = {name} - 1";
                }
            }
        }

        void RemoveAllComment()
        {
            RemoveAllWhiteSpaces();
            for (int i = 0; i < Src.Count; i++)
            {
                if (Src[i].StartsWith(Comment))
                {

                }
                else if (Src[i].Contains(Comment))
                {
                    int CommentStart = Src[i].IndexOf(Comment);
                    int Leng = Src[i].Length - CommentStart;
                    Src[i] = Src[i].Remove(CommentStart, Leng);
                }
            }
        }

        void RemoveAllWhiteSpaces()
        {
            for (int i = 0; i < Src.Count; i++)
            {
                for (int a = 0; a < 10; a++)
                {
                    Src[i] = Src[i].TrimStart();
                    Src[i] = Src[i].TrimEnd();
                }
            }
        }

        void Allocate()
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].IsAllocated == false)
                {
                    string HexAddr = Convert.ToString(VariableAddrOffset + VariableAddrCounter, 16);
                    string HexValue = Convert.ToString(Convert.ToInt32(variables[i].Value), 16);
                    AsmSrc.Add($"; allocating {variables[i].Name} at " +
                        $"{HexAddr} with {HexValue}");

                    AsmSrc.Add($"; moving the variable to the addr {HexValue}");
                    AsmSrc.Add($"push MB");
                    AsmSrc.Add($"mov MB #2");
                    AsmSrc.Add($"mov [{HexAddr}h] #{HexValue}h");
                    AsmSrc.Add($"pop MB");

                    variables[i].Addr = VariableAddrOffset + VariableAddrCounter;
                    variables[i].IsAllocated = true;
                    VariableAddrCounter += (uint)variables[i].Size;
                }
            }
        }
    }
}
