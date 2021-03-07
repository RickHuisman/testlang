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
            var expected = new List<Statement>
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
            var actual = Parser.Parse(input);
            
            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_BooleanExpr_ReturnsAst()
        {
            var expected = new List<Statement>
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
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_NilExpr_ReturnsAst()
        {
            var expected = new List<Statement>
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
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_VarStatement_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new VarStatement(
                    new Variable("test"),
                    new Expression(new Number(10))
                )
            };

            var input = "var test = 10;";
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_SetVarExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new VarStatement(
                    new Variable("test"),
                    new Expression(new Number(10))
                )
            };

            var input = @"var test = 10;
x = 20;";
            var actual = Parser.Parse(input);
            Console.WriteLine(TestHelper.AsJson(actual));

            // TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_GetVarExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new VarStatement(
                    new Variable("test"),
                    new Expression(new Number(10))
                )
            };

            //var input = @"var test = 10;
//print test;";
            var input = "print 10;";
            var actual = Parser.Parse(input);
            Console.WriteLine(TestHelper.AsJson(actual));

            // TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_Block_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new BlockStatement(
                    new List<Statement>
                    {
                        new VarStatement(
                            new Variable("x"),
                            new Expression(new Number(10))
                        )
                    })
            };

            var input = @"{
var x = 10;
}";
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
    }
}