using System;
using System.Collections.Generic;
using System.Linq;

namespace testlang.ast
{
    public static class ParserRules
    {
        public static List<ParseRule> Rules = GetRules();

        public static ParseRule GetRule(TokenType type)
        {
            var rule = Rules.SingleOrDefault(r => r.Type == type);
            if (rule == null)
            {
                throw new Exception($"No matching rule found for TokenType: {type}");
            }
            return rule;
        }

        private static List<ParseRule> GetRules()
        {
            return new List<ParseRule>
            {
                new ParseRule(TokenType.EOF, null, null, Precedence.None),
                new ParseRule(TokenType.Fun, null, null, Precedence.None),
                new ParseRule(TokenType.Identifier, Parser.ParseVariable, null, Precedence.None),
                new ParseRule(TokenType.LeftParen, Parser.Grouping, Parser.Call, Precedence.Call), // TODO Prefix Grouping
                new ParseRule(TokenType.RightParen, null, null, Precedence.None),
                new ParseRule(TokenType.RightBrace, null, null, Precedence.None),
                new ParseRule(TokenType.Number, Parser.Number, null, Precedence.None),
                new ParseRule(TokenType.Plus, null, Parser.Binary, Precedence.Term),
                new ParseRule(TokenType.Minus, null, Parser.Binary, Precedence.Comparison),
                new ParseRule(TokenType.Slash, null, Parser.Binary, Precedence.Factor),
                new ParseRule(TokenType.Star, null, Parser.Binary, Precedence.Factor),
                new ParseRule(TokenType.Semicolon, null, null, Precedence.None),
                new ParseRule(TokenType.LessThan, null, Parser.Binary, Precedence.Comparison),
                new ParseRule(TokenType.LessThanEqual, null, Parser.Binary, Precedence.Comparison),
                new ParseRule(TokenType.Return, null, null, Precedence.None),
                new ParseRule(TokenType.String, Parser.String, null, Precedence.None),
            };
        }
    }

    public class ParseRule
    {
        public TokenType Type;
        public Func<Token, ExpressionKind> Prefix;
        public Func<Token, ExpressionKind> Infix;
        public Precedence Precedence;

        public ParseRule(TokenType type, Func<Token, ExpressionKind> prefix, Func<Token, ExpressionKind> infix, Precedence precedence)
        {
            Type = type;
            Prefix = prefix;
            Infix = infix;
            Precedence = precedence;
        }
    }
}