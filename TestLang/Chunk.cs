using System.Text;
using System.Collections.Generic;

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
            this.Name = name;
            this.Code = new List<byte>(); 
            this.Constants = new List<Value>();
        }

        public void WriteChunk(byte b)
        {
            this.Code.Add(b);
        }

        public int AddConstant(Value val)
        {
            this.Constants.Add(val);
            return this.Constants.Count - 1;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"== {this.Name} chunk ==");

            foreach (var code in this.Code)
            {
                builder.AppendLine(code.ToString());
            }

            return builder.ToString();
        }
    }
}
