using System;
using NUnit.Framework;
using testlang;
using testlang.ast;

namespace UnitTest
{
    public class ParserTest
    {
        [Test]
        public void Parse_NumbersExpr_ReturnsAst()
        {
            // var expect = new List()<Statement>
            // {
            //     new Print()
            // }
            
            var input = "10 + 2";
            var parser = new Parser(input);
            var stmts = parser.Parse();
            
            foreach (var stmt in stmts)
            {
                Console.WriteLine($"Statement: {stmt}");
            }

            // var expect = new List<Token>
            // {
            //     new Token(TokenType.Number, "10"),
            //     new Token(TokenType.Plus, "+"),
            //     new Token(TokenType.Number, "5"),
            //     new Token(TokenType.Semicolon, ";"),
            // };
            //
            // var input = "10 + 5;";
            // var result = Lexer.Parse(input);
            //
            // Assert.That(expect, Is.EquivalentTo(result));
        }
    }
}