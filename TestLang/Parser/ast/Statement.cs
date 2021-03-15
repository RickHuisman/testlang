using System.Collections.Generic;

namespace testlang.Parser.ast
{
    public abstract class Statement
    {
        public abstract override string ToString();
    }

    public class PrintStatement : Statement
    {
        public Expression Expr { get; }

        public PrintStatement(Expression expr)
        {
            Expr = expr;
        }

        public override string ToString()
        {
            return $"{nameof(Expr)}: {Expr}";
        }
    }

    public class ExpressionStatement : Statement
    {
        public Expression Expr { get; }

        public ExpressionStatement(Expression expr)
        {
            Expr = expr;
        }

        public override string ToString()
        {
            return $"{nameof(Expr)}: {Expr}";
        }
    }

    public class VarStatement : Statement
    {
        public Variable Variable { get; }
        public Expression Expr { get; }

        public VarStatement(Variable variable, Expression expr)
        {
            Variable = variable;
            Expr = expr;
        }

        public override string ToString()
        {
            return $"{nameof(Variable)}: {Variable}, {nameof(Expr)}: {Expr}";
        }
    }

    public class BlockStatement : Statement
    {
        public List<Statement> Statements { get; }

        public BlockStatement(List<Statement> statements)
        {
            Statements = statements;
        }

        public override string ToString()
        {
            return $"{nameof(Statements)}: {Statements}";
        }
    }

    public class IfStatement : Statement
    {
        public Expression Condition { get; }
        public Statement ThenClause { get; }

        public IfStatement(Expression condition, Statement thenClause)
        {
            Condition = condition;
            ThenClause = thenClause;
        }

        public override string ToString()
        {
            return $"{nameof(Condition)}: {Condition}, {nameof(ThenClause)}: {ThenClause}";
        }
    }


    public class IfElseStatement : Statement
    {
        public Expression Condition { get; }
        public Statement ThenClause { get; }
        public Statement ElseClause { get; }

        public IfElseStatement(Expression condition, Statement thenClause, Statement elseClause)
        {
            Condition = condition;
            ThenClause = thenClause;
            ElseClause = elseClause;
        }

        public override string ToString()
        {
            return
                $"{nameof(Condition)}: {Condition}, {nameof(ThenClause)}: {ThenClause}, {nameof(ElseClause)}: {ElseClause}";
        }
    }

    public class WhileStatement : Statement
    {
        public Expression Condition { get; }
        public Statement ThenClause { get; }

        public WhileStatement(Expression condition, Statement thenClause)
        {
            Condition = condition;
            ThenClause = thenClause;
        }

        public override string ToString()
        {
            return $"{nameof(Condition)}: {Condition}, {nameof(ThenClause)}: {ThenClause}";
        }
    }

    public class ForStatement : Statement
    {
        public Statement VarDeclaration { get; }
        public Expression Condition { get; }
        public Expression Increment { get; }
        public Statement Body { get; }

        public ForStatement(Statement varDeclaration, Expression condition, Expression increment, Statement body)
        {
            VarDeclaration = varDeclaration;
            Condition = condition;
            Increment = increment;
            Body = body;
        }

        public override string ToString()
        {
            return
                $"{nameof(VarDeclaration)}: {VarDeclaration}, {nameof(Condition)}: {Condition}, {nameof(Increment)}: {Increment}, {nameof(Body)}: {Body}";
        }
    }

    public class FunctionStatement : Statement
    {
        public Variable Variable { get; }
        public FunctionDeclaration Declaration { get; }

        public FunctionStatement(Variable variable, FunctionDeclaration declaration)
        {
            Variable = variable;
            Declaration = declaration;
        }

        public override string ToString()
        {
            return $"{nameof(Variable)}: {Variable}, {nameof(Declaration)}: {Declaration}";
        }
    }

    public class ReturnStatement : Statement
    {
        public Expression Expr { get; }

        public ReturnStatement(Expression expr)
        {
            Expr = expr;
        }

        public override string ToString()
        {
            return $"{nameof(Expr)}: {Expr}";
        }
    }

    public class StructStatement : Statement
    {
        public Variable Name { get; } // TODO Rename
        
        public StructStatement(Variable name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}";
        }
    }

    public class FunctionDeclaration
    {
        public List<Variable> Parameters { get; }
        public BlockStatement Body { get; }

        public FunctionDeclaration(List<Variable> parameters, BlockStatement body)
        {
            Parameters = parameters;
            Body = body;
        }

        public override string ToString()
        {
            return $"{nameof(Parameters)}: {Parameters}, {nameof(Body)}: {Body}";
        }
    }

    public class Variable
    {
        public string Name { get; }

        public Variable(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{nameof(Variable)}: {Name}";
        }
    }
}