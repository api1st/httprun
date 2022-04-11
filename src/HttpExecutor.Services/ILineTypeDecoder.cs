using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface ILineTypeDecoder
    {
        LineType LastSignificantLineType(IBlockLine last);
    }
}