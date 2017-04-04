using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    /// <summary>
    /// Naive parser which makes strings containing type information easier to read
    /// </summary>
    public class TypeFormatter
    {
        enum ParseState
        {
            TypeNameSection,
            GenericCount,
            GenericArray,
            TypeArray
        }

        enum TokenType
        {
            TypeNameSection,
            GenericCount,
            GenericArrayStart,
            GenericArrayEnd,
            TypeArrayStart,
            TypeArrayEnd,
            GenericSeparator,
            TypeSectionSeparator
        }

        struct Token
        {
            public Token(TokenType type, string value)
            {
                this.Type = type;
                this.Value = value;
            }
            public TokenType Type { get; set; }
            public string Value { get; set; }
            public override string ToString()
            {
                return string.Format("{0} = {1}", this.Type, this.Value);
            }
        }

        public static string Parse(string typeName)
        {
            return ToString(Tokenise(typeName));
        }

        static string ToString(IEnumerable<Token> tokens)
        {
            var builder = new StringBuilder();
            var firstTypeNameSection = true;
            var firstType = true;
            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.TypeNameSection:
                        if (firstType)
                        {
                            builder.Append(token.Value);
                            firstTypeNameSection = false;
                            firstType = false;
                            break;
                        }
                        if (firstTypeNameSection)builder.Append(token.Value.Split('.').Last());
                        firstTypeNameSection = false;
                        break;
                    case TokenType.GenericArrayStart:
                        builder.Append('<');
                        break;
                    case TokenType.GenericArrayEnd:
                        builder.Append('>');
                        break;
                    case TokenType.TypeArrayStart:
                        firstTypeNameSection = true;
                        break;
                    case TokenType.GenericSeparator:
                        builder.Append(", ");
                        break;

                }
            }
            return builder.ToString();
        }
   

        static IEnumerable<Token> Tokenise(string value)
        {
            var buffer = new StringBuilder();
            var state = ParseState.TypeNameSection;
            foreach (var chr in value)
            {
                switch (chr)
                {
                    case '`':
                        yield return new Token(TokenType.TypeNameSection, buffer.ToString());
                        buffer.Clear();
                        state = ParseState.GenericCount;
                        break;
                    case '[':
                        if (state == ParseState.GenericCount)
                        {
                            yield return new Token(TokenType.GenericCount, buffer.ToString());
                            yield return new Token(TokenType.GenericArrayStart, "[");
                            buffer.Clear();
                            state = ParseState.GenericArray;
                            break;
                        }
                        if (state == ParseState.GenericArray)
                        {
                            yield return new Token(TokenType.TypeArrayStart, "[");
                            buffer.Clear();
                            state = ParseState.TypeArray;
                            break;
                        }
                        Console.WriteLine("unknown [");
                        break;
                    case ']':
                        if (state == ParseState.TypeArray)
                        {
                            if (buffer.Length> 0) yield return new Token(TokenType.TypeNameSection, buffer.ToString());
                            yield return new Token(TokenType.TypeArrayEnd, "]");
                            state = ParseState.GenericArray;
                            buffer.Clear();
                            break;
                        }
                        if (state == ParseState.GenericArray)
                        {
                            yield return new Token(TokenType.GenericArrayEnd, "]");
                            state = ParseState.TypeArray;
                            buffer.Clear();
                            break;
                        }
                        Console.WriteLine("unknown ]");
                        break;
                    case ' ':
                        // no op
                        break;
                    case ',':
                        if (state == ParseState.GenericArray)
                        {
                            yield return new Token(TokenType.GenericSeparator, ",");
                            buffer.Clear();
                            break;
                        }
                        if (state == ParseState.TypeArray)
                        {
                            yield return new Token(TokenType.TypeNameSection, buffer.ToString());
                            yield return new Token(TokenType.TypeSectionSeparator, ",");
                            buffer.Clear();
                            break;
                        }
                        Console.WriteLine("unknown comma: " + value);
                        buffer.Clear();
                        break;
                    default:
                        buffer.Append(chr);
                        break;
                }
            }

            if (state == ParseState.TypeNameSection && buffer.Length > 0)
            {
                yield return new Token(TokenType.TypeNameSection, buffer.ToString());
            }

        }

    }


}
