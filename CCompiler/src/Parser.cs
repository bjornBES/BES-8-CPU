using Ccompiler.nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Ccompiler
{
    public class Parser
    {
        List<NodeStmt> m_output = new List<NodeStmt>();

        NodeExpr parse_term(bool InBinExpr = false)
        {
            NodeTerm result = new NodeTerm();
            if (peek().HasValue && peek().Value.Type == TokenType.int_lit)
            {
                Token int_lit = consume();
                if (int_lit.Type == TokenType.none)
                {
                    // todo error expected expression
                    throw new Exception("expected expression");
                }
                result.term = new NodeTermIntLit() { int_lit = int_lit };
                return new NodeExpr() { var = result };
            }
            else if (InBinExpr == false && peek(1).HasValue && peek(1).Value.BinPrec().HasValue)
            {
                NodeExpr expr = parse_expr();
                if (expr == null)
                {
                    // todo error
                    throw new Exception("expected expression");
                }
                return expr;
            }
            else if (peek().HasValue && peek().Value.Type == TokenType.ident)
            {
                Token ident = consume();
                if (ident.Type == TokenType.none)
                {
                    // todo error expected expression
                    throw new Exception("expected expression");
                }
                result.term = new NodeTermIdent() { ident = ident };
                return new NodeExpr() { var = result };
            }
            else if (peek().HasValue && peek().Value.Type == TokenType.open_paren)
            {
                consume();
                NodeExpr expr = parse_expr();
                if(expr == null)
                {
                    // todo error
                    throw new Exception("expected expression");
                }
                try_consume_err(TokenType.close_paren);
                result.term = new NodeTermParen()
                {
                    expr = expr,
                };
                return new NodeExpr() { var = result };
            }
            else if (peek().HasValue && peek().Value.Type == TokenType.single_quotes)
            {
                consume();
                if(!peek().HasValue || peek().Value.Type != TokenType.ident)
                {
                    // error
                    throw new Exception("expected expression");
                }
                result.term = new NodeTermCharLit() { C = consume() };
                try_consume_err(TokenType.single_quotes);
                return new NodeExpr() { var = result };
            }
            else if (peek().HasValue && peek().Value.Type == TokenType.double_quotes)
            {
                consume();
                if (!peek().HasValue || peek().Value.Type != TokenType.ident)
                {
                    // error
                    throw new Exception("expected expression");
                }
                result.term = new NodeTermStringLit() { str = consume() };
                try_consume_err(TokenType.double_quotes);
                return new NodeExpr() { var = result };
            }
            return null;
        }
        NodeExpr parse_expr(int min_prec = 0)
        {
            NodeExpr expr_lhs = parse_term(true); 
            if(expr_lhs == null )
            {
                return null;
            }

            while (true)
            {
                Token? curr_tok = peek().Value;
                int? prec = null;
                if(curr_tok.HasValue)
                {
                    prec = curr_tok.Value.BinPrec();
                    if(!prec.HasValue || prec.Value < min_prec)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
                // consuming the operater
                Token token = consume();
                int next_min_prec = prec.Value + 1;
                NodeExpr expr_rhs = parse_expr(next_min_prec);
                if(expr_rhs == null)
                {
                    // error_expected("expression");
                }

                NodeBinExpr expr = new NodeBinExpr();

                if (token.Type == TokenType.plus)
                {
                    NodeBinExprAdd add = new NodeBinExprAdd() { lhs = expr_lhs, rhs = expr_rhs };
                    expr.expr = add;
                }
                else if (token.Type == TokenType.star)
                {
                    NodeBinExprMulti multi = new NodeBinExprMulti() { lhs = expr_lhs, rhs = expr_rhs };
                    expr.expr = multi;
                }
                else if (token.Type == TokenType.minus)
                {
                    NodeBinExprSub sub = new NodeBinExprSub() { lhs = expr_lhs, rhs = expr_rhs };
                    expr.expr = sub;
                }
                else if (token.Type == TokenType.fslash)
                {
                    NodeBinExprDiv div = new NodeBinExprDiv() { lhs = expr_lhs, rhs = expr_rhs };
                    expr.expr = div;
                }
                else
                {
                    Environment.Exit(-1);
                    //assert(false); // Unreachable;
                }

                return new NodeExpr() { var =  expr };
            }
            return expr_lhs;
        }

        RelationalOperater parse_operater()
        {
            if (try_consume(TokenType.eq).HasValue) // ==
            {
                if (try_consume(TokenType.eq).HasValue)
                {
                    return RelationalOperater.equality;
                }
            }
            else if (try_consume(TokenType.exclamation).HasValue) // !=
            {
                if (try_consume(TokenType.eq).HasValue)
                {
                    return RelationalOperater.inequality;
                }
            }
            else if (try_consume(TokenType.left_arrow).HasValue) // <
            {
                return RelationalOperater.LessThen;
            }
            else if (try_consume(TokenType.right_arrow).HasValue) // >
            {
                return RelationalOperater.GreaterThen;
            }

            return 0;
        }
        NodeExpr parse_type()
        {
            if (!peek().HasValue) return null;

            NodeExpr result;
            NodeTermType nodeTermType = new NodeTermType();
            NodeTerm nodeTerm = new NodeTerm();
            switch (peek().Value.Type)
            {
                case TokenType.char_:
                case TokenType.float_:
                case TokenType.int_:
                case TokenType.short_:
                    result = new NodeExpr();
                    nodeTermType.Type = consume().Type;
                    nodeTerm.term = nodeTermType;
                    result.var = nodeTerm;
                    break;
                case TokenType.long_:
                case TokenType.signed:
                case TokenType.unsigned:
                    consume();
                    return parse_type();
                default:
                    Console.WriteLine("Error: parse_type");
                    Environment.Exit(-1);
                    return null;
            }
            nodeTerm.term = new NodeTermType();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>a NodeStmtFuncPrototype or NodeStmtFunc</returns>
        object parse_func()
        {
            Token returnType = consume();
            NodeExpr FuncName = parse_term();
            try_consume_err(TokenType.open_paren);

            List<NodeStmt> arguments = new List<NodeStmt>();
            while (peek().HasValue && peek().Value.Type != TokenType.close_paren)
            {
                arguments.Add(parse_stmt());
                if(!try_consume(TokenType.comma).HasValue)
                {
                    break;
                }
            }

            try_consume_err(TokenType.close_paren);
            if(try_consume(TokenType.semi).HasValue)
            {
                return new NodeStmtFuncPrototype()
                {
                    Func_name = FuncName,
                    arguments = arguments.ToArray(),
                    ReturnType = returnType.Type
                };
            }

            return new NodeStmtFunc()
            {
                Func_name = FuncName,
                arguments = arguments.ToArray(),
                ReturnType = returnType.Type
            };
        }

        bool parse_assingment(out AssingmentOperater assingmentOperater, out NodeExpr value)
        {
            if(peek().HasValue && peek().Value.Type == TokenType.eq)
            {
                consume();
                assingmentOperater = AssingmentOperater.SimpleAssignment;
                value = parse_term();
                return true;
            }

            assingmentOperater = AssingmentOperater.None; 
            value = null;
            return false;
        }

        NodeStmtInt parse_int()
        {
            consume();
            if(peek().HasValue && peek().Value.Type == TokenType.ident)
            {
                NodeExpr name = parse_term();
                if (parse_assingment(out AssingmentOperater assingmentOperater, out NodeExpr value) == false)
                {
                    if (try_consume(TokenType.semi).HasValue)
                    {

                    }
                    return new NodeStmtInt()
                    {
                        Name = name,
                        assingmentOperater = AssingmentOperater.None,
                    };
                }
                else
                {
                    try_consume_err(TokenType.semi);
                    return new NodeStmtInt()
                    {
                        assingmentOperater = assingmentOperater,
                        Name = name,
                        Value = value,
                    };
                }
            }
            else
            {
                return new NodeStmtInt()
                {
                };
            }
        }

        NodeStmtChar parse_char()
        {
            consume();
            if (peek().HasValue && peek().Value.Type == TokenType.ident)
            {
                NodeExpr name = parse_term();
                if (parse_assingment(out AssingmentOperater assingmentOperater, out NodeExpr value) == false)
                {
                    if(try_consume(TokenType.semi).HasValue)
                    {

                    }
                    return new NodeStmtChar()
                    {
                        Name = name,
                        assingmentOperater = AssingmentOperater.None,
                    };
                }
                else
                {
                    try_consume_err(TokenType.semi);
                    return new NodeStmtChar()
                    {
                        assingmentOperater = assingmentOperater,
                        Name = name,
                        Value = value,
                    };
                }
            }
            else
            {
                return new NodeStmtChar()
                {
                };
            }
        }

        NodeScope parse_scope()
        {
            try_consume_err(TokenType.open_curly);
            NodeScope nodeScope = new NodeScope();
            List<NodeStmt> nodeStmts = new List<NodeStmt>();
            while (peek().HasValue && peek().Value.Type != TokenType.close_curly)
            {
                nodeStmts.Add(parse_stmt());
            }
            try_consume_err(TokenType.close_curly);
            nodeScope.ScopeStmts = nodeStmts.ToArray();
            return nodeScope;
        }
        NodeStmtIf parse_if()
        {
            consume();
            try_consume_err(TokenType.open_paren);
            NodeTermComparer nodeTermComparer = new NodeTermComparer();
            nodeTermComparer.lhs = parse_term();
            nodeTermComparer.operater = parse_operater();
            nodeTermComparer.rhs = parse_term();
            try_consume_err(TokenType.close_paren);
            NodeStmtIf nodeStmtIf = new NodeStmtIf()
            {
                Scope = parse_scope(),
                pred = parse_if_pred(),
                TermComparer = nodeTermComparer,
            };
            return nodeStmtIf;
        }
        NodeIfPred parse_if_pred()
        {
            if (try_consume(TokenType.else_).HasValue)
            {
                NodeTermComparer nodeTermComparer = new NodeTermComparer();
                if (try_consume(TokenType.if_).HasValue)
                {
                    // else if
                    try_consume_err(TokenType.open_paren);
                    NodeIfPredElif nodeIfPredElif = new NodeIfPredElif();
                    nodeTermComparer.lhs = parse_term();
                    nodeTermComparer.operater = parse_operater();
                    nodeTermComparer.rhs = parse_term();
                    try_consume_err(TokenType.close_paren);
                    nodeIfPredElif.TermComparer = nodeTermComparer;
                    nodeIfPredElif.Scope = parse_scope();
                    nodeIfPredElif.pred = parse_if_pred();
                    return new NodeIfPred() { var = nodeIfPredElif };
                }
                // else
                NodeIfPredElse nodeIfPredElse = new NodeIfPredElse();
                nodeIfPredElse.Scope = parse_scope();
                return new NodeIfPred() { var = nodeIfPredElse };
            }
            return null;
        }

        NodeStmtFuncCall parse_funccall()
        {
            NodeExpr Name = parse_term();
            try_consume_err(TokenType.open_paren);
            List<NodeExpr> arguments = new List<NodeExpr>();
            while (peek().HasValue && !try_consume(TokenType.close_paren).HasValue)
            {
                arguments.Add(parse_term());
                if (!try_consume(TokenType.comma).HasValue)
                {
                    try_consume_err(TokenType.close_paren);
                    break;
                }
            }
            try_consume_err(TokenType.semi);
            return new NodeStmtFuncCall()
            {
                arguments = arguments.ToArray(),
                FuncName = Name,
            };
        }

        NodeStmtReturn parse_return()
        {
            consume();
            NodeExpr expr = parse_term();
            try_consume_err(TokenType.semi);
            return new NodeStmtReturn()
            {
                expr = expr,
            };
        }

        NodeStmtWhile parse_while()
        {
            try_consume_err(TokenType.while_);
            try_consume_err(TokenType.open_paren);
            NodeTermComparer nodeTermComparer = new NodeTermComparer()
            {
                lhs = parse_term(),
            };
            nodeTermComparer.operater = parse_operater();
            nodeTermComparer.rhs = parse_term();
            try_consume_err(TokenType.close_paren);
            return new NodeStmtWhile()
            {
                TermComparer = nodeTermComparer,
                Scope = parse_scope()
            };
        }

        NodeStmt parse_stmt()
        {
            if(peek().Value.Type == TokenType.void_)
            {
                object Nodefunc = parse_func();
                if(Nodefunc.GetType() == typeof(NodeStmtFunc))
                {
                    return (NodeStmtFunc)Nodefunc;
                }
                else if (Nodefunc.GetType() == typeof(NodeStmtFuncPrototype))
                {
                    return (NodeStmtFuncPrototype)Nodefunc;
                }
                return null;
            }
            else if (peek().Value.Type == TokenType.int_)
            {
                if(peek(2).HasValue && peek(2).Value.Type == TokenType.open_paren)
                {
                    object Nodefunc = parse_func();
                    if (Nodefunc.GetType() == typeof(NodeStmtFunc))
                    {
                        return (NodeStmtFunc)Nodefunc;
                    }
                    else if (Nodefunc.GetType() == typeof(NodeStmtFuncPrototype))
                    {
                        return (NodeStmtFuncPrototype)Nodefunc;
                    }
                    return null;
                }
                return parse_int();
            }
            else if (peek().Value.Type == TokenType.char_)
            {
                if (peek(2).HasValue && peek(2).Value.Type == TokenType.open_paren)
                {
                    object Nodefunc = parse_func();
                    if (Nodefunc.GetType() == typeof(NodeStmtFunc))
                    {
                        return (NodeStmtFunc)Nodefunc;
                    }
                    else if (Nodefunc.GetType() == typeof(NodeStmtFuncPrototype))
                    {
                        return (NodeStmtFuncPrototype)Nodefunc;
                    }
                    return null;
                }
                return parse_char();
            }
            else if (peek().Value.Type == TokenType.open_curly)
            {
                return parse_scope();
            }
            else if (peek().Value.Type == TokenType.if_)
            {
                return parse_if();
            }
            else if (peek().Value.Type == TokenType.ident)
            {
                if (peek(1).HasValue && peek(1).Value.Type == TokenType.open_paren)
                {
                    return parse_funccall();
                }
            }
            else if (peek().Value.Type == TokenType.return_)
            {
                return parse_return();
            }
            else if (peek().Value.Type == TokenType.while_)
            {
                return parse_while();
            }
            else
            {
                Console.WriteLine($"prop in a loop {consume()}");
            }
            return null;
        }


        public NodeProg Parse_prog(Token[] tokens)
        {
            m_tokens = tokens;
            NodeProg result = new NodeProg();

            while (peek().HasValue && peek().Value.Type != TokenType.none)
            {
                if (peek().Value.Type == TokenType.NewFileToken) consume();

                NodeStmt nodeStmt = parse_stmt();
                if (nodeStmt == null) break;
                
                if(peek(-1).HasValue)
                {
                    nodeStmt.Token = peek(-1).Value;
                }
                else if (peek(0).HasValue)
                {
                    nodeStmt.Token = peek(0).Value;
                }
                m_output.Add(nodeStmt);
            }

            result.NodeStmt = m_output.ToArray();

            return result;
        }

        Token[] m_tokens;
        int m_index = 0;

        private Token? peek(int offset = 0)
        {
            if (offset + m_index >= m_tokens.Length)
            {
                return null;
            }
            if (offset + m_index < 0)
            {
                return null;
            }
            return m_tokens[m_index + offset];
        }

        private Token consume()
        {
            return m_tokens[m_index++];
        }

        private Token? try_consume(TokenType type)
        {
            if (peek().HasValue && peek().Value.Type != type)
            {
                return null;
            }
            return consume();
        }

        private Token try_consume_err(TokenType type)
        {
            if (peek().HasValue && peek().Value.Type != type)
            {
                CompilerErrors.Erorr_Expected_Token(peek(-1).Value, type);
                return new Token();
            }
            return consume();
        }
    }
}
