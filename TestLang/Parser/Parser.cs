using System;
using System.Collections.Generic;
using System.Linq;
using testlang.Parser.ast;
using testlang.Scanner;
using testlang.Scanner.ast;

namespace testlang.Parser
{
    public static class Parser
    {
        private static List<Token> _tokens;

        public static List<Statement> Parse(string source)
        {
            _tokens = Lexer.Parse(source);
            _tokens.Reverse();

            var statements = new List<Statement>();
            while (HasNext())
            {
                var statement = Declaration();
                statements.Add(statement);
            }

            return statements;
        }

        private static Statement Declaration()
        {
            switch (PeekType())
            {
                case TokenType.Struct:
                    Next();
                    return DeclareStruct();
                case TokenType.Fun:
                    Next();
                    return DeclareFun();
                case TokenType.Var:
                    Next();
                    return DeclareVar();
                default:
                    return Statement();
            }
        }

        private static Statement Statement()
        {
            switch (PeekType())
            {
                case TokenType.Print:
                    Next();
                    return PrintStatement();
                case TokenType.If:
                    Next();
                    return IfStatement();
                case TokenType.LeftBrace:
                    Next();
                    return BlockStatement();
                case TokenType.While:
                    Next();
                    return WhileStatement();
                case TokenType.For:
                    Next();
                    return ForStatement();
                case TokenType.Return:
                    Next();
                    return ReturnStatement();
                default:
                    return ExpressionStatement();
            }
        }

        private static PrintStatement PrintStatement()
        {
            var statement = new PrintStatement(Expression());
            Consume(TokenType.Semicolon, "Expect ';' after expression.");
            return statement;
        }

        private static Statement IfStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
            var cond = Expression();
            Consume(TokenType.RightParen, "Expect ')' after condition.");
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
                   !(PeekType() is TokenType.Eof))
            {
                block.Add(Declaration());
            }

            Consume(TokenType.RightBrace, "Expect '}' after block.");

