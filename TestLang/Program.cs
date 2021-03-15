using System.IO;
using System.Text;
using testlang.Scanner;

namespace testlang
{
    public class Program
    {
        private static void RunFile(string path)
        {
             var source = File.ReadAllText(path, Encoding.UTF8);
             
             // var compiler = new Compiler.Compiler(FunctionType.Script);
             // compiler.Compile(source);
             var vm = new VM();
             vm.Interpret(source);
        }

        private static void Main(string[] args)
        {
            RunFile("/Users/rickhuisman/Documents/C#/testlang/TestLang/test.txt");
        }
    }
}
