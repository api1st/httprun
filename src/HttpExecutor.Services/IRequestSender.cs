using System.Threading.Tasks;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IRequestSender
    {
        Task<(IHttpRequest, IHttpResponse)> SendAsync(IHttpRequest request);
    }
}