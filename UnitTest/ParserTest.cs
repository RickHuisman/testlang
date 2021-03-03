using System;
using System.Collections.Generic;
using NUnit.Framework;
using testlang;
using testlang.ast;

namespace UnitTest
{
    public class ParserTest
    {
        [Test]
        public void Parse_StringExpr_ReturnsAst()
        {
            var expect = new List<Statement>
            {
                new StatementExpr(
                    new Expression(
                        new BinaryExpression(
                            new Expression(new StringLiteral("test")),
                            new Expression(new TrueLiteral()),
                            BinaryOperator.Equal)
                    )
                )
            };

            var input = "\"test\" == true;";
            var result = Parser.Parse(input);
            
            Assert.That(expect, Is.EquivalentTo(result));
        }
        
        [Test]
        public void Parse_BooleanExpr_ReturnsAst()
        {
            var expect = new List<Statement>
            {
                new StatementExpr(
                    new Expression(
                        new BinaryExpression(
                            new Expression(new TrueLiteral()),
                            new Expression(new FalseLiteral()),
                            BinaryOperator.BangEqual)
                    )
                )
            };

            var input = "true != false;";
            var result = Parser.Parse(input);
            
            Assert.That(expect, Is.EquivalentTo(result));
        }
        
        [Test]
        public void Parse_NilExpr_ReturnsAst()
        {
            var expect = new List<Statement>
            {
                new StatementExpr(
                    new Expression(
                        new BinaryExpression(
                            new Expression(new NilLiteral()),
                            new Expression(new FalseLiteral()),
                            BinaryOperator.BangEqual)
                    )
                )
            };

            var input = "nil != false;";
            var result = Parser.Parse(input);
            
            Assert.That(expect, Is.EquivalentTo(result));
        }

        [Test]
        public void Parse_NumbersExpr_ReturnsAst()
        {
            // var expect = new List()<Statement>
            // {
            //     new Print()
            // }

            var input = "10 + 2";
            var stmts = Parser.Parse(input);

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