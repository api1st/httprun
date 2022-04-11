using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestNameLine : IBlockLine
    {
        private const string GroupRegex = "^# @name[ \t]+([a-zA-Z]?[a-zA-Z0-9_]*)[ \t]*$";

        public RequestNameLine(string value, IBlockLine previous, int lineNumber)
        {
            Raw = value;
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.RequestName;

        public string Raw { get; }

        public IBlockLine Previous { get; set; }

        public int LineNumber { get; }

        public string Name => new Regex(GroupRegex).Match(Raw).Groups[1].Value;
    }
}