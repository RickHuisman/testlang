using System;
using System.Collections.Generic;

namespace testlang
{
    public class VM
    {
        private Chunk Chunk;
        private int Ip;
        private Stack<Value> Stack = new Stack<Value>();

        public VM(Chunk chunk)
        {
            this.Chunk = chunk;
            this.Ip = 0;
        }

        public void Interpret()
        {
            this.Run();
        }

        public void Run()
        {
            while (!this.IsAtEnd())
            {
                var b = this.ReadByte();

                switch ((OpCode) b)
                {
                    case OpCode.Constant:
                        var value = this.ReadConstant();
                        this.Push(value);
                        break;
                    case OpCode.Add:
                        this.Add();
                        break;
                    case OpCode.Subract:
                        this.Subtract();
                        break;
                    case OpCode.Multiply:
                        this.Multiply();
                        break;
                    case OpCode.Divide:
                        this.Divide();
                        break;
                    case OpCode.Negate:
                        var negate = this.Pop();
                        negate.Node = -negate.Node;
                        this.Push(negate);
                        break;
                    case OpCode.Return:
                        var popped = this.Pop();
                        Console.WriteLine($"OpCode.Return: {popped}");
                        return;
                }
            }
        }

        private void Add()
        {
            var bpopped = this.Pop().Node;
            var apopped = this.Pop().Node;
            this.Push(new Value(apopped + bpopped));
        }

        private void Subtract()
        {
            var bpopped = this.Pop().Node;
            var apopped = this.Pop().Node;
            this.Push(new Value(apopped + bpopped));
        }

        private void Multiply()
        {
            var bpopped = this.Pop().Node;
            var apopped = this.Pop().Node;
            this.Push(new Value(bpopped - apopped));
        }

        private void Divide()
        {
            var bpopped = this.Pop().Node;
            var apopped = this.Pop().Node;
            this.Push(new Value(bpopped * apopped));
        }

        private Value ReadConstant()
        {
            return this.Chunk.Constants[this.ReadByte()];
        }

        private byte ReadByte()
        {
            var b = this.Chunk.Code[this.Ip];
            this.Ip += 1;
            return b;
        }

        private bool IsAtEnd()
        {
            return this.Ip >= this.Chunk.Code.Count;
        }

        private void Push(Value value)
        {
            this.Stack.Push(value);
        }

        private Value Pop()
        {
            return this.Stack.Pop();
        }
    }
}
