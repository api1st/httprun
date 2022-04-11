namespace HttpExecutor.Abstractions
{
    public interface IConfirmationReader
    {
        bool Confirm(IHttpRequest request);
    }
}