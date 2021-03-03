namespace testlang.ast
{
    public class Expression
    {
        public ExpressionKind Node;

        public Expression(ExpressionKind node)
        {
            Node = node;
        }

        public override string ToString()
        {
            return $"({Node})";
        }
    }

    public abstract class ExpressionKind
    {
        
    }

    public enum BinaryOperator
    {
        Equal,
        BangEqual,
        GreaterThan,
        GreaterThanEqual,
        LessThan,
        LessThanEqual,
        Minus,
        Plus,
        Slash,
        Star,
    }

    public class BinaryExpression : ExpressionKind
    {
        public Expression Lhs;
        public Expression Rhs;
        public BinaryOperator Operator;

        public BinaryExpression(Expression lhs, Expression rhs, BinaryOperator op)
        {
            Lhs = lhs;
            Rhs = rhs;
            Operator = op;
        }

        public override string ToString()
        {
            return $"Lhs: {Lhs} - Rhs: {Rhs} - Operator: {Operator}";
        }
    }
    
    public enum UnaryOperator
    {
        Minus,
        Bang
    }
    
    public class UnaryExpression : ExpressionKind
    {
        public UnaryOperator Operator;
        public Expression Unary;

        public UnaryExpression(UnaryOperator op, Expression unary)
        {
            Operator = op;
            Unary = unary;
        }

        public override string ToString()
        {
            return $"Node: {Unary} - Operator: {Operator}";
        }
    }
}