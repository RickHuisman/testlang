using System;
using System.Collections.Generic;
using System.Linq;
using testlang.Parser.ast;

namespace testlang.Scanner.ast
{
    public static class ParserRules
    {
        public static ParseRule GetRule(TokenType type)
        {
            var rule = GetRules().SingleOrDefault(r => r.Type == type);
            if (rule == null)
            {
                throw new Exception($"No matching rule found for TokenType: {type}"); // TODO
            }

            return rule;
        }

        private static List<ParseRule> GetRules()
        {
            return new List<ParseRule>
            {
                new ParseRule(TokenType.Eof, null, null, Precedence.None),
                new ParseRule(TokenType.Fun, null, null, Precedence.None),
                new ParseRule(TokenType.Identifier, Parser.Parser.ParseVariable, null, Precedence.None),
                new ParseRule(TokenType.LeftParen, Parser.Parser.Grouping, Parser.Parser.Call, Precedence.Call),
                new ParseRule(TokenType.RightParen, null, null, Precedence.None),
                new ParseRule(TokenType.RightBrace, null, null, Precedence.None),
                new ParseRule(TokenType.Dot, null, Parser.Parser.Dot, Precedence.Call),
                new ParseRule(TokenType.Number, Parser.Parser.Number, null, Precedence.None),
                new ParseRule(TokenType.String, Parser.Parser.String, null, Precedence.None),
                new ParseRule(TokenType.Plus, null, Parser.Parser.Binary, Precedence.Term),
                new ParseRule(TokenType.Minus, Parser.Parser.Unary, Parser.Parser.Binary, Precedence.Comparison),
                new ParseRule(TokenType.Slash, null, Parser.Parser.Binary, Precedence.Factor),
                new ParseRule(TokenType.Star, null, Parser.Parser.Binary, Precedence.Factor),
                new ParseRule(TokenType.Percent, null, Parser.Parser.Binary, Precedence.Factor),
                new ParseRule(TokenType.Semicolon, null, null, Precedence.None),
                new ParseRule(TokenType.LessThan, null, Parser.Parser.Binary, Precedence.Comparison),
                new ParseRule(TokenType.LessThanEqual, null, Parser.Parser.Binary, Precedence.Comparison),
                new ParseRule(TokenType.GreaterThan, null, Parser.Parser.Binary, Precedence.Comparison),
                new ParseRule(TokenType.GreaterThanEqual, null, Parser.Parser.Binary, Precedence.Comparison),
                new ParseRule(TokenType.Equal, null, null, Precedence.None),
                new ParseRule(TokenType.EqualEqual, null, Parser.Parser.Binary, Precedence.Equality),
                new ParseRule(TokenType.BangEqual, null, Parser.Parser.Binary, Precedence.Equality),
                new ParseRule(TokenType.False, Parser.Parser.Literal, null, Precedence.None),
                new ParseRule(TokenType.True, Parser.Parser.Literal, null, Precedence.None),
                new ParseRule(TokenType.Nil, Parser.Parser.Literal, null, Precedence.None),
                new ParseRule(TokenType.Struct, null, null, Precedence.None),
                new ParseRule(TokenType.Return, null, null, Precedence.None),
            };
        }
    }

    public class ParseRule
    {
        public TokenType Type;
        public Func<Token, IExpressionKind> Prefix;
        public Func<Token, IExpressionKind> Infix;
        public Precedence Precedence;

        public ParseRule(
            TokenType type,
            Func<Token, IExpressionKind> prefix,
            Func<Token, IExpressionKind> infix,
            Precedence precedence)
        {
            Type = type;
            Prefix = prefix;
            Infix = infix;
            Precedence = precedence;
        }
    }
}