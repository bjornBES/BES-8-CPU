using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;

namespace assembler
{
    public class Generation
    {
        int m_index;
        List<Label> Labels = new List<Label>();
        List<Variables> variables = new List<Variables>();
        public required Token[] m_src;
        public List<string> MachineCode = new List<string>();

        public const string LabelIdent = "L:";
        int PC = 0;
        string name;
        string[] Instructions = Enum.GetNames(typeof(Instructions));
        Instructions[] InstructionCodes = (Instructions[])Enum.GetValues(typeof(Instructions));

        string[] Registers = Enum.GetNames(typeof(Registers));
        Registers[] RegistersCodes = (Registers[])Enum.GetValues(typeof(Registers));

        public static Dictionary<string, string> ArgumentIdentifier = new Dictionary<string, string>();
        public static Dictionary<string, string> Pattens = new();

        Encoding StdEncoding = Encoding.ASCII;

        public List<string> ParserTokens = new List<string>();

        public void Build(Token[] tokens = null)
        {
            if(tokens != null)
            {
                m_src = tokens;
            }
            Pattens.Clear();
            Pattens.Add("NULL",    "0000");
            Pattens.Add("KW_ENT",  "000D");
            Pattens.Add("KW_ESC",  "001B");
            Pattens.Add("KW_BS",   "0008");
            Pattens.Add("KW_SP",   "0020");

            Pattens.Add("FZ",      "0001");
            Pattens.Add("FQ",      "0002");
            Pattens.Add("FL",      "0004");
            Pattens.Add("FC",      "0008");
            Pattens.Add("FU",      "0010");
            Pattens.Add("FS",      "0020");
            Pattens.Add("FP",      "0040");
            Pattens.Add("FO",      "0080");
            Pattens.Add("FI",      "0100");
            Pattens.Add("FH",      "0200");
            Pattens.Add("FE",      "0400");

            ArgumentIdentifier.Clear();
            ArgumentIdentifier.Add("I", "0");       // #number
            ArgumentIdentifier.Add("A", "1");       // [addr]
            ArgumentIdentifier.Add("R", "2");       // reg  
            ArgumentIdentifier.Add("RA", "3");      // [reg]
            ArgumentIdentifier.Add("IR", "4");      // [addr]&reg
            ArgumentIdentifier.Add("II", "5");      // [addr]&number
            ArgumentIdentifier.Add("IRR", "6");     // [reg]&reg
            ArgumentIdentifier.Add("IRI", "7");     // [reg]&imm
            List<byte> Bytestr;
            string text;
            while (peek() != null)
            {
                switch (peek().Type)
                {
                    case TokenType.NewFile:
                        consume();
                        /*
                        for (int i = 0; i < Labels.Count; i++)
                        {
                            if (Labels[i].IsGlobal == false && Labels[i].IsLocal == true)
                            {
                                Labels.RemoveAt(i);
                            }
                        }
                        */
                        break;
                    case TokenType.ident:
                        Token instruction = consume();
                        if (peek() != null && peek().Type == TokenType.colon)
                        {
                            Labels.Add(new Label()
                            {
                                Name = instruction.Value,
                                IsGlobal = false,
                                IsLocal = true,
                                Addr = PC
                            });
                            ObjFormatter.AddLabel(instruction.Value, Convert.ToString(PC, 16));
                            ParserTokens.Add($"NodeLable = {{ Name = {instruction.Value} addr = {MachineCode.Count} Line = {instruction.Line} }}");
                            consume();
                        }
                        else if (Instructions.Contains(instruction.Value.ToUpper()))
                        {
                            MakeInstruction(instruction);
                        }
                        else
                        {
                            //todo error instr not found
                            AssemblerErrors.ErrorInstructionNotFound(instruction);
                            Console.WriteLine($"{instruction.Value} is not found {instruction.Line}");
                        }
                        break;
                    case TokenType.byte_:
                    case TokenType.word:
                        consume();
                        if(peek().Type == TokenType.hash || peek().Type == TokenType.open_square)
                        {
                            AssemblerErrors.ErrorUnexpectedToken(peek());
                        }
                        List<string> Data = new List<string>();
                        int SavePC = PC;
                        Data.Add(DoArgs(out _));
                        PC++;
                        while (peek() != null && peek().Type == TokenType.comma)
                        {
                            consume();
                            Data.Add(DoArgs(out _));
                            PC++;
                        }
                        MachineCode.InsertRange(MachineCode.Count, Data);
                        ObjFormatter.AddBytes(Data.ToArray());
                        break;
                    case TokenType.extern_:
                        consume();
                        consume();
                        break;
                    case TokenType.bits:
                        consume();
                        consume();
                        break;
                    case TokenType.section:
                        consume();
                        consume();
                        break;
                    case TokenType.repeat:
                        consume();
                        int Times = Convert.ToInt32(ParseExpr(), 16);
                        List<Token> line = new List<Token>();
                        int PLineNumber = peek().Line;
                        while (peek() != null && peek().Line == PLineNumber)
                        {
                            line.Add(consume());
                        }
                        Generation generation = new Generation() { m_src = line.ToArray() };
                        for (int i = 0; i < Times; i++)
                        {
                            generation.Build();
                        }
                        MachineCode.InsertRange(MachineCode.Count, generation.MachineCode);
                        break;
                    case TokenType.org:
                        consume();
                        int offset = Convert.ToInt32(ParseExpr(), 16);
                        ObjFormatter.AddOrg(Convert.ToString(offset, 16));
                        PC = offset;
                        break;
                    case TokenType.str:
                        consume();

                        if (peek().Type != TokenType.quotation)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.quotation);
                        }
                        consume();

                        if (peek().Type != TokenType.ident)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.ident);
                        }
                        text = consume().Value;
                        ObjFormatter.AddStr(text);
                        Bytestr = StdEncoding.GetBytes(text).ToList();

