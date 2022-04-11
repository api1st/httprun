using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class DividerLine : IBlockLine
    {
        public DividerLine(string value, IBlockLine previous, int lineNumber)
        {
            Raw = value;
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.Divider;

        public string Raw { get; }

        public IBlockLine Previous { get; set; }

        public int LineNumber { get; }
    }
}