            return new BlockStatement(block);
        }

        private static Statement WhileStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
            var cond = Expression();
            Consume(TokenType.RightParen, "Expect ')' after condition.");

            var statement = Statement();

            return new WhileStatement(cond, statement);
        }

        private static Statement ForStatement()
        {
            Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

            // Initializer
            Statement initializer = null;
            switch (Next().Type)
            {
                case TokenType.Semicolon:
                    // No initializer.
                    break;
                case TokenType.Var:
                    initializer = DeclareVar();
                    break;
                default:
                    initializer = ExpressionStatement();
                    break;
            }

            // Condition
            Expression condition = null;
            if (!Match(TokenType.Semicolon))
            {
                condition = Expression();
                Consume(TokenType.Semicolon, "Expect ';' after loop condition.");
            }

            // Increment
            Expression increment = null;
            if (!Match(TokenType.RightParen))
            {
                increment = Expression();
                Consume(TokenType.RightParen, "Expect ')' after for clauses.");
            }

            var body = Statement();

            return new ForStatement(initializer, condition, increment, body);
        }

        public static ReturnStatement ReturnStatement()
        {
            if (PeekType() is TokenType.Semicolon)
            {
                Next();
                return new ReturnStatement(null);
            }

            var expr = Expression();
            Consume(TokenType.Semicolon, ""); // TODO
            return new ReturnStatement(expr);
        }

        public static IExpressionKind ParseVariable(Token lhs)
        {
            var next = PeekType();
            switch (next)
            {
                case TokenType.Equal:
                    // Set

                    // Pop '=' operator
                    Next();

                    var rhs = Expression();

                    var var = new Variable(lhs.Source);
                    return new VarSetExpression(var, rhs);
                default:
                    // Get
                    var var2 = new VarGetExpression(new Variable(lhs.Source));
                    return var2; // TODO
            }
        }

        public static IExpressionKind Call(Token token)
        {
            var args = new List<Expression>();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    args.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, "Expect ')' after arguments");

            return new CallExpression(null, args);
        }

        public static IExpressionKind Dot(Token token)
        {
            var name = Consume(TokenType.Identifier, "Expect property name after '.'.");

            if (Match(TokenType.Equal))
            {
                // SETTER
                var value = Expression();
                return new SetExpression(name.Source, null, value);
            }

            // GETTER
            return new GetExpression(name.Source, null);
        }

        private static ExpressionStatement ExpressionStatement()
        {
            var expr = new ExpressionStatement(Expression());
            Consume(TokenType.Semicolon, "Expect ';' after expression.");
            return expr;
        }

        private static Statement DeclareStruct()
        {
            var structName = Consume(TokenType.Identifier, "Expect struct name.");

            Consume(TokenType.LeftBrace, "Expect '{' before struct body.");
            Consume(TokenType.RightBrace, "Expect '}' after struct body.");
            return new StructStatement(new Variable(structName.Source));
        }

        private static Statement DeclareFun()
        {
            var identifier = Consume(TokenType.Identifier, ""); // TODO

            Consume(TokenType.LeftParen, ""); // TODO

            var parameters = new List<Variable>();

            while (!Check(TokenType.RightParen) && !Check(TokenType.Eof))
            {
                var param = Consume(TokenType.Identifier, ""); // TODO
                parameters.Add(new Variable(param.Source));

                if (Check(TokenType.Comma))
                {
                    Next();
                }
                else break;
            }

            Consume(TokenType.RightParen, ""); // TODO
            Consume(TokenType.LeftBrace, ""); // TODO

            var body = BlockStatement();

            var funDecl = new FunctionDeclaration(parameters, body);

            return new FunctionStatement(new Variable(identifier.Source), funDecl);
        }

        private static VarStatement DeclareVar()
        {
            var ident = Consume(TokenType.Identifier, "Expect identifier"); // TODO
            var initializer = new Expression(new NilLiteral());

            if (PeekType() is TokenType.Equal)
            {
                Next();
                initializer = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after expression.");

            return new VarStatement(new Variable(ident.Source), initializer);
        }

        private static Expression Expression()
        {
            return ParsePrecedence(Precedence.Assignment);
        }

        private static Expression ParsePrecedence(Precedence precedence)
        {
            var lhsToken = Next();
            var prefixRule = ParserRules.GetRule(lhsToken.Type).Prefix; // TODO Previous ???

            if (prefixRule == null)
            {
                throw new Exception("Expect expression");
            }

            var expr = prefixRule(lhsToken);

            while (precedence < ParserRules.GetRule(PeekType()).Precedence)
            {
                var rhsToken = Next();
                var infixRule = ParserRules.GetRule(rhsToken.Type).Infix;
                var infixExpr = infixRule(rhsToken);

                switch (infixExpr)
                {
                    case BinaryExpression binary2:
                        binary2.Lhs = new Expression(expr);
                        expr = binary2;
                        break;
                    case CallExpression call:
                        call.Callee = new Expression(expr);
                        expr = call;
                        break;
                    case SetExpression set:
                        set.Expr = new Expression(expr);
                        expr = set;
                        break;
                    case GetExpression get:
                        get.Expr = new Expression(expr);
                        expr = get;
                        break;
                    default:
                        throw new Exception("TODO");
                }
            }

            return new Expression(expr);
        }

        public static IExpressionKind Binary(Token token)
        {
            var operatorType = token.Type;

            var rule = ParserRules.GetRule(operatorType);
            var rhs = ParsePrecedence(rule.Precedence + 1);

            var op = operatorType switch
            {
                TokenType.BangEqual => BinaryOperator.BangEqual,
                TokenType.EqualEqual => BinaryOperator.Equal,
                TokenType.GreaterThan => BinaryOperator.GreaterThan,
                TokenType.GreaterThanEqual => BinaryOperator.GreaterThanEqual,
                TokenType.LessThan => BinaryOperator.LessThan,
                TokenType.LessThanEqual => BinaryOperator.LessThanEqual,
                TokenType.Plus => BinaryOperator.Plus,
                TokenType.Minus => BinaryOperator.Minus,
                TokenType.Star => BinaryOperator.Multiply,
                TokenType.Slash => BinaryOperator.Divide,
                TokenType.Percent => BinaryOperator.Modulo,
                _ => throw new Exception() // TODO
            };

            return new BinaryExpression(op, null, rhs);
        }

        public static IExpressionKind Unary(Token token)
        {
            var operatorType = token.Type;
            
            var expr = Expression();

            var op = operatorType switch
            {
                TokenType.Minus => UnaryOperator.Negate,
                TokenType.Bang => UnaryOperator.Not,
                _ => throw new Exception() // TODO
            };

            return new UnaryExpression(op, expr);
        }

        public static IExpressionKind Grouping(Token token)
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new GroupingExpression(expr);
        }

        public static IExpressionKind Number(Token token)
        {
            var number = Convert.ToDouble(token.Source);
            return new Number(number);
        }

        public static IExpressionKind String(Token token)
        {
            return new StringLiteral(token.Source);
        }
        
        public static IExpressionKind Literal(Token token)
        {
            return token.Type switch
            {
                TokenType.False => new FalseLiteral(),
                TokenType.True => new TrueLiteral(),
                TokenType.Nil => new NilLiteral(),
                _ => throw new Exception("Unreachable code reached"), // TODO
            };
        }

        private static Token Next()
        {
            var popped = _tokens[^1];
            _tokens.RemoveAt(_tokens.Count - 1);
            return popped;
        }

        private static Token Consume(TokenType type, string message)
        {
            if (PeekType() == type)
            {
                return Next();
            }

            throw new Exception(message); // TODO
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

        private static TokenType PeekType()
        {
            return HasNext() ? _tokens[^1].Type : TokenType.Eof;
        }

        private static bool HasNext()
        {
            return _tokens.Any();
        }
    }
}