using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class ConfirmationLine : IBlockLine
    {
        public ConfirmationLine(string value, IBlockLine previous, int lineNumber)
        {
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.UserConfirmation;
        
        public string Raw => "";

        public IBlockLine Previous { get; set; }

        public int LineNumber { get; }
    }
}