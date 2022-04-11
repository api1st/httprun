using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestBodyParser : IRegexParser
    {
        private const string RegexString = "^.*$";

        public bool IsMatch(string value)
        {
            return new Regex(RegexString).IsMatch(value);
        }

        public IBlockLine Decode(string value, IBlockLine previous, int lineNumber)
        {
            return new RequestBodyLine(value, previous, lineNumber);
        }
    }
}