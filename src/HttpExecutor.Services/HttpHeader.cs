using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class HttpHeader : IHttpHeader
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}