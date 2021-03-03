namespace testlang.ast
{
    public class Statement
    {
    }

    public class Print : Statement
    {
        private Expression Expr { get; }
        
        public Print(Expression expr)
        {
            Expr = expr;
        }

        public override string ToString()
        {
            return $"[Print {Expr}]";
        }
    }

    public class StatementExpr : Statement
    {
        private Expression Expr { get; }
        
        public StatementExpr(Expression expr)
        {
            Expr = expr;
        }
        
        public override string ToString()
        {
            return $"[Expr {Expr}]";
        }
    }
}