namespace testlang.ast
{
    public abstract class Literal : ExpressionKind
    {
    }

    public class Number : Literal
    {
        public double Value;

        public Number(double value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Number({Value})";
        }
    }
}