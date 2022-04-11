using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class DividerParser : IRegexParser
    {
        private const string RegexString = "^#{3}\\s*$";

        public bool IsMatch(string value)
        {
            return new Regex(RegexString).IsMatch(value);
        }

        public IBlockLine Decode(string value, IBlockLine previous, int lineNumber)
        {
            return new DividerLine(value, previous, lineNumber);
        }
    }
}