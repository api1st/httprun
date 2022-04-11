using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestVerbLineDecoder : IRequestVerbLineDecoder
    {
        public bool RequestVerbHasBody(IBlockLine last)
        {
            if (last == null)
            {
                return false;
            }

            if (last.LineType == LineType.RequestVerb)
            {
                var instance = (RequestVerbLine)last;
                return instance.VerbHasPayload;
            }

            if (last.LineType == LineType.RequestVerbMultiLine)
            {
                if (last.Previous == null || last.Previous.LineType != LineType.RequestVerbMultiLine)
                {
                    // this is the first line
                    return last.Raw.StartsWith("P");
                }
            }

            return RequestVerbHasBody(last.Previous);
        }
    }
}