using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using testlang.ast;

namespace testlang
{
    public class Program
    {
        private static void RunFile(string path)
        {
            var input = @"
for (var i = 0; i < 10; i = i + 1) {
    print i;
}";
            var test = JsonConvert.SerializeObject(Parser2.Parse(input));

            Console.WriteLine(test);
            // string source = File.ReadAllText(path, Encoding.UTF8);

            var compiler = new Compiler(FunctionType.Script);
            var fun = compiler.Compile(input);
            Console.WriteLine(fun.Chunk.ToString());
            // var function = compiler.Compile(source);
            // var chunk = compiler._chunk;

            // Console.WriteLine(chunk);

            // var vm = new VM();
            // vm.Interpret(input);
            // InterpretResult result = vm.Interpret(source);
        }

        static void Main(string[] args)
        {
            RunFile("/Users/rickhuisman/Documents/C#/testlang/TestLang/test.txt");
            //var source = "var test = 10;";

            // var compiler = new Compiler();
            // compiler.Compile(source);
            // var chunk = compiler._chunk;
            //
            // var vm = new VM(chunk);
            // vm.Interpret();

            // if (args.Length == 1) {
            //     RunFile(args[0]);
            // } else {
            //     Console.Error.WriteLine("Usage: Lox [path]");
            // }
        }

        // static void Main(string[] args)
        // {
        //     // var chunk = new Chunk("test");
        //     //
        //     // var constant = chunk.AddConstant(new Value(1.2));
        //     // chunk.WriteChunk((byte) OpCode.Constant);
        //     // chunk.WriteChunk((byte) constant);
        //     //
        //     //
        //     // var constant2 = chunk.AddConstant(new Value(3.4));
        //     // chunk.WriteChunk((byte) OpCode.Constant);
        //     // chunk.WriteChunk((byte) constant2);
        //     //
        //     // chunk.WriteChunk((byte) OpCode.Divide);
        //     // chunk.WriteChunk((byte) OpCode.Return);
        //     //
        //     // // Console.WriteLine($"{chunk}");
        //     //
        //     // var vm = new VM(chunk);
        //     // vm.Interpret();
        //     //
        //     // Console.WriteLine("");
        //     // Console.WriteLine("");
        //     // Console.WriteLine("");
        //
        //     //var input = "print 1 + 2;";
        //     // var input = "and 1 + 2;";
        //     // // var input = "\"test\"";
        //     // var lexer = new Lexer(input);
        //     // var tokens = lexer.Parse();
        //     // foreach (var token in tokens)
        //     // {
        //     //     Console.WriteLine($"{token}");
        //     // }
        //
        //     // var input = "-10.5 - 5 * 2;";
        //     // var parser = new Parser(input);
        //     // var stmts = parser.Parse();
        //     // foreach (var stmt in stmts)
        //     // {
        //     //     Console.WriteLine($"Statement: {stmt}");
        //     // }
        //
        //     var input = "print \"test\";";
        //     //var input = "print true == ;";
        //     var compiler = new Compiler();
        //     compiler.Compile(input);
        //     
        //     var vm2 = new VM(compiler._chunk);
        //     vm2.Interpret();
        // }
    }
}
