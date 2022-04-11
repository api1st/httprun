using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IRequestVariableProvider
    {
        bool HasNamedRequest(string name);

        string Resolve(string value);

        void RegisterRequest(string name, IHttpRequest request);

        void RegisterResponse(string name, IHttpResponse response);
    }
}