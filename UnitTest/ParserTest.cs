using System;
using System.Collections.Generic;
using NUnit.Framework;
using testlang.Parser;
using testlang.Parser.ast;
using testlang.Scanner;
using testlang.Scanner.ast;

namespace UnitTest
{
    public class Parser2Test
    {
        [Test]
        public void Parse_ModuloExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new ExpressionStatement(
                    new Expression(
                        new BinaryExpression(
                            BinaryOperator.Modulo,
                            new Expression(new Number(5)),
                            new Expression(new Number(10)))
                    )
                )
            };

            var input = "5 % 10;";
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_NumberExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new ExpressionStatement(
                    new Expression(
                        new BinaryExpression(
                            BinaryOperator.Plus,
                            new Expression(new Number(5)),
                            new Expression(new Number(10)))
                    )
                )
            };

            var input = "print 5 + 10;";
            var actual = Parser.Parse(input);

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
            var actual = Parser.Parse(input);

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
                                new PrintStatement(
                                    new Expression(new Number(10)))
                            })
                    )
                ),
                new ExpressionStatement(
                    new Expression(
                        new CallExpression(
                            new Expression(new VarGetExpression(new Variable("test"))),
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

            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_StringExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new ExpressionStatement(
                    new Expression(
                        new BinaryExpression(
                            BinaryOperator.Equal,
                            new Expression(new StringLiteral("test")),
                            new Expression(new TrueLiteral()))
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
                new ExpressionStatement(
                    new Expression(
                        new BinaryExpression(
                            BinaryOperator.BangEqual,
                            new Expression(new TrueLiteral()),
                            new Expression(new FalseLiteral()))
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
                new ExpressionStatement(
                    new Expression(
                        new BinaryExpression(
                            BinaryOperator.BangEqual,
                            new Expression(new NilLiteral()),
                            new Expression(new FalseLiteral()))
                    )
                )
            };

            var input = "nil != false;";
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_SetVarExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new VarStatement(
                    new Variable("x"),
                    new Expression(new Number(10))
                ),
                new ExpressionStatement(
                    new Expression(new VarSetExpression(new Variable("x"), new Expression(new Number(20)))))
            };

            var input = @"var x = 10;
x = 20;";
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
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

        [Test]
        public void Parse_Fun_ReturnsAst()
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
                                new PrintStatement(
                                    new Expression(new Number(10)))
                            })
                    )
                )
            };

            var input = @"
fun test() {
    print 10;
}";

            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }

        [Test]
        public void Parse_UnaryExpr_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new ExpressionStatement(
                    new Expression(
                        new UnaryExpression(
                            UnaryOperator.Negate,
                            new Expression(new Number(5))
                        )
                    )
                )
            };

            var input = "-5;";
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_Struct_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new StructStatement(
                    new Variable("Foo")
                    )
            };

            const string input = @"
struct Foo {
}";
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
        
        [Test]
        public void Parse_CallStruct_ReturnsAst()
        {
            var expected = new List<Statement>
            {
                new StructStatement(
                    new Variable("Foo")
                ),
            };

            const string input = @"
struct Foo {
}
print Foo();";
            var actual = Parser.Parse(input);

            TestHelper.AreEqualByJson(expected, actual);
        }
    }
}