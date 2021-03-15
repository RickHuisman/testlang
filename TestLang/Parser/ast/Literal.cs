using testlang.Scanner.ast;

namespace testlang.Parser.ast
{
    public interface ILiteral : IExpressionKind
    {
    }

    public class Number : ILiteral
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

    public class StringLiteral : ILiteral
    {
        public string Value { get; }

        public StringLiteral(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"String({Value})";
        }
    }

    public class TrueLiteral : ILiteral
    {
    }

    public class FalseLiteral : ILiteral
    {
    }

    public class NilLiteral : ILiteral
    {
    }
}