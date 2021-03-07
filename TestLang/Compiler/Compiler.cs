using System;
using testlang.ast;

namespace testlang
{
    // TODO Make singleton
    public class Compiler
    {
        public Chunk _chunk; // TODO Make private
        private Local[] _locals;
        private int _localCount;
        private int _scopeDepth;
        
        public Compiler()
        {
            _chunk = new Chunk("test"); // TODO
            _locals = new Local[byte.MaxValue];
            _localCount = 0;
            _scopeDepth = 0;
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
                case IfStatement ifStmt:
                    CompileIfStatement(ifStmt.Condition, ifStmt.ThenClause, null);
                    break;
                case IfElseStatement ifElseStmt:
                    CompileIfStatement(ifElseStmt.Condition, ifElseStmt.ThenClause, ifElseStmt.ElseClause);
                    break;
                case BlockStatement block:
                    CompileBlock(block);
                    break;
                case WhileStatement whileStmt:
                    CompileWhile(whileStmt);
                    break;
                default:
                    throw new Exception($"TODO {statement}");
            }
        }

        private void CompileWhile(WhileStatement whileStmt)
        {
            var loopStart = _chunk.Code.Count;
            CompileExpr(whileStmt.Condition);

            int exitJump = EmitJump(OpCode.JumpIfFalse);
            Emit(OpCode.Pop);
            CompileStatement(whileStmt.ThenClause);

            EmitLoop(loopStart);
            PatchJump(exitJump);
            Emit(OpCode.Pop);
        }

        private void EmitLoop(int loopStart)
        {
            Emit(OpCode.Loop);

            var offset = _chunk.Code.Count - loopStart + 2;

            EmitByte((byte)((offset >> 8) & 0xff));
            EmitByte((byte)(offset & 0xff));
        }

        private void CompileIfStatement(Expression condition, Statement thenClause, Statement elseClause)
        {
            CompileExpr(condition);

            // Jump to else clause if false
            var thenJump = EmitJump(OpCode.JumpIfFalse);
            Emit(OpCode.Pop);
            
            CompileStatement(thenClause);
            
            var elseJump = EmitJump(OpCode.Jump);
            
            PatchJump(thenJump);
            Emit(OpCode.Pop);

            if (elseClause != null)
            {
                CompileStatement(elseClause);
            }
            PatchJump(elseJump);
        }

        private int EmitJump(OpCode instruction)
        {
            Emit(instruction);
            EmitByte(0xff);
            EmitByte(0xff);
            return _chunk.Code.Count - 2;
        }

        private void PatchJump(int offset)
        {
            // -2 to adjust for the bytecode for the jump offset itself.
            var jump = _chunk.Code.Count - offset - 2;
            
            _chunk.Code[offset] = (byte)((jump >> 8) & 0xff);
            _chunk.Code[offset + 1] = (byte)(jump & 0xff);
        }

        private void CompileBlock(BlockStatement block)
        {
            BeginScope();
            foreach (var stmt in block.Statements)
            {
                CompileStatement(stmt);
            }
            EndScope();
        }

        private void CompileVarExpr(VarStatement var)
        {
            // TODO Check if initialized -> if not init with nil
            CompileExpr(var.Expr);

            if (_scopeDepth > 0)
            {
                // Local
                DeclareVar(var.Variable);
            }
            else
            {
                // Global
                DefineVar(var.Variable);
            }
        }

        private void DefineVar(Variable variable)
        {
            if (_scopeDepth > 0) return;
            
            // TODO only global
            Emit(OpCode.DefineGlobal);
            
            var strVal = Value.Obj(ObjString.CopyString(variable.Name));
            var constant = _chunk.AddConstant(strVal);
            _chunk.WriteChunk((byte) constant);
        }

        private void DeclareVar(Variable variable)
        {
            if (_scopeDepth == 0) return;

            for (var i = _localCount - 1; i >= 0; i -= 1) {
                var local = _locals[i];
                if (local.Depth != -1 && local.Depth < _scopeDepth) {
                    break;
                }

                if (variable.Name == local.Name)
                {
                    throw new Exception($"Already a variable called {variable.Name} in this scope.");
                }
            }
            
            AddLocal(variable.Name);
            MarkInitialized();
        }

        private void AddLocal(string name)
        {
            _locals[_localCount++] = new Local(name, -1);
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

                    var arg = ResolveLocal(set.Var.Name);
                    if (arg != -1)
                    {
                        // Local
                        Emit(OpCode.SetLocal);
                        _chunk.WriteChunk((byte) arg);
                    }
                    else
                    {
                        // Global
                        Emit(OpCode.SetGlobal);
                        var strVal2 = Value.Obj(ObjString.CopyString(set.Var.Name));
                        var constant2 = _chunk.AddConstant(strVal2);
                        _chunk.WriteChunk((byte) constant2);
                    }
                    
                    break;
                case VarGetExpression get:
                    var arg2 = ResolveLocal(get.Var.Name);
                    if (arg2 != -1)
                    {
                        // Local
                        Emit(OpCode.GetLocal);
                        _chunk.WriteChunk((byte) arg2); // TODO
                    }
                    else
                    {
                        // Global
                        Emit(OpCode.GetGlobal);
                        var strVal = Value.Obj(ObjString.CopyString(get.Var.Name));
                        var constant = _chunk.AddConstant(strVal);
                        _chunk.WriteChunk((byte) constant);
                    }

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

        private int ResolveLocal(string name) {
            for (var i = _localCount - 1; i >= 0; i -= 1) {
                var local = _locals[i];
                if (name == local.Name) {
                    if (local.Depth == -1) {
                        throw new Exception($"Can't read local variable {name} in it's own initializer.");
                    }
                    return i;
                }
            }

            return -1;
        }
        
        private void MarkInitialized()
        {
            if (_scopeDepth == 0) {
                return;
            }
            _locals[_localCount - 1].Depth = _scopeDepth;
        }

        private void BeginScope()
        {
            _scopeDepth += 1;
        }
        
        private void EndScope()
        {
            _scopeDepth -= 1;

            while (_localCount > 0 &&
                   _locals[_localCount - 1].Depth > _scopeDepth)
            {
                Emit(OpCode.Pop);
                _localCount -= 1;
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
        
        private void EmitByte(byte b)
        {
            _chunk.WriteChunk(b);
        }

        private void Emit(OpCode opCode)
        {
            _chunk.WriteChunk((byte) opCode);
        }
    }
}