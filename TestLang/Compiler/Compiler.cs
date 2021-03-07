using System;
using testlang.ast;

namespace testlang
{
    public class Compiler
    {
        public Chunk _chunk; // TODO Make private
        
        public Compiler()
        {
            _chunk = new Chunk("test"); // TODO
        }

        public void Compile(string source)
        {
            var statements = Parser.Parse(source);

            foreach (var statement in statements)
            {
                CompileStatement(statement);
            }
        }

        private void CompileStatement(Statement statement)
        {
            switch (statement)
            {
                case VarStatement var:
                    CompileVarExpr(var);
                    break;
                case StatementExpr expr:
                    CompileExpr(expr.Expr);
                    Emit(OpCode.Pop);
                    break;
                case Print expr:
                    CompileExpr(expr.Expr);
                    Emit(OpCode.Print);
                    break;
            }
        }

        private void CompileVarExpr(VarStatement var)
        {
            // TODO Check if initialized -> if not init with nil
            CompileExpr(var.Expr);
            DefineVar(var.Variable);
        }

        private void DefineVar(Variable varVariable)
        {
            // TODO only global
            Emit(OpCode.DefineGlobal);
            
            var strVal = Value.Obj(ObjString.CopyString(varVariable.Name));
            var constant = _chunk.AddConstant(strVal);
            _chunk.WriteChunk((byte) constant);
        }

        private void CompileExpr(Expression expr)
        {
            switch (expr.Node)
            {
                case BinaryExpression binary:
                    CompileBinaryExpr(binary);
                    break;
                case Literal literal:
                    EmitLiteral(literal);
                    break;
                case VarSetExpression set:
                    CompileExpr(set.Expr);
                    Emit(OpCode.SetGlobal);
                    var strVal2 = Value.Obj(ObjString.CopyString(set.Var.Name));
                    var constant2 = _chunk.AddConstant(strVal2);
                    _chunk.WriteChunk((byte) constant2);
                    break;
                case VarGetExpression get:
                    Emit(OpCode.GetGlobal);
                    var strVal = Value.Obj(ObjString.CopyString(get.Var.Name));
                    var constant = _chunk.AddConstant(strVal);
                    _chunk.WriteChunk((byte) constant);
                    break;
                default:
                    throw new Exception($"TODO {expr.Node}");
            }
        }

        private void CompileBinaryExpr(BinaryExpression expr)
        {
            CompileExpr(expr.Lhs);
            CompileExpr(expr.Rhs);

            switch (expr.Operator)
            {
                case BinaryOperator.Equal:
                    Emit(OpCode.Equal);
                    break;
                case BinaryOperator.BangEqual:
                    Emit(OpCode.Equal);
                    Emit(OpCode.Not);
                    break;
                case BinaryOperator.GreaterThan:
                    Emit(OpCode.Greater);
                    break;
                case BinaryOperator.GreaterThanEqual:
                    Emit(OpCode.Less);
                    Emit(OpCode.Not);
                    break;
                case BinaryOperator.LessThan:
                    Emit(OpCode.Less);
                    break;
                case BinaryOperator.LessThanEqual:
                    Emit(OpCode.Greater);
                    Emit(OpCode.Not);
                    break;
                case BinaryOperator.Minus:
                    Emit(OpCode.Subtract);
                    break;
                case BinaryOperator.Plus:
                    Emit(OpCode.Add);
                    break;
                case BinaryOperator.Slash:
                    Emit(OpCode.Divide);
                    break;
                case BinaryOperator.Star:
                    Emit(OpCode.Multiply);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EmitLiteral(Literal literal)
        {
            switch (literal)
            {
                case Number number:
                    EmitConstant(Value.Number(number.Value));
                    break;
                case TrueLiteral _:
                    EmitConstant(Value.Bool(true));
                    break;
                case FalseLiteral _:
                    EmitConstant(Value.Bool(false));
                    break;
                case StringLiteral str:
                    // EmitConstant(Value.Obj(ObjString.CopyString(str.Value)));
                    throw new ArgumentOutOfRangeException(); // TODO
                    break;
                default:
                    throw new ArgumentOutOfRangeException(); // TODO
            }
        }

        private void EmitConstant(Value val)
        {
            var constant = _chunk.AddConstant(val);
            _chunk.WriteChunk((byte) OpCode.Constant);
            _chunk.WriteChunk((byte) constant);
        }

        private void Emit(OpCode opCode)
        {
            _chunk.WriteChunk((byte) opCode);
        }
    }
}