                        if (peek().Type != TokenType.quotation)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.quotation);
                        }
                        consume();

                        while (peek().Type == TokenType.comma)
                        {
                            consume();
                            DoArgs(out _);
                        }
                        for (int i = 0; i < Bytestr.Count; i++)
                        {
                            MachineCode.Add(Convert.ToString(Bytestr[i]).PadLeft(4, '0'));
                        }
                        break;
                    case TokenType.strz:
                        consume();

                        if(peek().Type != TokenType.quotation)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.quotation);
                        }
                        consume();

                        if (peek().Type != TokenType.ident)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.ident);
                        }
                        text = consume().Value;
                        ObjFormatter.AddStr(text + "\0");
                        Bytestr = StdEncoding.GetBytes(text).ToList();

                        if (peek().Type != TokenType.quotation)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.quotation);
                        }
                        consume();

                        while(peek().Type == TokenType.comma)
                        {
                            consume();
                            DoArgs(out _);
                        }
                        Bytestr.Add(00);
                        for (int i = 0; i < Bytestr.Count; i++)
                        {
                            MachineCode.Add(Convert.ToString(Bytestr[i]).PadLeft(4,'0'));
                        }
                        break;
                    case TokenType.include:
                        consume();
                        consume();
                        break;
                    case TokenType.const_:
                        consume();
                        consume();
                        consume();
                        break;
                    case TokenType.global:
                        consume();
                        if (peek().Type != TokenType.ident)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.ident);
                        }
                        name = consume().Value;
                        ParserTokens.Add($"NodeLable = {{ Name = {name} IsGlobal = true addr = {MachineCode.Count} Line = {peek().Line} }}");
                        Labels.Add(new Label()
                        {
                            Name = name,
                            IsGlobal = true,
                            IsLocal = false,
                            Addr = PC
                        });
                        ObjFormatter.AddLabel(name, Convert.ToString(PC, 16));
                        if (peek().Type != TokenType.colon)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.colon);
                        }
                        consume();
                        break;
                    case TokenType.local:
                        consume();
                        if(peek().Type != TokenType.ident)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.ident);
                        }
                        name = consume().Value;
                        ParserTokens.Add($"NodeLable = {{ Name = {name} IsLocal = true addr = {MachineCode.Count} Line = {peek().Line} }}");
                        Labels.Add(new Label()
                        {
                            Name = name,
                            IsGlobal = false,
                            IsLocal = true,
                            Addr = PC
                        });
                        ObjFormatter.AddLabel(name, Convert.ToString(PC, 16));
                        if (peek().Type != TokenType.colon)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.colon);
                        }
                        consume();
                        break;
                    case TokenType.doller:
                        consume();
                        NewVariable();
                        break;
                    default:
                        AssemblerErrors.ErrorUnexpectedToken(peek());
                        break;
                }
            }

            for (int i = 0; i < MachineCode.Count; i++)
            {
                if (MachineCode[i].StartsWith(LabelIdent))
                {
                    string label_Name = MachineCode[i].Replace(LabelIdent, "");
                    Label label = GetLabel(label_Name);
                    if (label != null)
                    {
                        MachineCode[i] = Convert.ToString(label.Addr, 16).PadLeft(4, '0');
                    }
                    else
                    {
                        AssemblerErrors.LabelNotFound(label_Name);
                    }
                }
            }
        }

        private void MakeInstruction(Token instruction)
        {
            List<string> ArgumentBuffer = new List<string>();
            int instructionValue = (int)InstructionCodes[Instructions.ToList().IndexOf(instruction.Value.ToUpper())];
            string Instruction = Convert.ToString(instructionValue, 16).PadLeft(2, '0');
            Instructions Instr = InstructionCodes[instructionValue];
            int ArgumentSize = 0;

            switch (Instr)
            {
                case assembler.Instructions.NOP:    ArgumentSize = 0;   break;
                case assembler.Instructions.MOV:    ArgumentSize = 3;   break;
                case assembler.Instructions.PUSH:   ArgumentSize = 1;   break;
                case assembler.Instructions.POP:    ArgumentSize = 1;   break;
                case assembler.Instructions.ADD:    ArgumentSize = 2;   break;
                case assembler.Instructions.SUB:    ArgumentSize = 2;   break;
                case assembler.Instructions.MUL:    ArgumentSize = 2;   break;
                case assembler.Instructions.DIV:    ArgumentSize = 2;   break;
                case assembler.Instructions.AND:    ArgumentSize = 2;   break;
                case assembler.Instructions.OR:     ArgumentSize = 2;   break;
                case assembler.Instructions.NOT:    ArgumentSize = 1;   break;
                case assembler.Instructions.NOR:    ArgumentSize = 1;   break;
                case assembler.Instructions.ROL:    ArgumentSize = 2;   break;
                case assembler.Instructions.ROR:    ArgumentSize = 2;   break;
                case assembler.Instructions.JMP:    ArgumentSize = 1;   break;
                case assembler.Instructions.CMP:    ArgumentSize = 2;   break;
                case assembler.Instructions.JLE:    ArgumentSize = 1;   break;
                case assembler.Instructions.JE:     ArgumentSize = 1;   break;
                case assembler.Instructions.JGE:    ArgumentSize = 1;   break;
                case assembler.Instructions.JG:     ArgumentSize = 1;   break;
                case assembler.Instructions.JNE:    ArgumentSize = 1;   break;
                case assembler.Instructions.JL:     ArgumentSize = 1;   break;
                case assembler.Instructions.JER:    ArgumentSize = 1;   break;
                case assembler.Instructions.JMC:    ArgumentSize = 1;   break;
                case assembler.Instructions.JMZ:    ArgumentSize = 1;   break;
                case assembler.Instructions.JNC:    ArgumentSize = 1;   break;
                case assembler.Instructions.INT:    ArgumentSize = 1;   break;
                case assembler.Instructions.CALL:   ArgumentSize = 1;   break;
                case assembler.Instructions.RTS:    ArgumentSize = 0;   break;
                case assembler.Instructions.RET:    ArgumentSize = 1;   break;
                case assembler.Instructions.PUSHR:  ArgumentSize = 0;   break;
                case assembler.Instructions.POPR:   ArgumentSize = 0;   break;
                case assembler.Instructions.INC:    ArgumentSize = 1;   break;
                case assembler.Instructions.DEC:    ArgumentSize = 1;   break;
                case assembler.Instructions.IN:     ArgumentSize = 2;   break;
                case assembler.Instructions.OUT:    ArgumentSize = 2;   break;
                case assembler.Instructions.CLF:    ArgumentSize = 1;   break;
                case assembler.Instructions.SEF:    ArgumentSize = 1;   break;
                case assembler.Instructions.XOR:    ArgumentSize = 2;   break;
                case assembler.Instructions.JMS:    ArgumentSize = 1;   break;
                case assembler.Instructions.JNS:    ArgumentSize = 1;   break;
                case assembler.Instructions.SHL:    ArgumentSize = 2;   break;
                case assembler.Instructions.SHR:    ArgumentSize = 2;   break;
                case assembler.Instructions.HALT:   ArgumentSize = 0;   break;
                default:
                    AssemblerErrors.ErrorInstructionNotFound(instruction);
                    break;
            }
            ParserTokens.Add($"NodeExprInstruction = {{Instr = {Instr}, Value = {instructionValue}}}");
            while (true && peek() != null && ArgumentBuffer.Count < ArgumentSize)
            {
                if(ArgumentBuffer.Count > 0)
                {
                    if(peek().Type == TokenType.comma)
                    {
                        consume(); 
                    }
                    else
                    {
                        AssemblerErrors.ErrorExpectedToken(peek(), TokenType.comma);
                    }
                }
                switch (peek().Type)
                {
                    case TokenType.doller:
                    case TokenType.int_lit:
                    case TokenType.ident:
                    case TokenType.Hex_int_lit:
                    case TokenType.Bin_int_lit:
                    case TokenType.hash:
                    case TokenType.comma:
                    case TokenType.open_square:
                    case TokenType.Percent:
                        ArgumentBuffer.Add(DoArgs(out string ArgumentIdent));
                        Instruction += ArgumentIdent;
                        break;
                    default:
                        Instruction += "F";
                        break;
                }
                if(Instruction.Length == 4)
                {
                    break;
                }
            }
            PC++;
            Instruction = Instruction.PadRight(4, 'F');
            ObjFormatter.AddInstr(Instruction, ArgumentBuffer.ToArray());
            MachineCode.Add(Instruction);
            MachineCode.InsertRange(MachineCode.Count, ArgumentBuffer);
        }

        private string DoArgs(out string ArgumentIdent)
        {
            string ArgumentBuffer = "";
            int RegisterIndex;
            int RegisterValue;
            ArgumentIdent = "";
            string Value = "";
            switch (peek().Type)
            {
                case TokenType.doller:
                    ParserTokens.Add($"NodeExprAddress = {{NoteExpr = {{Value = 0x{Convert.ToString(PC, 16)}}}}}");
                    consume();
                    ArgumentBuffer = Convert.ToString(PC + 1, 16).PadLeft(4, '0');
                    ArgumentIdent += ArgumentIdentifier["A"];
                    break;
                case TokenType.int_lit:
                case TokenType.ident:
                case TokenType.Hex_int_lit:
                case TokenType.Bin_int_lit:
                    Value = ParseExpr();
                    if (Registers.Contains(Value))
                    {
                        RegisterIndex = Registers.ToList().IndexOf(Value);
                        RegisterValue = (int)RegistersCodes[RegisterIndex];
                        ParserTokens.Add($"NoteExprRegister = {{Value = {RegisterValue}}}");
                        ArgumentBuffer = Convert.ToString(RegisterValue, 16).PadLeft(4, '0');
                        ArgumentIdent += ArgumentIdentifier["R"];
                    }
                    else
                    {
                        ParserTokens.Add($"NoteExpr = {{Value = 0x{Value}}}");
                        ArgumentIdent += ArgumentIdentifier["I"];
                        ArgumentBuffer = Value;
                    }
                    PC++;
                    break;
                case TokenType.hash:
                    consume();
                    Value = ParseExpr();
                    ArgumentIdent += ArgumentIdentifier["I"];
                    if (Pattens.ContainsKey(Value.ToUpper()))
                    {
                        ParserTokens.Add($"NoteExpr = {{Value = 0x{Pattens[Value]}}}");
                        ArgumentBuffer = Pattens[Value].PadLeft(4, '0');
                    }
                    else
                    {
                        ParserTokens.Add($"NoteExpr = {{Value = 0x{Value}}}");
                        ArgumentBuffer = Value.PadLeft(4, '0');
                    }
                    PC++;
                    break;
                case TokenType.open_square:
                    consume();
                    Value = ParseExpr();
                    if (peek().Type != TokenType.close_square)
                    {
                        AssemblerErrors.ErrorExpectedToken(peek(), TokenType.close_square);
                    }
                    PC++;
                    consume();
                    if (Registers.Contains(Value))
                    {
                        if (peek().Type == TokenType.ampersand)
                        {
                            consume();
                            PC++;
                            // 2 x register addr indexed
                            ArgumentBuffer = Value; // base address

                            Value = ParseExpr();
                            if (Registers.Contains(Value))
                            {
                                ArgumentIdent += ArgumentIdentifier["IRR"];
                                RegisterIndex = Registers.ToList().IndexOf(Value);
                                RegisterValue = (int)RegistersCodes[RegisterIndex];
                                ParserTokens.Add($"NodeExprIndexedAddress = {{Address = NodeExprRegister{{{Value}}}, Indexed = NodeExprRegister{{0x{RegisterValue}, Ident = {Value}}}}}");
                                ArgumentBuffer = Convert.ToString(RegisterValue, 16).PadLeft(4, '0');
                            }
                            else
                            {
                                ParserTokens.Add($"NodeExprIndexedAddress = {{Address = NodeExprRegister{{{Value}}}, Indexed = 0x{Value}}}");
                                ArgumentIdent += ArgumentIdentifier["IRI"];
                                ArgumentBuffer = Value;
                            }
                            break;
                        }
                        // register addr
                        RegisterIndex = Registers.ToList().IndexOf(Value);
                        RegisterValue = (int)RegistersCodes[RegisterIndex];
                        ParserTokens.Add($"NodeExprAddress = {{Address = NodeExprRegister {{0x{RegisterValue}, Ident = {Value}}}}}");
                        ArgumentBuffer = Convert.ToString(RegisterValue, 16).PadLeft(4, '0');
                        ArgumentIdent += ArgumentIdentifier["RA"];
                    }
                    else
                    {
                        if (peek().Type == TokenType.ampersand)
                        {
                            consume();
                            PC++;
                            // 2 x addr indexed
                            ArgumentBuffer = Value; // base address

                            Value = ParseExpr();
                            if (Registers.Contains(Value))
                            {
                                ArgumentIdent += ArgumentIdentifier["IR"];
                                RegisterIndex = Registers.ToList().IndexOf(Value);
                                RegisterValue = (int)RegistersCodes[RegisterIndex];
                                ParserTokens.Add($"NodeExprIndexedAddress = {{Address = {Value}, Indexed = NodeExprRegister{{0x{RegisterValue}, Ident = {Value}}}}}");
                                ArgumentBuffer = Convert.ToString(RegisterValue, 16).PadLeft(4, '0');
                            }
                            else
                            {
                                ParserTokens.Add($"NodeExprIndexedAddress = {{Address = {Value}, Indexed = 0x{Value}}}");
                                ArgumentIdent += ArgumentIdentifier["II"];
                                ArgumentBuffer = Value;
                            }
                            break;
                        }
                        else
                        {
                            if (IsLetter(Value))
                            {
                                Value = LabelIdent + Value;
                            }
                            ParserTokens.Add($"NodeExprAddress = {{Address = {Value}}}");
                            ArgumentIdent += ArgumentIdentifier["A"];
                            ArgumentBuffer = Value;
                        }
                    }
                    break;
                case TokenType.Percent:
                    consume();
                    if (peek().Type == TokenType.ident)
                    {
                        Value = ParseExpr();
                        if (IsVariable(Value))
                        {
                            Variables variables = GetVariable(Value);
                            if (variables.IsImm)
                            {
                                ParserTokens.Add($"NodeExpr = {{Value = 0x{variables.Value}}}");
                                ArgumentIdent += ArgumentIdentifier["I"];
                            }
                            else if (variables.IsPointer)
                            {
                                ParserTokens.Add($"NodeExprAddress = {{Address = 0x{variables.Value}}}");
                                ArgumentIdent += ArgumentIdentifier["A"];
                            }
                            else
                            {
                                ParserTokens.Add($"NodeExprAddress = {{Address = 0x{variables.Value}}}");
                                ArgumentIdent += ArgumentIdentifier["A"];
                            }
                            PC++;
                            ArgumentBuffer = variables.Value;
                        }
                    }
                    break;
                default:
                    break;
            }
            return ArgumentBuffer;
        }

        private bool isLabel(string name)
        {
            for (int i = 0; i < Labels.Count; i++)
            {
                if (name == Labels[i].Name)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsVariable(string name)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if(name == variables[i].Name)
                {
                    return true;
                }
            }
            return false;
        }
        private Variables GetVariable(string name)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (name == variables[i].Name)
                {
                    return variables[i];
                }
            }
            return null;
        }
        private Label GetLabel(string name)
        {
            for (int i = 0; i < Labels.Count; i++)
            {
                if (name == Labels[i].Name)
                {
                    return Labels[i];
                }
            }
            return null;
        }

        private void NewVariable()
        {
            if (peek().Type != TokenType.ident)
            {
                AssemblerErrors.ErrorExpectedToken(peek(), TokenType.ident);
            }
            string name = consume().Value;

            // variable Erorr cheaking
            if ((peek().Type == TokenType.hash && peek(1).Type != TokenType.eq) || peek().Type == TokenType.open_square)
            {
                AssemblerErrors.ErrorUnexpectedToken(peek());
            }

            if (peek().Type == TokenType.hash && peek(1).Type == TokenType.eq)
            {
                //#=
                consume();
                consume();
                string value = ParseExpr();
                ParserTokens.Add($"NodeVariableExpr = {{ Name = {name} operator = \"#=\" value = {value}h Line = {peek(-1).Line} }}");
                variables.Add(new Variables()
                {
                    IsImm = true,
                    IsPointer = false,
                    Name = name,
                    Value = value
                });
            }
            else if (peek().Type == TokenType.star && peek(1).Type == TokenType.eq)
            {
                //*=
                consume();
                consume();
                string value = ParseExpr();
                ParserTokens.Add($"NodeVariableExpr = {{ Name = {name} operator = \"*=\" value = {value}h Line = {peek(-1).Line} }}");
                variables.Add(new Variables()
                {
                    IsImm = false,
                    IsPointer = true,
                    Name = name,
                    Value = value
                });
            }
            else if (peek(0).Type == TokenType.eq)
            {
                //=
                consume();
                string value = ParseExpr();
                ParserTokens.Add($"NodeVariableExpr = {{ Name = {name} operator = \"=\" value = {value}h Line = {peek(-1).Line} }}");
                variables.Add(new Variables()
                {
                    IsImm = false,
                    IsPointer = false,
                    Name = name,
                    Value = value
                });
            }
        }

        private string ParseExpr()
        {
            string value;
            if(peek() == null)
            {
                return "NULLPARSEEXPR";
            }
            switch (peek().Type)
            {
                case TokenType.int_lit:
                    value = consume().Value;
                    if(peek() != null && peek().Type == TokenType.ident && peek().Value == "h")
                    {
                        int DecValue = Convert.ToUInt16(value, 16);
                        if(DecValue > 0xFFFF)
                        {
                            AssemblerErrors.ErrorBitSize(peek());
                        }
                        consume();
                        return value;
                    }
                    else if (peek() != null && peek().Type == TokenType.ident && peek().Value == "b")
                    {
                        int DecValue = Convert.ToUInt16(value, 2);
                        if (DecValue > 0xFFFF)
                        {
                            AssemblerErrors.ErrorBitSize(peek());
                        }
                        consume();
                        return Convert.ToString(Convert.ToUInt32(value, 2), 16);
                    }
                    else
                    {
                        int DecValue = Convert.ToUInt16(value);
                        if (DecValue > 0xFFFF)
                        {
                            AssemblerErrors.ErrorBitSize(peek());
                        }
                        return Convert.ToString(Convert.ToUInt32(value, 10), 16);
                    }
                case TokenType.ident:
                    value = consume().Value;
                    if(Instructions.Contains(value.ToUpper())) return value;
                    if (value.EndsWith('h'))
                    {
                        value = value.TrimEnd('h');
                        int DecValue = Convert.ToUInt16(value, 16);
                        if (DecValue > 0xFFFF)
                        {
                            AssemblerErrors.ErrorBitSize(peek());
                        }
                        return value;
                    }
                    else if (value.EndsWith('b'))
                    {
                        value = value.TrimEnd('b');
                        int DecValue = Convert.ToUInt16(value, 2);
                        if (DecValue > 0xFFFF)
                        {
                            AssemblerErrors.ErrorBitSize(peek());
                        }
                        return Convert.ToString(Convert.ToUInt32(value, 2), 16);
                    }
                    else
                    {
                        if (char.IsDigit(value, 0))
                        {
                            int DecValue = Convert.ToUInt16(value);
                            if (DecValue > 0xFFFF)
                            {
                                AssemblerErrors.ErrorBitSize(peek());
                            }
                            return Convert.ToString(Convert.ToUInt32(value, 10), 16);
                        }
                        else
                        {
                            return value;
                        }
                    }
                case TokenType.Hex_int_lit:
                    consume();
                    return ParseExpr();
                case TokenType.Bin_int_lit:
                    consume();
                    return ParseExpr();
                case TokenType.apostrophe:
                    consume();
                    string C = "\0\0";
                    if (peek().Type == TokenType.ident)
                    {
                        C = consume().Value;
                        if(peek().Type != TokenType.apostrophe)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.apostrophe);
                        }
                        if(C.Length > 1)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.ident);
                        }
                        if (C.Length < 1)
                        {
                            AssemblerErrors.ErrorExpectedToken(peek(), TokenType.ident);
                        }
                        consume();
                    }
                    return Convert.ToString(StdEncoding.GetBytes(C)[0], 16);
                default:
                    return "NULLPARSEEXPR";
            }
        }

        bool IsLetter(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!char.IsLetter(str[i]) && str[i] != '_') return false;
            }
            return true;
        }

        Token peek(int offset = 0)
        {
            if (m_index + offset >= m_src.Length)
            {
                return null;
            }
            return m_src[m_index + offset];
        }

        Token consume()
        {
            return m_src[m_index++];
        }
    }
}
