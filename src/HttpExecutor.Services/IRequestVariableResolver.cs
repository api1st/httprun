using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IRequestVariableResolver
    {
        IHttpRequest ResolveVariables(IHttpRequest request);
    }
}