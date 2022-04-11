using System.Collections.Generic;

namespace HttpExecutor.Abstractions
{
    public interface IHttpRequest
    {
        string Name { get; }

        string Verb { get; }

        string Path { get; }

        string Scheme { get; }

        string Host { get; }

        bool IsMultipartFormData { get; }

        ICollection<IHttpHeader> Headers { get; }

        string Body { get; }
        
        ICollection<IRequestBodyFile> Files { get; }

        bool RequiresConfirmation { get; }
    }
}