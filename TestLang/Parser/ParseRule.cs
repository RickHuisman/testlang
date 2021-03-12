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
                new ParseRule(TokenType.Identifier, Parser2.ParseVariable, null, Precedence.None),
                new ParseRule(TokenType.LeftParen, Parser2.Grouping, Parser2.Call, Precedence.Call), // TODO Prefix Grouping
                new ParseRule(TokenType.RightParen, null, null, Precedence.None),
                new ParseRule(TokenType.Number, Parser2.Number, null, Precedence.None),
                new ParseRule(TokenType.Plus, null, Parser2.Binary, Precedence.Term),
                new ParseRule(TokenType.Slash, null, Parser2.Binary, Precedence.Factor),
                new ParseRule(TokenType.Star, null, Parser2.Binary, Precedence.Factor),
                new ParseRule(TokenType.Semicolon, null, null, Precedence.None),
                new ParseRule(TokenType.LessThan, null, Parser2.Binary, Precedence.Comparison),
                new ParseRule(TokenType.LessThanEqual, null, Parser2.Binary, Precedence.Comparison),
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