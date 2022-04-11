using System.Net.Http;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IHttpRequestPreparer
    {
        HttpRequestMessage BuildRequest(IHttpRequest request, HttpClient client);
    }
}