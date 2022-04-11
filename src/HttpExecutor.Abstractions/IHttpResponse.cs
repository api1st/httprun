using System.Collections.Generic;

namespace HttpExecutor.Abstractions
{
    public interface IHttpResponse
    {
        int StatusCode { get; }

        ICollection<IHttpHeader> Headers { get; }

        string Body { get; }
    }
}