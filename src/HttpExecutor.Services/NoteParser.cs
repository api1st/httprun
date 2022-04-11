using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class NoteParser : IRegexParser
    {
        private const string RegexString = "^# @note$";

        public bool IsMatch(string value)
        {
            return new Regex(RegexString).IsMatch(value);
        }

        public IBlockLine Decode(string value, IBlockLine previous, int lineNumber)
        {
            return new ConfirmationLine(value, previous, lineNumber);
        }
    }
}