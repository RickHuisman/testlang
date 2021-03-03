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
            var expect = new List<Token>
            {
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Plus, "+"),
                new Token(TokenType.Number, "5"),
                new Token(TokenType.Semicolon, ";"),
            };

            var input = "10 + 5;";
            var result = Lexer.Parse(input);
            
            Assert.That(expect, Is.EquivalentTo(result));
        }
        
        [Test]
        public void Parse_String_ReturnsTokens()
        {
            var expect = new List<Token>
            {
                new Token(TokenType.String, "test"),
                new Token(TokenType.Semicolon, ";"),
            };

            var input = "\"test\";";
            var result = Lexer.Parse(input);
            
            Assert.That(expect, Is.EquivalentTo(result));
        }
        
        [Test]
        public void Parse_StringAndNumber_ReturnsTokens()
        {
            var expect = new List<Token>
            {
                new Token(TokenType.String, "test"),
                new Token(TokenType.Plus, "+"),
                new Token(TokenType.Number, "10"),
                new Token(TokenType.Semicolon, ";"),
            };

            var input = "\"test\" + 10;";
            var result = Lexer.Parse(input);
            
            Assert.That(expect, Is.EquivalentTo(result));
        }
    }
}