using System;
using System.Collections.Generic;
using testlang.ast;

namespace testlang
{
    public class VM
    {
        private Chunk _chunk;
        private int _ip;
        private Stack<Value> _stack = new Stack<Value>();

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
                    case OpCode.Subract:
                        Subtract();
                        break;
                    case OpCode.Multiply:
                        Multiply();
                        break;
                    case OpCode.Divide:
                        Divide();
                        break;
                    case OpCode.Negate:
                        if (!Peek().IsNumber) throw new Exception(""); // TODO
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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
            Console.WriteLine(Pop());
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
            _stack.Push(value);
        }

        private Value Peek()
        {
            return _stack.Peek();
        }
        
        
        private Value Pop()
        {
            return _stack.Pop();
        }
    }
}
