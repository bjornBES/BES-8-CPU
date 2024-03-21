namespace Ccompiler.nodes
{
    public enum AssingmentOperater
    {
        None,
        /// <summary>
        /// =
        /// </summary>
        SimpleAssignment,

        /// <summary>
        /// +=
        /// </summary>
        AdditionAssignment,
        
        /// <summary>
        /// -=
        /// </summary>
        SubtractionAssignment,

        /// <summary>
        /// *=
        /// </summary>
        MultiplicationAssignment,
        
        /// <summary>
        /// /=
        /// </summary>
        DivisionAssignment,
        
        /// <summary>
        /// %=
        /// </summary>
        ModulusAssignment,
        
        /// <summary>
        /// &=
        /// </summary>
        BitwiseANDAssignment,
        
        /// <summary>
        /// |=
        /// </summary>
        BitwiseORAssignment,
        
        /// <summary>
        /// ^=
        /// </summary>
        BitwiseXORAssignment,
        
        /// <summary>
        /// <<=
        /// </summary>
        LeftShiftAssignment,
        
        /// <summary>
        /// >>=
        /// </summary>
        RightShiftAssignment,
    }
    public enum RelationalOperater
    {
        /// <summary>
        /// ==
        /// </summary>
        equality,
        /// <summary>
        /// !=
        /// </summary>
        inequality,
        /// <summary>
        /// <
        /// </summary>
        LessThen,
        /// <summary>
        /// >
        /// </summary>
        GreaterThen,

    }
    public class NodeTermIntLit
    {
        public Token int_lit;
    }

    public class NodeTermCharLit
    {
        public Token C;
    }

    public class NodeTermStringLit
    {
        public Token str;
    }

    public class NodeTermComparer
    {
        public NodeExpr lhs;
        public NodeExpr rhs;
        public RelationalOperater operater;
    }

    public class NodeTermIdent
    {
        public Token ident;
    }

    public class NodeTermType
    {
        public TokenType Type;
    }

    public class NodeTermParen
    {
        public NodeExpr expr;
    }

    public class NodeBinExprAdd
    {
        public NodeExpr lhs;
        public NodeExpr rhs;
    }

    public class NodeBinExprMulti
    {
        public NodeExpr lhs;
        public NodeExpr rhs;
    }

    public class NodeBinExprSub
    {
        public NodeExpr lhs;
        public NodeExpr rhs;
    }

    public class NodeBinExprDiv
    {
        public NodeExpr lhs;
        public NodeExpr rhs;
    }

    public class NodeBinExpr
    {
        //NodeBinExprAdd, NodeBinExprMulti, NodeBinExprSub, NodeBinExprDiv
        public object expr;
    }

    public class NodeTerm
    {
        // NodeTermIntLit, NodeTermIdent, NodeTermParen, NodeTermType, NodeTermCharLit, NodeTermStringLit
        public object term;
    }
    public class NodeExpr
    {
        // NodeTerm, NodeBinExpr
        public object var;
    }
    public class NodeIfPredElif
    {
        public NodeScope Scope;
        public NodeTermComparer TermComparer;
        public NodeIfPred pred;
    }
    public class NodeIfPredElse
    {
        public NodeScope Scope;
    }
    public class NodeIfPred
    {
        // NodeIfPredElif, NodeIfPredElse
        public object var;
    }
    public class NodeStmtIf : NodeStmt
    {
        public NodeScope Scope;
        public NodeTermComparer TermComparer;
        public NodeIfPred pred;
    }
    public class NodeStmtAssingment : NodeStmt
    {

    }
    public class NodeStmtInt : NodeStmt
    {
        public NodeExpr Name;
        public NodeExpr Value;
        public AssingmentOperater assingmentOperater;
    }
    public class NodeStmtChar : NodeStmt
    {
        public NodeExpr Name;
        public NodeExpr Value;
        public AssingmentOperater assingmentOperater;
    }

    public class NodeStmtWhile : NodeStmt
    {
        public NodeTermComparer TermComparer;
        public NodeScope Scope;
    }

    public class NodeStmtSizeOf : NodeStmt
    {
        public NodeExpr Expr;
    }

    public class NodeStmtReturn : NodeStmt
    {
        public NodeExpr expr;
    }

    public class NodeStmtFuncCall : NodeStmt
    {
        public NodeExpr FuncName;
        public NodeExpr[] arguments;
    }

    public class NodeScope : NodeStmt
    {
        public NodeStmt[] ScopeStmts;
    }

    public class NodeStmtFuncPrototype : NodeStmt
    {
        public NodeExpr Func_name;
        public TokenType ReturnType;
        public NodeStmt[] arguments;
    }

    public class NodeStmtFunc : NodeStmt
    {
        public NodeExpr Func_name;
        public TokenType ReturnType;
        public NodeStmt[] arguments;
    }

    public abstract class NodeStmt
    {
        // NodeStmtFunc, NodeScope, NodeStmtReturn, NodeStmtInt
        public object stmts;
        public Token Token;
    }

    public class NodeProg
    {
        public NodeStmt[] NodeStmt;
    }
}
