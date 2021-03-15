using System;

namespace testlang.Scanner
{
    public class UnterminatedStringException : Exception
    {
    }

    public class UnexpectedChar : Exception
    {
        public UnexpectedChar(char c) : base($"Unexpected character '{c}'")
        {
        }
    }
}