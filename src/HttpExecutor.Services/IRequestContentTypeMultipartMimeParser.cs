namespace HttpExecutor.Services
{
    public interface IRequestContentTypeMultipartMimeParser
    {
        string GetBoundary(string line);
    }
}