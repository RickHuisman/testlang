namespace testlang.Scanner
{
    public enum TokenType
    {
        // Single-character tokens
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        Comma,
        Dot,
        Minus,
        Plus,
        Percent,
        Semicolon,
        Star,

        // One or two character tokens
        Bang,
        BangEqual,
        Equal,
        EqualEqual,
        LessThan,
        LessThanEqual,
        GreaterThan,
        GreaterThanEqual,
        Slash,
        Comment,

        // Literals
        Identifier,
        String,
        Number,

        // Keywords
        And,
        Else,
        False,
        For,
        Fun,
        Struct,
        If,
        Nil,
        Or,
        Print,
        Return,
        Super,
        This,
        True,
        Var,
        While,

        Eof
    }

    public static class TokenTypeTranslator
    {
        public static TokenType FromString(string str)
        {
            return str switch
            {
                "and" => TokenType.And,
                "else" => TokenType.Else,
                "false" => TokenType.False,
                "for" => TokenType.For,
                "fun" => TokenType.Fun,
                "if" => TokenType.If,
                "nil" => TokenType.Nil,
                "or" => TokenType.Or,
                "print" => TokenType.Print,
                "return" => TokenType.Return,
                "super" => TokenType.Super,
                "this" => TokenType.This,
                "true" => TokenType.True,
                "var" => TokenType.Var,
                "while" => TokenType.While,
                "struct" => TokenType.Struct,
                _ => TokenType.Identifier
            };
        }
    }
}
