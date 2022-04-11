using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestVerbMultiline : IBlockLine
    {
        public RequestVerbMultiline(string value, IBlockLine previous, int lineNumber)
        {
            Raw = value.Trim();
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.RequestVerbMultiLine;

        public string Raw { get; }

        public IBlockLine Previous { get; set; }

        public int LineNumber { get; }
    }
}