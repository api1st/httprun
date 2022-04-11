using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestHeaderLine : IBlockLine
    {
        private const string GroupsRegex = "^([^:]*): (.*)$";

        public RequestHeaderLine(string value, IBlockLine previous, int lineNumber)
        {
            Raw = value;
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.RequestHeader;

        public string Raw { get; }

        public IBlockLine Previous { get; set; }

        public int LineNumber { get; set; }

        public string HeaderName => new Regex(GroupsRegex).Match(Raw).Groups[1].Value;

        public string HeaderValue => new Regex(GroupsRegex).Match(Raw).Groups[2].Value;
    }
}