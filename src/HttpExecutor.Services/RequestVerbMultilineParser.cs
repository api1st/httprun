using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestVerbMultilineParser : IRegexParser
    {
        private const string RegexStringVerb = "^(?<Verb>GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|TRACE)[ \\t]+((?<scheme>http:\\/\\/|https:\\/\\/)(?<host>[^:\\/]*(:\\d{1,5}?)?)?)?\\/?(?<Path>[a-zA-Z0-9\\.\\/\\-\\?\\&\\=_{}\\$%\\@\\:]*)[ \\t]*$";
        private const string RegexStringContinuation = "^(?:\\t| )*([\\?|\\&][a-zA-Z0-9_-]+=?.*?)[ \\t]*$";
        private const string RegexStringCompletion = "^(?:\\t| )*([\\?|\\&][a-zA-Z0-9_-]+=?.*?[ \\t]+HTTP\\/1\\.1)[ \\t]*$";

        public bool IsMatch(string value)
        {
            return new Regex(RegexStringVerb).IsMatch(value) ||
                   new Regex(RegexStringContinuation).IsMatch(value) ||
                   new Regex(RegexStringCompletion).IsMatch(value);
        }

        public IBlockLine Decode(string value, IBlockLine previous, int lineNumber)
        {
            var verbMatch = new Regex(RegexStringVerb).Match(value);

            if (verbMatch.Success)
            {
                return new RequestVerbMultiline(value, previous, lineNumber);
            }

            var completionMatch = new Regex(RegexStringCompletion).Match(value);

            if (completionMatch.Success)
            {
                return new RequestVerbMultiline(completionMatch.Groups[1].Value, previous, lineNumber);
            }

            var continuationMatch = new Regex(RegexStringContinuation).Match(value);

            return new RequestVerbMultiline(continuationMatch.Groups[1].Value, previous, lineNumber);
        }
    }
}