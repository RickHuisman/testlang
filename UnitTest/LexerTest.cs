using System;
using System.Collections.Generic;
using NUnit.Framework;
using testlang;

namespace UnitTest
{
    public class LexerTest
    {
        [Test]
        public void Parse_Numbers_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Plus, "+"),
                new Token(TokenType.Number, "5"),
                new Token(TokenType.Semicolon, ";"),
            };

            var input = "10 + 5;";
            var actual = Lexer.Parse(input);
            
            TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_String_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.String, "test"),
                new Token(TokenType.Semicolon, ";"),
            };

            var input = "\"test\";";
            var actual = Lexer.Parse(input);
            
            TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_StringAndNumber_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.String, "test"),
                new Token(TokenType.Plus, "+"),
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Semicolon, ";"),
            };

            var input = "\"test\" + 10;";
            var actual = Lexer.Parse(input);
            
            TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_Boolean_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.True, "true"),
                new Token(TokenType.BangEqual, "!="),
                new Token(TokenType.False, "false"),
                new Token(TokenType.Semicolon, ";"),
            };

            var input = "true != false;";
            var actual = Lexer.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }


        [Test]
        public void Parse_SetVar_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.Var, "var"),
                new Token(TokenType.Identifier, "test"),
                new Token(TokenType.Equal, "="),
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.Identifier, "test"),
                new Token(TokenType.Equal, "="),
                new Token(TokenType.Number, "20"),
                new Token(TokenType.Semicolon, ";"),
            };

            var input = @"var test = 10;
x = 20;";
            var actual = Lexer.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
    }
}