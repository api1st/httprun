using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestNameParser : IRegexParser
    {
        private const string RegexString = "^# @name[ \t]+([a-zA-Z]?[a-zA-Z0-9_]*)[ \t]*$";

        public bool IsMatch(string value)
        {
            return new Regex(RegexString).IsMatch(value);
        }

        public IBlockLine Decode(string value, IBlockLine previous, int lineNumber)
        {
            return new RequestNameLine(value, previous, lineNumber);
        }
    }
}