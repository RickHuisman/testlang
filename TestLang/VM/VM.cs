using System;
using System.Collections.Generic;
using System.Linq;

namespace testlang
{
    public class VM
    {
        private SliceableArray<Value> _stack = new SliceableArray<Value>(byte.MaxValue); // TODO size
        private int _stackTop = 0;
        private Dictionary<string, Value> _globals = new Dictionary<string, Value>();

        private readonly CallFrame[] _frames = new CallFrame[64]; // TODO 64???
        private CallFrame _frame;
        private int _frameCount;

        public void Interpret(string source)
        {
            var function = new Compiler(FunctionType.Script).Compile(source);

            Push(Value.Obj(function));
            var closure = new ObjClosure(function);
            Pop();
            Push(Value.Obj(closure));
            CallValue(Value.Obj(closure), 0);

            Run();
        }

        private void Run()
        {
            _frame = _frames[_frameCount - 1];

            while (!IsAtEnd())
            {
                var b = ReadByte();

                switch ((OpCode)b)
                {
                    case OpCode.Constant:
                        var value = ReadConstant();
                        Push(value);
                        break;
                    case OpCode.Add:
                        Add();
                        break;
                    case OpCode.Subtract:
                        Subtract();
                        break;
                    case OpCode.Multiply:
                        Multiply();
                        break;
                    case OpCode.Divide:
                        Divide();
                        break;
                    case OpCode.Negate:
                        if (!Peek(0).IsNumber) throw new Exception(""); // TODO
                        var negate = Pop().AsNumber;
                        Push(Value.Number(-negate));
                        break;
                    case OpCode.Return:
                        var result = Pop();

                        _frameCount -= 1;
                        if (_frameCount == 0) {
                            Pop();
                            return;
                            // return InterpretResult.OK;
                        }

                        _stackTop = _frame.Slots.Offset;
                        Push(result);

                        _frame = _frames[_frameCount - 1];
                        break;
                    case OpCode.Print:
                        Print();
                        break;
                    case OpCode.Not:
                        Not();
                        break;
                    case OpCode.Equal:
                        Equal();
                        break;
                    case OpCode.Greater:
                        Greater();
                        break;
                    case OpCode.Less:
                        Less();
                        break;
                    case OpCode.Pop:
                        Pop();
                        break;
                    case OpCode.DefineGlobal:
                        DefineGlobal();
                        break;
                    case OpCode.GetGlobal:
                        GetGlobal();
                        break;
                    case OpCode.SetGlobal:
                        SetGlobal();
                        break;
                    case OpCode.GetLocal:
                        GetLocal();
                        break;
                    case OpCode.SetLocal:
                        SetLocal();
                        break;
                    case OpCode.JumpIfFalse:
                        JumpIfFalse();
                        break;
                    case OpCode.Jump:
                        Jump();
                        break;
                    case OpCode.Loop:
                        Loop();
                        break;
                    case OpCode.Nil:
                        Push(Value.Nil);
                        break;
                    case OpCode.Call:
                        var argCount = ReadByte();
                        if (!CallValue(Peek(argCount), argCount))
                        {
                            throw new Exception("????"); // TODO
                        }
                        _frame = _frames[_frameCount - 1];
                        break;
                    case OpCode.Closure:
                        var function = ReadConstant().AsFunction;
                        var closure = new ObjClosure(function);
                        Push(Value.Obj(closure));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private bool CallValue(Value callee, int argCount)
        {
            if (callee.IsObj)
            {
                switch (callee.ObjType)
                {
                    // case ObjType.Function:
                    //     return Call(callee.AsFunction, argCount);
                    case ObjType.Closure:
                        return Call(callee.AsClosure, argCount);
                    // case ObjType.Native: // TODO
                    //     {
                    //         Func<int, Value[], Value> native = callee.AsNative;
                    //         Value result = native(argCount, stack.Take(argCount));
                    //         stackTop -= argCount + 1;
                    //         Push(result);
                    //         return true;
                    //     }
                    default:
                        break;
                }
            }

            throw new Exception("Can only call functions and classes");
            return false;
        }

        private bool Call(ObjClosure closure, int argCount)
        {
            if (argCount != closure.Function.Arity)
            {
                throw new Exception($"Expected {closure.Function.Arity} arguments but got {argCount}"); // TODO
                return false;
            }

            // if (_frameCount == FramesMax) // TODO
            // {
            //     throw new Exception($"Stack overflow.");
            //     return false;
            // }

            _frames[_frameCount++] = new CallFrame
            {
                Closure = closure,
                // Ip = closure.Function.Chunk.Code.Count, // TODO ???
                Ip = 0, // TODO ???
                Slots = _stack.Slice(_stackTop - argCount - 1)
            };

            return true;
        }

        private void Loop()
        {
            var offset = ReadShort();
            _frame.Ip -= offset;
        }

        private void JumpIfFalse()
        {
            var offset = ReadShort();
            if (IsFalsey(Peek(0))) _frame.Ip += offset;
        }

        private void Jump()
        {
            var offset = ReadShort();
            _frame.Ip += offset;
        }

        private void GetLocal()
        {
            var slot = ReadByte();
            Push(_frame.Slots[slot]);
        }

        private void SetLocal()
        {
            var slot = ReadByte();
            _frame.Slots[slot] = Peek(0);
        }

        private void SetGlobal()
        {
            var name = ReadConstant().AsString;
            var value = Peek(0);
            if (_globals.ContainsKey(name.ToString()))
            {
                _globals[name.ToString()] = value;
            }
            else
            {
                throw new Exception($"No global with name: {name}");
            }
        }

        private void GetGlobal()
        {
            var name = ReadConstant().AsString;
            var value = _globals.GetValueOrDefault(name.ToString()); // TODO error if not found
            Push(value);
        }

        private void DefineGlobal()
        {
            var name = ReadConstant().AsString;
            _globals.Add(name.ToString(), Peek(0));
            Pop();
        }

        private void Not()
        {
            Push(Value.Bool(IsFalsey(Pop())));
        }

        private void Equal()
        {
            var bpopped = Pop();
            var apopped = Pop();
            Push(Value.Bool(ValuesEqual(apopped, bpopped)));
        }

        private void Greater()
        {
            var bpopped = Pop().AsNumber;
            var apopped = Pop().AsNumber;
            Push(Value.Bool(apopped > bpopped));
        }

        private void Less()
        {
            var bpopped = Pop().AsNumber;
            var apopped = Pop().AsNumber;
            Push(Value.Bool(apopped < bpopped));
        }

        private void Print()
        {
            Console.WriteLine($"Print: {Pop()}");
        }

        private void Add()
        {
            var bpopped = Pop().AsNumber;
            var apopped = Pop().AsNumber;
            Push(Value.Number(apopped + bpopped));
        }

        private void Subtract()
        {
            var bpopped = Pop().AsNumber;
            var apopped = Pop().AsNumber;
            Push(Value.Number(apopped - bpopped));
        }

        private void Multiply()
        {
            var bpopped = Pop().AsNumber;
            var apopped = Pop().AsNumber;
            Push(Value.Number(apopped * bpopped));
        }

        private void Divide()
        {
            var bpopped = Pop().AsNumber;
            var apopped = Pop().AsNumber;
            Push(Value.Number(bpopped / apopped));
        }

        private bool IsFalsey(Value value)
        {
            return value.IsNil || value.IsBool && !value.AsBool;
        }

        private bool ValuesEqual(Value a, Value b)
        {
            if (a.Type != b.Type) return false;

            return a.Type switch
            {
                ValueType.Bool => a.AsBool == b.AsBool,
                ValueType.Nil => true,
                ValueType.Number => a.AsNumber == b.AsNumber,
            };
        }

        private void ResetStack()
        {
            _stackTop = 0;
            _frameCount = 0;
        }

        private Value ReadConstant()
        {
            return CurrentChunk().Constants[ReadByte()];
        }

        private ushort ReadShort()
        {
            _frame.Ip += 2;

            return (ushort)((CurrentChunk().Code[_frame.Ip - 2] << 8) | CurrentChunk().Code[_frame.Ip - 1]);
        }

        private byte ReadByte()
        {
            var b = CurrentChunk().Code[_frame.Ip];
            _frame.Ip += 1;
            return b;
        }

        private bool IsAtEnd()
        {
            return _frame.Ip >= CurrentChunk().Code.Count;
        }

        private void Push(Value value)
        {
            _stack[_stackTop++] = value;
        }

        private Value Peek(int offset)
        {
            return _stack[_stackTop - 1 - offset];
        }

        private Value Pop()
        {
            return _stack[--_stackTop]; // TODO switch --
        }

        private Chunk CurrentChunk()
        {
            return _frame.Closure.Function.Chunk;
        }
    }
}
