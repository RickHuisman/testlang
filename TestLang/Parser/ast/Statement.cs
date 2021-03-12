using System.Collections.Generic;

namespace testlang.ast
{
    public class Statement
    {
    }

    public class Print : Statement
    {
        public Expression Expr { get; }
        
        public Print(Expression expr)
        {
            Expr = expr;
        }

        public override string ToString()
        {
            return $"[Print {Expr}]";
        }
    }

    public class StatementExpr : Statement
    {
        public Expression Expr { get; }
        
        public StatementExpr(Expression expr)
        {
            Expr = expr;
        }
        
        public override string ToString()
        {
            return $"[Expr {Expr}]";
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
            return $"[VarStatement {Expr}]";
        }
    }

    public class BlockStatement : Statement
    {
        public List<Statement> Statements { get; }

        public BlockStatement(List<Statement> statements)
        {
            Statements = statements;
        }
    }

    public class Variable
    {
        public string Name { get; }

        public Variable(string name)
        {
            Name = name;
        }
    }

    public class IfStatement : Statement
    {
        public Expression Condition;
        public Statement ThenClause;

        public IfStatement(Expression condition, Statement thenClause)
        {
            Condition = condition;
            ThenClause = thenClause;
        }
    }
    
    
    public class IfElseStatement : Statement
    {
        public Expression Condition;
        public Statement ThenClause;
        public Statement ElseClause;

        public IfElseStatement(Expression condition, Statement thenClause, Statement elseClause)
        {
            Condition = condition;
            ThenClause = thenClause;
            ElseClause = elseClause;
        }
    }

    public class WhileStatement : Statement
    {
        public Expression Condition;
        public Statement ThenClause;

        public WhileStatement(Expression condition, Statement thenClause)
        {
            Condition = condition;
            ThenClause = thenClause;
        }
    }

    public class ForStatement : Statement
    {
        public Statement VarDeclaration;
        public Expression Condition;
        public Expression Increment;
        public Statement Body;

        public ForStatement(Statement varDeclaration, Expression condition, Expression increment, Statement body)
        {
            VarDeclaration = varDeclaration;
            Condition = condition;
            Increment = increment;
            Body = body;
        }
    }

    public class FunctionStatement : Statement
    {
        public Variable Variable;
        public FunctionDeclaration Declaration;

        public FunctionStatement(Variable variable, FunctionDeclaration declaration)
        {
            Variable = variable;
            Declaration = declaration;
        }
    }
    
    public class FunctionDeclaration
    {
        public List<Variable> Parameters;
        public BlockStatement Body;

        public FunctionDeclaration(List<Variable> parameters, BlockStatement body)
        {
            Parameters = parameters;
            Body = body;
        }
    }

    public class ReturnStatement : Statement
    {
        public Expression Expr;
        
        public ReturnStatement(Expression expr)
        {
            Expr = expr;
        }
    }
}