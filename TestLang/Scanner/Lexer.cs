using System;
using System.Collections.Generic;

namespace testlang
{
    public static class Lexer
    {
        private static int _current;
        private static string _source;

        public static List<Token> Parse(string source)
        {
            ResetLexer();
            _source = source;
            
            var tokens = new List<Token>();

            while (!IsAtEnd())
            {
                var token = ScanToken();
                tokens.Add(token);
            }

            return tokens;
        }

        private static Token ScanToken()
        {
            SkipWhitespace();

            if (IsAtEnd())
            {
                return new Token(TokenType.EOF, "");
            }

            var c = Advance();

            if (char.IsLetter(c)) return Identifier();
            if (char.IsDigit(c)) return Number();

            return c switch
            {
                '(' => MakeToken(TokenType.LeftParen),
                ')' => MakeToken(TokenType.RightParen),
                '{' => MakeToken(TokenType.LeftBrace),
                '}' => MakeToken(TokenType.RightBrace),
                ';' => MakeToken(TokenType.Semicolon),
                ',' => MakeToken(TokenType.Comma),
                '.' => MakeToken(TokenType.Dot),
                '-' => MakeToken(TokenType.Minus),
                '+' => MakeToken(TokenType.Plus),
                '/' => MakeToken(TokenType.Slash),
                '*' => MakeToken(TokenType.Star),
                '!' => MakeToken2(Match('=') ? TokenType.BangEqual : TokenType.Bang),
                '=' => MakeToken2(Match('=') ? TokenType.EqualEqual : TokenType.Equal),
                '<' => MakeToken2(Match('=') ? TokenType.LessThanEqual : TokenType.LessThan),
                '>' => MakeToken2(Match('=') ? TokenType.GreaterThanEqual : TokenType.GreaterThan),
                '"' => String(),
                _ => throw new ArgumentException($"Unknown char: {c}")
            };
        }

        private static Token String()
        {
            var start = _current; // TODO maybe - 1
            while (Peek() != '"' && !IsAtEnd())
            {
                Advance();
            }

            if (IsAtEnd()) throw new ArgumentException("Unterminated string."); // TODO custom error

            var end = _current;
            
            // The closing quote.
            Advance();

            return new Token(TokenType.String, _source[start..end]);
        }

        private static Token Number()
        {
            var start = _current - 1;
            while (char.IsDigit(Peek())) Advance();

            // Look for a fractional part
            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (char.IsDigit(Peek())) Advance();
            }
            
            return new Token(TokenType.Number, _source[start.._current]);
        }

        private static Token Identifier()
        {
            var start = _current - 1;
            while (char.IsLetter(Peek()) || char.IsDigit(Peek())) Advance();

            var identifier = _source[start.._current];
            var identifierType = TokenTypeTranslator.FromString(identifier);
            
            return new Token(identifierType, identifier);
        }

        private static Token MakeToken(TokenType type)
        {
            var c = _source[_current - 1];
            var token = new Token(type, c.ToString());
            return token;
        }
        
        private static Token MakeToken2(TokenType type)
        {
            // TODO This function is only used for having a 2 char source
            var start = _current - 2;
            var str = _source[start.._current];
            var token = new Token(type, str);
            return token;
        }

        private static void SkipWhitespace()
        {
            for (;;)
            {
                var c = Peek();
                if (char.IsWhiteSpace(c))
                {
                    Advance();
                }
                else return;
            }
        }

        private static bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current += 1;
            return true;
        }

        private static char Advance()
        {
            _current += 1;
            return _source[_current - 1];
        }

        private static char PeekNext()
        {
            return IsAtEnd() ? '\0' : _source[_current + 1];
        }

        private static char Peek()
        {
            return IsAtEnd() ? '\0' : _source[_current];
        }

        private static bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        private static void ResetLexer()
        {
            _current = 0;
        }
    }
}