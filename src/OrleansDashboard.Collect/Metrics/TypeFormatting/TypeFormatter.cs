﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrleansDashboard.Metrics.TypeFormatting
{
    /// <summary>
    /// Naive parser which makes strings containing type information easier to read
    /// </summary>
    public class TypeFormatter
    {
        private static ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

        public static string Parse(string typeName)
        {
            return cache.GetOrAdd(typeName, x => ToString(Tokenise(x)));
        }

        private static string ToString(IEnumerable<Token> tokens)
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
   
        private static IEnumerable<Token> Tokenise(string value)
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
