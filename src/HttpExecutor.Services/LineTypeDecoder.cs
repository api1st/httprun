using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class LineTypeDecoder : ILineTypeDecoder
    {
        public LineType LastSignificantLineType(IBlockLine last)
        {
            if (last == null)
            {
                return LineType.Divider;
            }

            switch (last.LineType)
            {
                case LineType.RequestBody:
                case LineType.RequestHeader:
                case LineType.RequestVerb:
                case LineType.RequestVerbMultiLine:
                    return last.LineType;

                case LineType.Comment:
                case LineType.VariableDefinition:
                case LineType.RequestName:
                    return LastSignificantLineType(last.Previous);
            }

            return LineType.Divider;
        }
    }
}