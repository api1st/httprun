using System;
using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestVerbLine : IBlockLine
    {
        private const string GroupsRegex =
            "^(?<verb>GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|TRACE)[ \\t]+((?<scheme>http:\\/\\/|https:\\/\\/)(?<userpass>.*@)?(?<host>[^:\\/]*(:\\d{1,5}?)?)?)?\\/?(?<path>[a-zA-Z0-9\\.\\/\\-\\?\\=\\&_{}\\$%@:]*)(?:[ \t]+HTTP\\/(?:1\\.0|1\\.1|2|3)[ \t]*)?$";

        private readonly Match _regexMatch;

        public RequestVerbLine(string value, IBlockLine? previous, int lineNumber)
        {
            Raw = value;
            Previous = previous;
            LineNumber = lineNumber;
            _regexMatch = new Regex(GroupsRegex).Match(Raw);

            if (!_regexMatch.Success)
            {
                throw new Exception($"Could not properly parse 'RequestVerbLine' from line {lineNumber} using expression: {GroupsRegex}");
            }
		}

        public LineType LineType => LineType.RequestVerb;

        public string Raw { get; }

        public IBlockLine? Previous { get; set; }

        public int LineNumber { get; }

        public bool VerbHasPayload => Raw.StartsWith("P"); // PUT, PATCH, POST all have bodies

        public string Verb => _regexMatch.Groups["verb"].Value;

        public string Host => _regexMatch.Groups["host"].Value;

        public string Scheme => _regexMatch.Groups["scheme"].Value;

        public string Path => _regexMatch.Groups["path"].Value;

        public string UserPass => _regexMatch.Groups["userpass"].Value;
    }
}