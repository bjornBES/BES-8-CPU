using Compiler.nodes;

namespace Compiler
{
    public class Parser
    {
        List<NodeStmt> m_nodes = new List<NodeStmt>();
        Token[] m_tokens;
        int m_index;

        public Parser() 
        {
            
        }

        NodeStmtFunc parse_func()
        {
            // type ident (arguments) {
            if(peek() == null)
            {
                Environment.Exit(1);
            }
            Token FuncType = consume();
            NodeExpr name = parse_expr();
            tryConsumeError(TokenType.open_paren);
            List<NodeStmt> stmts = new List<NodeStmt>();
            while (peek().Type != TokenType.close_paren)
            {
                stmts.Add(parse_stmt());
            }
            tryConsumeError(TokenType.close_paren);
            return new NodeStmtFunc()
            {
                Arguments = stmts.ToArray(),
                Name = name,
                ReturnType = new NodeTermType()
                {
                     type = FuncType.Type,
                }
            };
        }

        void parse_variable()
        {

        }

        NodeStmtReturn parse_retrun()
        {
            tryConsumeError(TokenType.return_);
            NodeStmtReturn nodeStmtReturn = new NodeStmtReturn();
            nodeStmtReturn.expr = parse_expr();
            return nodeStmtReturn;
        }

        NodeScope parse_scope()
        {
            tryConsumeError(TokenType.open_curly);
            NodeScope result = new NodeScope();
            List<NodeStmt> stmts = new List<NodeStmt>();
            int level = 0;
            while (peek() != null && peek().Type != TokenType.close_curly && level == 0) 
            { 
                if(peek().Type == TokenType.open_paren)
                {
                    level++;
                }
                else if (peek().Type == TokenType.close_curly && level > 0)
                {
                    level--;
                }
                stmts.Add(parse_stmt());
            }
            result.stmts = stmts.ToArray();
            return result;
        }

        NodeStmtAssign parse_int()
        {
            tryConsumeError(TokenType.int_);
            NodeExpr name = parse_expr();
            NodeStmtAssign nodeStmtAssign = new NodeStmtAssign()
            {
                name = name,
            };
            if(tryConsume(TokenType.eq) != null)
            {
                nodeStmtAssign.value = parse_expr();
                nodeStmtAssign.OperatorToken.tokens = new TokenType[] 
                {
                    TokenType.eq,
                    TokenType.none
                };
            }

            return nodeStmtAssign;
        }

        NodeExpr parse_expr()
        {
            NodeExpr result = new NodeExpr()
            {
                var = new NodeTerm()
                {
                }
            };

            switch (peek().Type)
            {
                case TokenType.Hex_int_lit:
                    break;
                case TokenType.Bin_int_lit:
                    break;
                case TokenType.int_lit:
                    result.var = new NodeTermIntLit() { int_lit = consume() };
                    break;
                case TokenType.ident:
                    result.var = new NodeTermIdent() { ident = consume() };
                    break;
                case TokenType.quotation:
                    break;
                case TokenType.apostrophe:
                    break;
                case TokenType.ampersand:
                    break;
                default:
                    break;
            }
            return result;
        }
        
        NodeStmt parse_stmt()
        {
            NodeStmt result = new NodeStmt();
            if ( peek().Type == TokenType.const_)
            {
                parse_variable();
            }
            else if (peek().Type == TokenType.global ||
                    peek().Type == TokenType.public_ || 
                    peek().Type == TokenType.local)
            {
                // maybe variable?
            }
            else if (peek().Type == TokenType.func)
            {
                NodeStmtFunc nodeStmtFunc = parse_func();
                result.stmt = nodeStmtFunc;
            }
            else if (peek().Type == TokenType.return_)
            {
                NodeStmtReturn nodeStmtReturn = parse_retrun();
                result.stmt = nodeStmtReturn;
            }
            else if (peek().Type == TokenType.open_curly)
            {
                NodeScope nodeScope = parse_scope();
                result.stmt = nodeScope;
            }
            else if (peek().Type == TokenType.int_)
            {
                NodeStmtAssign nodeStmtAssign = parse_int();
                result.stmt = nodeStmtAssign;
            }
            else
            {
                consume();
            }
            return result;
        }

        public NodeProg Parse_Prog(Token[] tokens)
        {
            m_tokens = tokens;

            NodeProg result = new NodeProg();
            while (peek() != null)
            {
                m_nodes.Add(parse_stmt());
            }

            result.stmts = m_nodes.ToArray();

            return result;
        }

        Token peek(int offset = 0)
        {
            if (m_index + offset >= m_tokens.Length)
            {
                return null;
            }
            return m_tokens[m_index + offset];
        }

        Token tryConsume(TokenType type)
        {
            if (type != peek().Type)
            {
                return null;
            }
            return consume();
        }

        Token tryConsumeError(TokenType type)
        {
            if (type != peek().Type)
            {
                Environment.Exit(1);
                return null;
            }
            return consume();
        }

        Token consume()
        {
            return m_tokens[m_index++];
        }
    }
}
