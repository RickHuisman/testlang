using System.Collections.Generic;
using NUnit.Framework;
using testlang;

namespace UnitTest
{
    public class CompilerTest
    {
        [Test]
        public void Compile_Block_ReturnsChunk()
        {
            var expected = new Chunk("test")
            {
                Code = new List<byte>
                {
                    // new Token(TokenType.Number, "10"),
                    // new Token(TokenType.Plus, "+"),
                    // new Token(TokenType.Number, "5"),
                    // new Token(TokenType.Semicolon, ";"),
                }
            };

            var input = @"{
var x = 10;
}";
            var compiler = new Compiler();
            compiler.Compile(input);
            
            TestHelper.AreEqualByJson(expected, compiler._chunk);
        }
    }
}