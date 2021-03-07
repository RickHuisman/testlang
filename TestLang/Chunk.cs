using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace testlang
{
    public class Chunk
    {
        private string Name;
        public List<byte> Code { get; }
        private List<int> Lines = new List<int>();
        public List<Value> Constants { get; }

        public Chunk(string name)
        {
            Name = name;
            Code = new List<byte>(); 
            Constants = new List<Value>();
        }

        public void WriteChunk(byte b)
        {
            Code.Add(b);
        }

        public int AddConstant(Value val)
        {
            Constants.Add(val);
            return Constants.Count - 1;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"== {Name} chunk ==");

            var foobar = new List<byte>(Code);
            foobar.Reverse();
            var test = new Stack<byte>(foobar);

            while (test.Count != 0)
            {
                var code = (OpCode) test.Pop();
                switch (code)
                {
                    case OpCode.Constant:
                        builder.AppendLine(code.ToString()); 
                        var constant = test.Pop(); 
                        builder.AppendLine($"Constant index at {constant}");
                        break;
                    case OpCode.DefineGlobal: 
                        builder.AppendLine(code.ToString());
                        var constant2 = test.Pop(); 
                        builder.AppendLine($"Constant index at {constant2}");
                        break;
                    case OpCode.SetGlobal:
                        builder.AppendLine(code.ToString());
                        var constant4 = test.Pop(); 
                        builder.AppendLine($"Constant index at {constant4}");
                        break;
                    case OpCode.GetGlobal:
                        builder.AppendLine(code.ToString());
                        var constant3 = test.Pop(); 
                        builder.AppendLine($"Constant index at {constant3}");
                        break;
                    default:
                       builder.AppendLine(code.ToString());
                       break;
                }
            }

            // foreach (var code in Code)
            // {
            //     var opcode = (OpCode) code;
            //     builder.AppendLine(opcode.ToString());
            // }

            return builder.ToString();
        }
    }
}
