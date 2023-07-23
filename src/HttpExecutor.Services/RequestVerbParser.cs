using System;
using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestVerbParser : IRegexParser
    {
        private const string RegexString = "^(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|TRACE)[ \\t]+(http(s?):\\/\\/.*)?\\/?([a-zA-Z0-9\\.\\/\\-\\?\\=\\&_{}\\$%\\@\\:]*)(?:[ \t]+HTTP\\/(?:1\\.0|1\\.1|2|3)[ \t]*)?$";

        public bool IsMatch(string value)
        {
            return new Regex(RegexString).IsMatch(value);
        }

        public IBlockLine Decode(string value, IBlockLine previous, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Verb component cannot be null or empty.");
            }

            return new RequestVerbLine(value, previous, lineNumber);
        }
    }
}