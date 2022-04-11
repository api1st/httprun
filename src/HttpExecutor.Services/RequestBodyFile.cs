using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestBodyFile : IRequestBodyFile
    {
        public string Encoding { get; set; }

        public bool ResolveVariables { get; set; }

        public byte[] Data { get; set; }
    }
}