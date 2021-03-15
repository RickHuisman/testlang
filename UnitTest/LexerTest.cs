using System;
using System.Collections.Generic;
using NUnit.Framework;
using testlang.Scanner;

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

            const string input = "10 + 5;";
            var actual = Lexer.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_Modulo_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Percent, "%"),
                new Token(TokenType.Number, "5"),
                new Token(TokenType.Semicolon, ";"),
            };

            const string input = "10 % 5;";
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

            const string input = "\"test\";";
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

            const string input = "\"test\" + 10;";
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

            const string input = "true != false;";
            var actual = Lexer.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_SetVar_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.Var, "var"),
                new Token(TokenType.Identifier, "x"),
                new Token(TokenType.Equal, "="),
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.Identifier, "x"),
                new Token(TokenType.Equal, "="),
                new Token(TokenType.Number, "20"),
                new Token(TokenType.Semicolon, ";"),
            };

            const string input = @"var x = 10;
x = 20;";
            var actual = Lexer.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_Block_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.LeftBrace, "{"),
                new Token(TokenType.Var, "var"),
                new Token(TokenType.Identifier, "x"),
                new Token(TokenType.Equal, "="),
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.RightBrace, "}"),
                new Token(TokenType.Semicolon, ";"),
            };

            const string input = @"{
var x = 10;
};";
            var actual = Lexer.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_Fun_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.Fun, "fun"),
                new Token(TokenType.Identifier, "test"),
                new Token(TokenType.LeftParen, "("),
                new Token(TokenType.RightParen, ")"),
                new Token(TokenType.LeftBrace, "{"),
                new Token(TokenType.Print, "print"),
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Semicolon, ";"),
                new Token(TokenType.RightBrace, "}"),
                new Token(TokenType.Semicolon, ";"),
            };

            const string input = @"
fun test() {
    print 10;
};";
            var actual = Lexer.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_Struct_ReturnsTokens()
        {
            var expected = new List<Token>
            {
                new Token(TokenType.Struct, "struct"),
                new Token(TokenType.Identifier, "Foo"),
                new Token(TokenType.LeftBrace, "{"),
                new Token(TokenType.RightBrace, "}"),
            };

            const string input = @"
struct Foo {
}";
            var actual = Lexer.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
    }
}