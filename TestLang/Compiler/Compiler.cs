using System;
using testlang.ast;

namespace testlang
{
    // TODO Make singleton
    public class Compiler
    {
        private Instance current;

        class Instance
        {
            internal Instance()
            {
                locals = new Local[byte.MaxValue];
                upValues = new UpValue[256]; // TODO int ???
                localCount = 0;
                scopeDepth = 0;
                function = new ObjFunction();

                locals[localCount++] = new Local("", 0);
            }

            internal ObjFunction function;
            internal FunctionType type;

            internal readonly Local[] locals;
            internal int localCount;
            internal UpValue[] upValues;
            internal int scopeDepth;

            internal Instance enclosing;
        }

        public Compiler(FunctionType type)
        {
            current = new Instance
            {
                type = type,
                enclosing = null
            };
        }

        private Chunk CurrentChunk()
        {
            return current.function.Chunk;
        }

        public ObjFunction Compile(string source)
        {
            var statements = Parser.Parse(source);

            foreach (var statement in statements)
            {
                CompileStatement(statement);
            }

            return EndCompiler();
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
                case ForStatement forStmt:
                    CompileFor(forStmt);
                    break;
                case FunctionStatement funStmt:
                    CompileFun(funStmt);
                    DefineVar(funStmt.Variable);
                    break;
                case ReturnStatement returnStmt:
                    CompileReturn(returnStmt);
                    break;
                default:
                    throw new Exception($"TODO {statement}");
            }
        }

        private void CompileReturn(ReturnStatement stmt)
        {
            if (current.type == FunctionType.Script) {
                throw new Exception("Can't return from top level code."); // TODO
            }

            if (stmt.Expr != null)
            {
                CompileExpr(stmt.Expr);
                Emit(OpCode.Return);
            }
            else
            {
                Emit(OpCode.Nil);
                Emit(OpCode.Return);
            }
        }

        private void CompileFun(FunctionStatement fun)
        {
            current = new Instance
            {
                type = FunctionType.Function, // TODO
                enclosing = current,
                function = { Name = ObjString.CopyString(fun.Variable.Name) },
            };

            BeginScope();

            // TODO Parameters
            foreach (var p in fun.Declaration.Parameters)
            {
                current.function.Arity += 1;
                DeclareVar(p);
            }

            // The body.
            CompileBlock(fun.Declaration.Body);

            // Create the function object.
            var test = EndCompiler();
            
            Emit(OpCode.Closure);
            EmitByte(MakeConstant(Value.Obj(test)));
        }

        private void CompileFor(ForStatement forStmt)
        {
            BeginScope();
            if (forStmt.VarDeclaration != null)
            {
                if (forStmt.VarDeclaration is VarStatement statement)
                {
                    // Declare var
                    CompileVarExpr(statement);
                }
                else
                {
                    // Expression
                    CompileStatement(forStmt.VarDeclaration);
                }
            }

            var loopStart = CurrentChunk().Code.Count;

            var exitJump = -1;
            if (forStmt.Condition != null)
            {
                CompileExpr(forStmt.Condition);

                // Jump out of loop if condition is false
                exitJump = EmitJump(OpCode.JumpIfFalse);
                Emit(OpCode.Pop);
            }

            if (forStmt.Increment != null)
            {
                var bodyJump = EmitJump(OpCode.Jump);

                var incrementStart = CurrentChunk().Code.Count;
                CompileExpr(forStmt.Increment);
                Emit(OpCode.Pop);

                EmitLoop(loopStart);
                loopStart = incrementStart;
                PatchJump(bodyJump);
            }

            CompileStatement(forStmt.Body);

            EmitLoop(loopStart);

            if (exitJump != -1)
            {
                PatchJump(exitJump);
                Emit(OpCode.Pop);
            }

            EndScope();
        }

        private void CompileWhile(WhileStatement whileStmt)
        {
            var loopStart = CurrentChunk().Code.Count;
            CompileExpr(whileStmt.Condition);

            var exitJump = EmitJump(OpCode.JumpIfFalse);
            Emit(OpCode.Pop);
            CompileStatement(whileStmt.ThenClause);

            EmitLoop(loopStart);
            PatchJump(exitJump);
            Emit(OpCode.Pop);
        }

        private void EmitLoop(int loopStart)
        {
            Emit(OpCode.Loop);

            var offset = CurrentChunk().Code.Count - loopStart + 2;

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
            return CurrentChunk().Code.Count - 2;
        }

        private void PatchJump(int offset)
        {
            // -2 to adjust for the bytecode for the jump offset itself.
            var jump = CurrentChunk().Code.Count - offset - 2;

            CurrentChunk().Code[offset] = (byte)((jump >> 8) & 0xff);
            CurrentChunk().Code[offset + 1] = (byte)(jump & 0xff);
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

            if (current.scopeDepth > 0)
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
            if (current.scopeDepth > 0) {
                MarkInitialized();
                return;
            }

            Emit(OpCode.DefineGlobal);

            var strVal = Value.Obj(ObjString.CopyString(variable.Name));
            var constant = CurrentChunk().AddConstant(strVal);
            CurrentChunk().WriteChunk((byte)constant);
        }

