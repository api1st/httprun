using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestBodyLine : IBlockLine
    {
        public RequestBodyLine(string value, IBlockLine previous, int lineNumber)
        {
            Raw = value;
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.RequestBody;

        public string Raw { get; }

        public IBlockLine Previous { get; set; }

        public int LineNumber { get; }
    }
}