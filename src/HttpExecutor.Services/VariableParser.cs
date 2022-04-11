using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class VariableParser : IRegexParser
    {
        private const string RegexString = "^@[a-zA-Z_]?[a-zA-Z0-9_]*\\s*=\\s*.*$";

        public bool IsMatch(string value)
        {
            return new Regex(RegexString).IsMatch(value);
        }

        public IBlockLine Decode(string value, IBlockLine previous, int lineNumber)
        {
            return new VariableLine(value, previous, lineNumber);
        }
    }
}