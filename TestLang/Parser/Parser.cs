using System;
using System.Collections.Generic;
using System.Linq;
using testlang.ast;
using BinaryExpression = testlang.ast.BinaryExpression;
using Expression = testlang.ast.Expression;

namespace testlang
{
    public class Parser
    {
        private List<Token> _tokens;

        public Parser(string source)
        {
            _tokens = Lexer.Parse(source);
            _tokens.Reverse();
        }

        public List<Statement> Parse()
        {
            var statements = new List<Statement>();

            while (HasNext())
            {
                var statement = ParseStatement();
                statements.Add(statement);
            }

            return statements;
        }

        private Statement ParseStatement()
        {
            return Declaration();
        }

        private Statement Declaration()
        {
            Statement statement;
            switch (PeekType())
            {
                case TokenType.Var:
                    Next();
                    statement = DeclareVar();
                    break;
                default:
                    statement = Statement();
                    break;
            }

            return statement;
        }

        private Statement Statement()
        {
            Statement result;
            switch (PeekType())
            {
                case TokenType.Print:
                    Next();
                    result = PrintStatement();
                    break;
                default:
                    result = ExpressionStatement();
                    break;
                // throw new Exception(); // TODO custom
            }

            return result;
        }

        private Statement PrintStatement()
        {
            var val = ParseExpression(0);
            return new Print(val);
            // TODO Expect semicolon after expression
        }

        private Statement ExpressionStatement()
        {
            var expr = ParseExpression(0);
            Expect(TokenType.Semicolon); // TODO
            return new StatementExpr(expr);
        }

        private Expression ParseExpression(int minBp)
        {
            var lhs = Next();

            Expression expr;
            switch (lhs.Type)
            {
                case TokenType.Number:
                    var number = Convert.ToDouble(lhs.Source);
                    expr = new Expression(new Number(number));
                    break;
                case TokenType.Minus:
                case TokenType.Bang:
                    expr = ParseUnaryExpression(lhs.Type);
                    break;
                default:
                    throw new Exception($"Token not covered: {lhs}");
            }

            for (;;)
            {
                var op = PeekType();
                if (op is TokenType.EOF) break;

                var (leftBp, rightBp) = InfixBindingPower(op);
                if (leftBp < minBp) break;

                // Skip op
                Next();

                var rhs = ParseExpression(rightBp);

                var binaryOp = op switch
                {
                    TokenType.Equal => BinaryOperator.Equal,
                    TokenType.BangEqual => BinaryOperator.BangEqual,
                    TokenType.GreaterThan => BinaryOperator.GreaterThan,
                    TokenType.GreaterThanEqual => BinaryOperator.GreaterThanEqual,
                    TokenType.LessThan => BinaryOperator.LessThan,
                    TokenType.LessThanEqual => BinaryOperator.LessThanEqual,
                    TokenType.Minus => BinaryOperator.Minus,
                    TokenType.Plus => BinaryOperator.Plus,
                    TokenType.Slash => BinaryOperator.Slash,
                    TokenType.Star => BinaryOperator.Star,
                };
                
                expr = new Expression(new BinaryExpression(expr, rhs, binaryOp));
            }

            return expr;
        }

        private Expression ParseUnaryExpression(TokenType lhs)
        {
            var (_, rbp) = PrefixBindingPower(lhs);
            var rhs = ParseExpression(rbp);
            var op = lhs switch
            {
                TokenType.Minus => UnaryOperator.Minus,
                TokenType.Bang => UnaryOperator.Bang,
                _ => throw new ArgumentOutOfRangeException()
            };
            return new Expression(new UnaryExpression(op, rhs));
        }

        private (int, int) PrefixBindingPower(TokenType op)
        {
            return op switch
            {
                TokenType.Minus => Precedence.PREC_UNARY,
                TokenType.Bang => Precedence.PREC_UNARY,
                _ => throw new Exception($"Bad op: {op}")
            };
        }

        private (int, int) InfixBindingPower(TokenType op)
        {
            return op switch
            {
                TokenType.EqualEqual => Precedence.PREC_EQUALITY,
                TokenType.BangEqual => Precedence.PREC_EQUALITY,
                TokenType.GreaterThan => Precedence.PREC_COMPARISON,
                TokenType.GreaterThanEqual => Precedence.PREC_COMPARISON,
                TokenType.LessThan => Precedence.PREC_COMPARISON,
                TokenType.LessThanEqual => Precedence.PREC_COMPARISON,
                TokenType.Minus => Precedence.PREC_TERM,
                TokenType.Plus => Precedence.PREC_TERM,
                TokenType.Star => Precedence.PREC_FACTOR,
                TokenType.Slash => Precedence.PREC_FACTOR,
                TokenType.Semicolon => Precedence.PREC_NONE,
                _ => throw new Exception($"Bad op: {op}")
            };
        }

        private Statement DeclareVar()
        {
            throw new Exception();
        }

        private bool HasNext()
        {
            return _tokens.Any();
        }

        private Token Next()
        {
            var popped = _tokens[^1];
            _tokens.RemoveAt(_tokens.Count - 1);
            return popped;
        }

        private TokenType PeekType()
        {
            return HasNext() ? _tokens[^1].Type : TokenType.EOF;
        }

        private void Expect(TokenType expect)
        {
            var next = PeekType();
            if (!expect.Equals(next))
            {
                throw new Exception("TODO"); // TODO
            }

            Next();
        }
    }
}