using System;

namespace testlang
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

        public override bool Equals(object other)  // TODO is used
        {
            if (other is Token token)
            {
                return Type == token.Type && Source == token.Source;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) Type, Source);
        }

        public override string ToString()
        {
            return $"Type: {Type} - Source: {Source}";
        }
    }
}
