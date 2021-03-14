using System;
using System.Text;
using System.Collections.Generic;

namespace testlang
{
    public class Chunk
    {
        private string Name;
        public List<byte> Code { get; set; }
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

            for (var offset = 0; offset < Code.Count;)
            {
                offset = DisassembleInstruction(builder, offset);
            }

            return builder.ToString();
        }

        private int DisassembleInstruction(StringBuilder builder, int offset)
        {
            builder.AppendFormat("{0:X4} ", offset);

            var instruction = (OpCode) Code[offset];
            switch (instruction)
            {
                case OpCode.Constant:
                    return ConstantInstruction(builder, "OP_CONSTANT", offset);
                case OpCode.Less:
                    return SimpleInstruction(builder, "OP_LESS", offset);
                case OpCode.Add:
                    return SimpleInstruction(builder, "OP_ADD", offset);
                case OpCode.Equal:
                    return SimpleInstruction(builder, "OP_EQUALS", offset);
                case OpCode.Greater:
                    return SimpleInstruction(builder, "OP_GREATER", offset);
                case OpCode.DefineGlobal:
                    return ConstantInstruction(builder, "OP_DEFINE_GLOBAL", offset);
                case OpCode.GetLocal:
                    return ByteInstruction(builder, "OP_GET_LOCAL", offset);
                case OpCode.SetLocal:
                    return ByteInstruction(builder, "OP_SET_LOCAL", offset);
                case OpCode.GetGlobal:
                    return ConstantInstruction(builder, "OP_GET_GLOBAL", offset);
                case OpCode.Print:
                    return SimpleInstruction(builder, "OP_PRINT", offset);
                case OpCode.Pop:
                    return SimpleInstruction(builder, "OP_POP", offset);
                case OpCode.Jump:
                    return JumpInstruction(builder, "OP_JUMP", 1, offset);
                case OpCode.JumpIfFalse:
                    return JumpInstruction(builder, "OP_JUMP_IF_FALSE", 1, offset);
                case OpCode.Loop:
                    return JumpInstruction(builder, "OP_LOOP", -1, offset);
                case OpCode.Call:
                    return ByteInstruction(builder, "OP_CALL", offset);
                case OpCode.Nil:
                    return SimpleInstruction(builder, "OP_NIL", offset);
                case OpCode.Return:
                    return SimpleInstruction(builder, "OP_RETURN", offset);
                case OpCode.Closure:
                    offset++;
                    var constant = Code[offset++];
                    builder.AppendFormat($"{"OP_CLOSURE", -16} {constant, 4:X} {Constants[constant]}\n");

                    return offset;
                default:
                    builder.AppendLine($"Unknown opcode {instruction}");
                    return offset + 1;
            }
        }
        
        private int ConstantInstruction(StringBuilder builder, string name, int offset)
        {
            var constant = Code[offset + 1];
            builder.AppendFormat($"{name,-16} {constant,4:X} '{Constants[constant]}'\n");
            return offset + 2;
        }
        
        private int ByteInstruction(StringBuilder builder, string name, int offset)
        {
            var slot = Code[offset + 1];
            builder.AppendLine($"{name,-16} {slot,4:X}");
            return offset + 2;
        }
        
        private int JumpInstruction(StringBuilder builder, string name, int sign, int offset)
        {
            ushort jump = (ushort)((Code[offset + 1] << 8) | Code[offset + 2]);
            builder.AppendLine($"{name,-16} {offset,4:X} -> {offset+ 3 + sign * jump,4:X}");
            return offset + 3;
        }
        
        private static int SimpleInstruction(StringBuilder builder, string name, int offset)
        {
            builder.AppendLine(name);
            return offset + 1;
        }
    }
}
