using System;

namespace testlang
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

        EOF,
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
                _ => throw new ArgumentOutOfRangeException(nameof(str), str, null)
            };
        }
    }
}
