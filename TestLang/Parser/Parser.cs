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
        private static List<Token> _tokens;

        public static List<Statement> Parse(string source)
        {
            _tokens = Lexer.Parse(source);
            _tokens.Reverse();

            var statements = new List<Statement>();

            while (HasNext())
            {
                var statement = ParseStatement();
                statements.Add(statement);
            }

            return statements;
        }

        private static Statement ParseStatement()
        {
            return Declaration();
        }

        private static Statement Declaration()
        {
            Statement statement;
            switch (PeekType())
            {
                case TokenType.Fun:
                    Next();
                    statement = DeclareFun();
                    break;
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

        private static Statement DeclareFun()
        {
            var identifier = Expect(TokenType.Identifier);

            Expect(TokenType.LeftParen);

            var parameters = new List<Variable>();

            while (!Check(TokenType.RightParen) && !Check(TokenType.EOF))
            {
                var param = Expect(TokenType.Identifier);
                parameters.Add(new Variable(param.Source));

                if (Check(TokenType.Comma))
                {
                    Next();
                }
                else
                {
                    break;
                }
            }

            Expect(TokenType.RightParen);
            Expect(TokenType.LeftBrace);

            var body = BlockStatement();

            var funDecl = new FunctionDeclaration(parameters, body);

            return new FunctionStatement(new Variable(identifier.Source), funDecl);
        }

        private static Statement Statement()
        {
            Statement result;
            switch (PeekType())
            {
                case TokenType.Print:
                    Next();
                    result = PrintStatement();
                    break;
                case TokenType.If:
                    Next();
                    result = IfStatement();
                    break;
                case TokenType.LeftBrace:
                    Next();
                    result = BlockStatement();
                    break;
                case TokenType.While:
                    Next();
                    result = WhileStatement();
                    break;
                case TokenType.For:
                    Next();
                    result = ForStatement();
                    break;
                default:
                    result = ExpressionStatement();
                    break;
            }

            return result;
        }

        private static Statement ForStatement()
        {
            Expect(TokenType.LeftParen);

            // Initializer
            Statement initializer = null;
            if (Match(TokenType.Semicolon))
            {
                // No initializer.
            }
            else if (Match(TokenType.Var))
            {
                initializer = DeclareVar();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            // Condition
            Expression condition = null;
            if (!Match(TokenType.Semicolon))
            {
                condition = ParseExpression(0);
                Expect(TokenType.Semicolon);
            }

            // Increment
            Expression increment = null;
            if (!Match(TokenType.RightParen))
            {
                increment = ParseExpression(0);
                Expect(TokenType.RightParen);
            }

            var body = ParseStatement();

            return new ForStatement(initializer, condition, increment, body);
        }

        private static Statement WhileStatement()
        {
            Expect(TokenType.LeftParen);
            var cond = ParseExpression(0);
            Expect(TokenType.RightParen);

            var stmt = ParseStatement();

            return new WhileStatement(cond, stmt);
        }

        private static Statement IfStatement()
        {
            Expect(TokenType.LeftParen);
            var cond = ParseExpression(0);
            Expect(TokenType.RightParen);
            var thenClause = Declaration();

            if (PeekType() is TokenType.Else)
            {
                Next();
                var elseClause = Declaration();
                return new IfElseStatement(cond, thenClause, elseClause);
            }

            return new IfStatement(cond, thenClause);
        }

        private static BlockStatement BlockStatement()
        {
            var block = new List<Statement>();
            while (!(PeekType() is TokenType.RightBrace) &&
                   !(PeekType() is TokenType.EOF))
            {
                block.Add(Declaration());
            }

            Expect(TokenType.RightBrace);
            return new BlockStatement(block);
        }

        private static Statement PrintStatement()
        {
            var val = ParseExpression(0);
            Expect(TokenType.Semicolon); // TODO
            return new Print(val);
        }

        private static Statement ExpressionStatement()
        {
            var expr = ParseExpression(0); // TODO pass precedence
            Expect(TokenType.Semicolon); // TODO
            return new StatementExpr(expr);
        }

        private static Expression ParseExpression(int minBp)
        {
            var lhs = Next();

            Expression expr;

            // Prefix
            switch (lhs.Type)
            {
                case TokenType.Number:
                    var number = Convert.ToDouble(lhs.Source);
                    expr = new Expression(new Number(number));
                    break;
                case TokenType.True:
                    expr = new Expression(new TrueLiteral());
                    break;
                case TokenType.False:
                    expr = new Expression(new FalseLiteral());
                    break;
                case TokenType.Nil:
                    expr = new Expression(new NilLiteral());
                    break;
                case TokenType.String:
                    expr = new Expression(new StringLiteral(lhs.Source));
                    break;
                case TokenType.Minus:
                case TokenType.Bang:
                    expr = ParseUnaryExpression(lhs.Type);
                    break;
                case TokenType.Identifier:
                    expr = ParseVariable(lhs);
                    break;
                default:
                    throw new Exception($"Token not covered: {lhs}");
            }

            for (; ; )
            {
                var op = PeekType();
                if (op is TokenType.EOF) break;

                // Infix
                var (leftBp, rightBp) = InfixBindingPower(op);
                if (leftBp < minBp) break;

                // Skip op
                Next();

                var rhs = ParseExpression(rightBp);

                // var binaryOp = op switch
                // {
                //     TokenType.EqualEqual => BinaryOperator.Equal,
                //     TokenType.BangEqual => BinaryOperator.BangEqual,
                //     TokenType.GreaterThan => BinaryOperator.GreaterThan,
                //     TokenType.GreaterThanEqual => BinaryOperator.GreaterThanEqual,
                //     TokenType.LessThan => BinaryOperator.LessThan,
                //     TokenType.LessThanEqual => BinaryOperator.LessThanEqual,
                //     TokenType.Minus => BinaryOperator.Minus,
                //     TokenType.Plus => BinaryOperator.Plus,
                //     TokenType.Slash => BinaryOperator.Slash,
                //     TokenType.Star => BinaryOperator.Star,
                // };

                // expr = new Expression(new BinaryExpression(expr, rhs, null));
            }

            return expr;
        }

        private static Expression ParseVariable(Token lhs)
        {
            var next = PeekType();
            switch (next)
            {
                case TokenType.Equal:
                    // Set

                    // Pop '=' operator
                    Next();

                    var rhs = ParseExpression(0);

                    var var = new Variable(lhs.Source);
                    return new Expression(new VarSetExpression(var, rhs));
                case TokenType.LeftParen:
                    return Call();
                default:
                    // Get
                    var var2 = new VarGetExpression(new Variable(lhs.Source));
                    return new Expression(var2); // TODO
            }

            // if (PeekType() is TokenType.Equal)
            // {
            //     // Set

            //     // Pop '=' operator
            //     Next();

            //     var rhs = ParseExpression(0);

            //     var var = new Variable(lhs.Source);
            //     return new Expression(new VarSetExpression(var, rhs));
            // }
            // else
            // {
            //     // Get
            //     var var = new VarGetExpression(new Variable(lhs.Source));
            //     return new Expression(var); // TODO
            // }
        }

        private static Expression ParseUnaryExpression(TokenType lhs)
        {
            var (_, rbp) = PrefixBindingPower(lhs);
            var rhs = ParseExpression(rbp);
            var op = lhs switch
            {
                TokenType.Minus => UnaryOperator.Minus,
                TokenType.Bang => UnaryOperator.Bang,
                // TokenType.LeftParen => Call(),
                _ => throw new ArgumentOutOfRangeException()
            };
            return new Expression(new UnaryExpression(op, rhs));
        }

        private static Expression Call()
        {
            // var expr = ParseExpression(0); // TODO Works???

            if (PeekType() is TokenType.LeftParen)
            {
                Next();
                // expr = FinishCall(expr);
                // return FinishCall();
                Expect(TokenType.RightParen);
                return new Expression(new CallExpression(null, new List<Expression>()));
            }

            throw new Exception("TODO");

            // return expr;
        }

        private static Expression FinishCall()
        {
            var args = new List<Expression>();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    args.Add(ParseExpression(0));
                } while (Match(TokenType.Comma));
            }

            Expect(TokenType.RightParen);

            return new Expression(new CallExpression(null, args));
        }

        private static (int, int) PrefixBindingPower(TokenType op)
        {
            return (10, 10);
            // return op switch
            // {
            //     TokenType.Minus => Precedence.PREC_UNARY,
            //     TokenType.Bang => Precedence.PREC_UNARY,
            //     _ => throw new Exception($"Bad op: {op}")
            // };
        }

        private static (int, int) InfixBindingPower(TokenType op)
        {
            return (10, 10);
            // return op switch
            // {
            //     TokenType.EqualEqual => Precedence.PREC_EQUALITY,
            //     TokenType.BangEqual => Precedence.PREC_EQUALITY,
            //     TokenType.GreaterThan => Precedence.PREC_COMPARISON,
            //     TokenType.GreaterThanEqual => Precedence.PREC_COMPARISON,
            //     TokenType.LessThan => Precedence.PREC_COMPARISON,
            //     TokenType.LessThanEqual => Precedence.PREC_COMPARISON,
            //     TokenType.Minus => Precedence.PREC_TERM,
            //     TokenType.Plus => Precedence.PREC_TERM,
            //     TokenType.Star => Precedence.PREC_FACTOR,
            //     TokenType.Slash => Precedence.PREC_FACTOR,
            //     TokenType.Semicolon => Precedence.PREC_NONE,
            //     TokenType.Equal => Precedence.PREC_ASSIGNMENT, // TODO correct???
            //     TokenType.LeftParen => Precedence.PREC_CALL, // TODO correct???
            //     TokenType.RightParen => Precedence.PREC_NONE, // TODO correct???
            //     _ => throw new Exception($"Bad op: {op}")
            // };
        }

        private static Statement DeclareVar()
        {
            var ident = Expect(TokenType.Identifier);
            var initializer = new Expression(new NilLiteral());

            if (PeekType() is TokenType.Equal)
            {
                Next();
                initializer = ParseExpression(0);
            }

            Expect(TokenType.Semicolon);

            return new VarStatement(new Variable(ident.Source), initializer);
        }

        private static bool HasNext()
        {
            return _tokens.Any();
        }

        private static Token Next()
        {
            var popped = _tokens[^1];
            _tokens.RemoveAt(_tokens.Count - 1);
            return popped;
        }

        private static TokenType PeekType()
        {
            return HasNext() ? _tokens[^1].Type : TokenType.EOF;
        }

        private static bool Match(TokenType type)
        {
            if (!Check(type))
            {
                return false;
            }

            Next();
            return true;
        }

        private static bool Check(TokenType type)
        {
            return PeekType() == type;
        }

        private static Token Expect(TokenType expect)
        {
            var next = PeekType();
            if (!expect.Equals(next))
            {
                throw new Exception($"Next is {next} not {expect}"); // TODO
            }

            return Next();
        }
    }
}