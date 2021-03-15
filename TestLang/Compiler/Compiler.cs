using System;
using testlang.Parser.ast;
using testlang.Scanner;

namespace testlang.Compiler
{
    public class Compiler
    {
        private Instance _current;

        private class Instance
        {
            internal Instance()
            {
                Locals = new Local[byte.MaxValue];
                UpValues = new UpValue[256]; // TODO int ???
                LocalCount = 0;
                ScopeDepth = 0;
                Function = new ObjFunction();

                Locals[LocalCount++] = new Local("", 0);
            }

            internal ObjFunction Function;
            internal FunctionType Type;

            internal readonly Local[] Locals;
            internal int LocalCount;
            internal UpValue[] UpValues;
            internal int ScopeDepth;

            internal Instance Enclosing;
        }

        public Compiler(FunctionType type)
        {
            _current = new Instance
            {
                Type = type,
                Enclosing = null
            };
        }

        public ObjFunction Compile(string source)
        {
            var statements = Parser.Parser.Parse(source);

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
                case ExpressionStatement expr:
                    CompileExpr(expr.Expr);
                    Emit(OpCode.Pop);
                    break;
                case PrintStatement expr:
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
                case StructStatement structStmt:
                    CompileStruct(structStmt);
                    break;
                default:
                    throw new Exception($"TODO {statement}");
            }
        }

        private void CompileStruct(StructStatement structStmt)
        {
            var nameConstant = MakeConstant(Value.Obj(ObjString.CopyString(structStmt.Name.Name)));
            DeclareVar(structStmt.Name);
            
            Emit(OpCode.Struct);
            EmitByte(nameConstant);
            DefineVar(structStmt.Name);
        }

        private void CompileReturn(ReturnStatement stmt)
        {
            if (_current.Type == FunctionType.Script) {
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
            _current = new Instance
            {
                Type = FunctionType.Function, // TODO
                Enclosing = _current,
            };
            _current.Function.SetName(fun.Variable.Name);

            BeginScope();

            foreach (var p in fun.Declaration.Parameters)
            {
                _current.Function.Arity += 1;
                DeclareVar(p);
            }

            // The body.
            CompileBlock(fun.Declaration.Body);

            // Create the function object.
            var function = EndCompiler();
            
            Emit(OpCode.Closure);
            EmitByte(MakeConstant(Value.Obj(function)));

            for (var i = 0; i < function.UpValueCount; i++) {
                EmitByte(_current.UpValues[i].IsLocal ? (byte) 1 : (byte) 0);
                EmitByte((byte) _current.UpValues[i].Index);
            }
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

            if (_current.ScopeDepth > 0)
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
            if (_current.ScopeDepth > 0) {
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
            if (_current.ScopeDepth == 0) return;

            for (var i = _current.LocalCount - 1; i >= 0; i -= 1)
            {
                var local = _current.Locals[i];
                if (local.Depth != -1 && local.Depth < _current.ScopeDepth)
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
            _current.Locals[_current.LocalCount++] = new Local(name, -1);
        }

        private void CompileExpr(Expression expr)
        {
            switch (expr.Node)
            {
                case BinaryExpression binary:
                    CompileBinaryExpr(binary);
                    break;
                case UnaryExpression unary:
                    CompileUnaryExpr(unary);
                    break;
                case ILiteral literal:
                    EmitLiteral(literal);
                    break;
                case VarSetExpression set:
                    CompileExpr(set.Expr);

                    var arg = ResolveLocal(_current, set.Var.Name);
                    if (arg != -1)
                    {
                        // Local
                        Emit(OpCode.SetLocal);
                        CurrentChunk().WriteChunk((byte)arg);
                    }
                    else if ((arg = ResolveUpValue(_current, set.Var.Name)) != -1)
                    {
                        Emit(OpCode.SetUpValue);
                        EmitByte((byte) arg);
                        // Upvalue
                        // Emit(OpCode.SetUpValue);
                        // EmitByte((byte) arg);
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
                    var arg2 = ResolveLocal(_current, get.Var.Name);
                    if (arg2 != -1)
                    {
                        // Local
                        Emit(OpCode.GetLocal);
                        CurrentChunk().WriteChunk((byte)arg2); // TODO
                    }
                    else if ((arg2 = ResolveUpValue(_current, get.Var.Name)) != -1)
                    {
                        // throw new Exception();
                        // Upvalue
                        Emit(OpCode.GetUpValue);
                        EmitByte((byte) arg2);
                        // TODO
                        // getOp = OP_GET_UPVALUE;
                        // setOp = OP_SET_UPVALUE;
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
                case SetExpression set:
                    // TODO Compile init???
                    
                    CompileExpr(set.Expr);

                    //
                    var name = MakeConstant(Value.Obj(ObjString.CopyString(set.Name)));
                    
                    CompileExpr(set.Value);
                    
                    Emit(OpCode.SetProperty);
                    EmitByte(name);
                    break;
                case GetExpression get:
                    // TODO Compile init???
                    
                    CompileExpr(get.Expr);

                    //
                    
                    var name2 = MakeConstant(Value.Obj(ObjString.CopyString(get.Name)));
                    
                    Emit(OpCode.GetProperty);
                    EmitByte(name2);
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

        private void CompileUnaryExpr(UnaryExpression expr)
        {
            CompileExpr(expr.Unary);

            switch (expr.Operator)
            {
                case UnaryOperator.Negate:
                    Emit(OpCode.Negate);
                    break;
                case UnaryOperator.Not:
                    Emit(OpCode.Not);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int ResolveLocal(Instance instance, string name)
        {
            for (var i = instance.LocalCount - 1; i >= 0; i -= 1)
            {
                var local = instance.Locals[i];
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
        
        private static int ResolveUpValue(Instance instance, string varName) {
            if (instance.Enclosing == null) return -1;

            var local = ResolveLocal(instance.Enclosing, varName);
            if (local != -1) {
                return AddUpValue(instance, local, true);
            }
            
            var upValue = ResolveUpValue(instance.Enclosing, varName);
            if (upValue != -1) {
                return AddUpValue(instance, upValue, false);
            }

            return -1;
        }
        
        private static int AddUpValue(Instance instance, int index, bool isLocal) {
            var upValueCount = instance.Function.UpValueCount;
            
            for (var i = 0; i < upValueCount; i++) {
                var upValue = instance.UpValues[i];
                if (upValue.Index == index && upValue.IsLocal == isLocal) {
                    return i;
                }
            }

            instance.UpValues[upValueCount] = new UpValue(index, isLocal);
            return instance.Function.UpValueCount++;
        }

        private void MarkInitialized()
        {
            if (_current.ScopeDepth == 0)
            {
                return;
            }

            _current.Locals[_current.LocalCount - 1].Depth = _current.ScopeDepth;
        }

        private void BeginScope()
        {
            _current.ScopeDepth += 1;
        }

        private void EndScope()
        {
            _current.ScopeDepth -= 1;

            while (_current.LocalCount > 0 &&
                   _current.Locals[_current.LocalCount - 1].Depth > _current.ScopeDepth)
            {
                Emit(OpCode.Pop);
                _current.LocalCount -= 1;
            }
        }

        private void EmitLiteral(ILiteral literal)
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
            EmitByte((byte)OpCode.Constant);
            EmitByte((byte)constant);
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
            var fun = _current.Function;
            
            Console.WriteLine(CurrentChunk());

            _current = _current.Enclosing;
            return fun;
        }

        private Chunk CurrentChunk()
        {
            return _current.Function.Chunk;
        }
    }
}