using System;
using System.Collections.Generic;
using NUnit.Framework;
using testlang;
using testlang.ast;

namespace UnitTest
{
    public class Parser2Test
    {
        [Test]
        public void Parse_NumberExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new StatementExpr(
                    new Expression(
                        new BinaryExpression(
                            new Expression(new Number(5)),
                            new Expression(new Number(10)),
                            BinaryOperator.Plus)
                    )
                )
            };

            var input = "print 5 + 10;";
            var actual = Parser2.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_VarStatement_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new VarStatement(
                    new Variable("x"),
                    new Expression(new Number(10))
                )
            };

            var input = "var x = 10;";
            var actual = Parser2.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_GetVarExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new VarStatement(
                    new Variable("x"),
                    new Expression(new Number(10))
                )
            };

            var input = "print 10;";

            //             var input = @"
            // var x = 10;
            // print x;";
            var actual = Parser2.Parse(input);
            Console.WriteLine(TestHelper.AsJson(actual));

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_FunCall_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new FunctionStatement(
                    new Variable("test"),
                    new FunctionDeclaration(
                        new List<Variable>(),
                        new BlockStatement(
                            new List<Statement>
                            {
                                new Print(
                                    new Expression(new Number(10)))
                            })
                    )
                ),
                new StatementExpr(
                    new Expression(
                        new CallExpression(
                            new Expression(new  VarGetExpression(new Variable("test"))),
                            new List<Expression>()
                        )
                    )
                )
            };

            var input = @"
fun test() {
    print 10;
}
test();";

            var actual = Parser2.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
    }
}