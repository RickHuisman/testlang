namespace testlang.Scanner
{
    public class Token
    {
        public TokenType Type { get; }
        public string Source { get; }

        public Token(TokenType type, string source)
        {
            Type = type;
            Source = source;
        }

        public override string ToString()
        {
            return $"Token (Type: {Type}, Source: {Source})";
        }
    }
}
