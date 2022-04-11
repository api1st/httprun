using System.Net.Http;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IMultipartHttpRequestPreparer
    {
        HttpRequestMessage BuildMultipartFormDataRequest(IHttpRequest request, HttpClient client);
    }
}