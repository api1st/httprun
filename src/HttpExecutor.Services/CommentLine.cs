using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class CommentLine : IBlockLine
    {
        public CommentLine(string value, IBlockLine previous, int lineNumber)
        {
            Raw = value;
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.Comment;

        public string Raw { get; }

        public IBlockLine Previous { get; set; }

        public int LineNumber { get; }
    }
}