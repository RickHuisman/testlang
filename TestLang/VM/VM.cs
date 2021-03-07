using System;
using System.Collections.Generic;
using System.Linq;
using testlang.ast;

namespace testlang
{
    public class VM
    {
        private Chunk _chunk;
        private int _ip;
        private Value[] _stack = new Value[byte.MaxValue]; // TODO size
        private int _stackTop = 0;
        private Dictionary<string, Value> _globals = new Dictionary<string, Value>();

        public VM(Chunk chunk)
        {
            _chunk = chunk;
            _ip = 0;
        }

        public void Interpret()
        {
            Run();
        }

        private void Run()
        {
            while (!IsAtEnd())
            {
                var b = ReadByte();

                switch ((OpCode) b)
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
                        var popped = Pop();
                        Console.WriteLine($"OpCode.Return: {popped}"); // TODO
                        return;
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Loop()
        {
            var offset = ReadShort();
            _ip -= offset;
        }

        private void JumpIfFalse()
        { 
            var offset = ReadShort();
            if (IsFalsey(Peek(0))) {
                _ip += offset;
            }
        }

        private void Jump()
        {
            var offset = ReadShort();
            _ip += offset;
        }
        
        private void GetLocal()
        {
            var slot = ReadByte();
            Push(_stack.ElementAt(slot));
        }

        private void SetLocal()
        {
            var slot = ReadByte();
            _stack[slot] = Peek(0);
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
            Console.WriteLine($"Popped: {Pop()}");
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
        
        private bool IsFalsey(Value value) {
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
        
        private Value ReadConstant()
        {
            return _chunk.Constants[ReadByte()];
        }

        private ushort ReadShort()
        {
            _ip += 2;
            
            return (ushort)((_chunk.Code[_ip - 2] << 8) | _chunk.Code[_ip - 1]);
        }

        private byte ReadByte()
        {
            var b = _chunk.Code[_ip];
            _ip += 1;
            return b;
        }

        private bool IsAtEnd()
        {
            return _ip >= _chunk.Code.Count;
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
    }
}
