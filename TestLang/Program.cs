using System;

namespace testlang
{
    public class Program
    {
        static void Main(string[] args)
        {
            // var chunk = new Chunk("test");
            //
            // var constant = chunk.AddConstant(new Value(1.2));
            // chunk.WriteChunk((byte) OpCode.Constant);
            // chunk.WriteChunk((byte) constant);
            //
            //
            // var constant2 = chunk.AddConstant(new Value(3.4));
            // chunk.WriteChunk((byte) OpCode.Constant);
            // chunk.WriteChunk((byte) constant2);
            //
            // chunk.WriteChunk((byte) OpCode.Divide);
            // chunk.WriteChunk((byte) OpCode.Return);
            //
            // // Console.WriteLine($"{chunk}");
            //
            // var vm = new VM(chunk);
            // vm.Interpret();
            //
            // Console.WriteLine("");
            // Console.WriteLine("");
            // Console.WriteLine("");

            //var input = "print 1 + 2;";
            // var input = "and 1 + 2;";
            // // var input = "\"test\"";
            // var lexer = new Lexer(input);
            // var tokens = lexer.Parse();
            // foreach (var token in tokens)
            // {
            //     Console.WriteLine($"{token}");
            // }

            // var input = "-10.5 - 5 * 2;";
            // var parser = new Parser(input);
            // var stmts = parser.Parse();
            // foreach (var stmt in stmts)
            // {
            //     Console.WriteLine($"Statement: {stmt}");
            // }

            var input = "print \"test\";";
            //var input = "print true == ;";
            var compiler = new Compiler();
            compiler.Compile(input);
            
            var vm2 = new VM(compiler._chunk);
            vm2.Interpret();
        }
    }
}