        private void DeclareVar(Variable variable)
        {
            if (current.scopeDepth == 0) return;

            for (var i = current.localCount - 1; i >= 0; i -= 1)
            {
                var local = current.locals[i];
                if (local.Depth != -1 && local.Depth < current.scopeDepth)
                {
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
            current.locals[current.localCount++] = new Local(name, -1);
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
                        CurrentChunk().WriteChunk((byte)arg);
                    }
                    else if ((arg = ResolveUpValue(set.Var.Name)) != -1) {
                        // TODO
                        // getOp = OP_GET_UPVALUE;
                        // setOp = OP_SET_UPVALUE;
                    }
                    else
                    {
                        // Global
                        Emit(OpCode.SetGlobal);
                        var strVal2 = Value.Obj(ObjString.CopyString(set.Var.Name));
                        var constant2 = CurrentChunk().AddConstant(strVal2);
                        CurrentChunk().WriteChunk((byte)constant2);
                    }

                    break;
                case VarGetExpression get:
                    var arg2 = ResolveLocal(get.Var.Name);
                    if (arg2 != -1)
                    {
                        // Local
                        Emit(OpCode.GetLocal);
                        CurrentChunk().WriteChunk((byte)arg2); // TODO
                    }
                    else
                    {
                        // Global
                        Emit(OpCode.GetGlobal);
                        var strVal = Value.Obj(ObjString.CopyString(get.Var.Name));
                        var constant = CurrentChunk().AddConstant(strVal);
                        CurrentChunk().WriteChunk((byte)constant);
                    }
                    break;
                case CallExpression call:
                    var arity = call.Arguments.Count;
                    if (arity > 8)
                    {
                        throw new Exception("Too many arguments"); // TODO
                    }

                    CompileExpr(call.Callee);

                    foreach (var varArg in call.Arguments)
                    {
                        CompileExpr(varArg);
                    }

                    Emit(OpCode.Call);
                    EmitByte((byte)arity);

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
                case BinaryOperator.Multiply:
                    Emit(OpCode.Multiply);
                    break;
                case BinaryOperator.Divide:
                    Emit(OpCode.Divide);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int ResolveLocal(string name)
        {
            for (var i = current.localCount - 1; i >= 0; i -= 1)
            {
                var local = current.locals[i];
                if (name == local.Name)
                {
                    if (local.Depth == -1)
                    {
                        throw new Exception($"Can't read local variable {name} in it's own initializer.");
                    }

                    return i;
                }
            }

            return -1;
        }
        
        private int ResolveUpValue(string varName) {
            if (current.enclosing == null) return -1;

            var local = ResolveLocal(varName);
            if (local != -1) {
                return AddUpValue(local, true);
            }

            return -1;
        }
        
        private int AddUpValue(int index, bool isLocal) {
            var upValueCount = current.function.UpValueCount;
            
            for (var i = 0; i < upValueCount; i++) {
                var upValue = current.upValues[i];
                if (upValue.Index == index && upValue.IsLocal == isLocal) {
                    return i;
                }
            }

            current.upValues[upValueCount] = new UpValue(index, isLocal);
            return current.function.UpValueCount++;
        }

        private void MarkInitialized()
        {
            if (current.scopeDepth == 0)
            {
                return;
            }

            current.locals[current.localCount - 1].Depth = current.scopeDepth;
        }

        private void BeginScope()
        {
            current.scopeDepth += 1;
        }

        private void EndScope()
        {
            current.scopeDepth -= 1;

            while (current.localCount > 0 &&
                   current.locals[current.localCount - 1].Depth > current.scopeDepth)
            {
                Emit(OpCode.Pop);
                current.localCount -= 1;
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
                    EmitConstant(Value.Obj(ObjString.CopyString(str.Value)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(); // TODO
            }
        }
        
        private byte MakeConstant(Value v)
        {
            var constant = CurrentChunk().AddConstant(v);
            if (constant > byte.MaxValue) {
                throw new Exception("Too many constants in one chunk"); // TODO
            }

            return (byte)constant;
        }

        private void EmitConstant(Value val)
        {
            var constant = CurrentChunk().AddConstant(val);
            CurrentChunk().WriteChunk((byte)OpCode.Constant);
            CurrentChunk().WriteChunk((byte)constant);
        }

        private void EmitByte(byte b)
        {
            CurrentChunk().WriteChunk(b);
        }

        private void Emit(OpCode opCode)
        {
            CurrentChunk().WriteChunk((byte)opCode);
        }

        private ObjFunction EndCompiler()
        {
            Emit(OpCode.Nil);
            Emit(OpCode.Return);
            var fun = current.function;
            
            Console.WriteLine(CurrentChunk());

            current = current.enclosing;
            return fun;
        }
    }
}