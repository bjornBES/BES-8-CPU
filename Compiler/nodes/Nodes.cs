namespace Compiler.nodes
{
    public struct NodeTermIntLit
    {
        public Token int_lit;
    }
    public struct NodeTermIdent
    {
        public Token ident;
    }
    public struct NodeTermType
    {
        public TokenType type;
    }

    public struct NodeTerm
    {
        /// <summary>
        /// NodeTermIntLit, NodeTermIdent, NodeTermType
        /// </summary>
        public object var;
    }

    public struct NodeOperator
    {
        /// <summary>
        /// limit is 2 tokens
        /// </summary>
        public TokenType[] tokens;
    }

    public struct NodeScope
    {
        public NodeStmt[] stmts;
    }

    public struct NodeExpr
    {
        /// <summary>
        /// NodeTerm
        /// </summary>
        public object var;
    }

    public struct NodeStmtReturn
    {
        public NodeExpr expr;
    }

    public struct NodeStmtFunc
    {
        public NodeTermType ReturnType;
        public NodeExpr Name;
        public NodeStmt[] Arguments;
    }

    public struct NodeStmtAssign
    {
        public NodeExpr name;
        public NodeTermType type;
        public NodeOperator OperatorToken;
        public NodeExpr value;
    }

    public struct NodeStmt
    {
        /// <summary>
        /// NodeStmtFunc, NodeStmtReturn. NodeScope, NodeStmtAssign
        /// </summary>
        public object stmt;
    }

    public struct NodeProg
    {
        public NodeStmt[] stmts;
    }
}
