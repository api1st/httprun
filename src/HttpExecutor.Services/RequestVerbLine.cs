using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestVerbLine : IBlockLine
    {
        // private const string GroupsRegex = "^(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|TRACE)[ \\t]+(?:http(?:s?):\\/\\/(.*:?\\d{,5}))?\\/([a-zA-Z0-9\\.\\/\\-\\?\\=\\&_{}\\$%]*)[ \\t]+HTTP\\/1\\.1[ \\t]*$";
        private const string GroupsRegex =
            "^(?<verb>GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS|TRACE)[ \\t]+((?<scheme>http:\\/\\/|https:\\/\\/)(?<userpass>.*@)?(?<host>[^:\\/]*(:\\d{1,5}?)?)?)?\\/?(?<path>[a-zA-Z0-9\\.\\/\\-\\?\\=\\&_{}\\$%@:]*)[ \t]+HTTP\\/1\\.1[ \t]*$";

        public RequestVerbLine(string value, IBlockLine? previous, int lineNumber)
        {
            Raw = value;
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.RequestVerb;

        public string Raw { get; }

        public IBlockLine? Previous { get; set; }

        public int LineNumber { get; }

        public bool VerbHasPayload => Raw.StartsWith("P"); // PUT, PATCH, POST all have bodies

        public string Verb => new Regex(GroupsRegex).Match(Raw).Groups["verb"].Value;

        public string Host => new Regex(GroupsRegex).Match(Raw).Groups["host"].Value;

        public string Scheme => new Regex(GroupsRegex).Match(Raw).Groups["scheme"].Value;

        public string Path => new Regex(GroupsRegex).Match(Raw).Groups["path"].Value;

        public string UserPass => new Regex(GroupsRegex).Match(Raw).Groups["userpass"].Value;
    }
}