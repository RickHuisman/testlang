namespace testlang.ast
{
    public abstract class Literal : ExpressionKind
    {
    }

    public class Number : Literal
    {
        public double Value { get; }

        public Number(double value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Number({Value})";
        }
    }

    public class TrueLiteral : Literal
    {
    }

    public class FalseLiteral : Literal
    {
    }
    
    public class NilLiteral : Literal
    {
    }
    
    public class StringLiteral : Literal
    {
        public string Value { get; }

        public StringLiteral(string value)
        {
            Value = value;
        }
    }
}