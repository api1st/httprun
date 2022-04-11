using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IRequestBuilder
    {
        void AddLine(IBlockLine line);

        void SetName(string name);

        void RequireConfirmation();

        IHttpRequest Build();
    }
}