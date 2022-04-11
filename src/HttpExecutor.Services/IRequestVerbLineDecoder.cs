using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IRequestVerbLineDecoder
    {
        bool RequestVerbHasBody(IBlockLine last);
    }
}