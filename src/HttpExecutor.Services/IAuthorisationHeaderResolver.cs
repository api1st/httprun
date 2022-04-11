namespace HttpExecutor.Services
{
    public interface IAuthorisationHeaderResolver
    {
        string ProcessAuth(string headerValue, string verb, string uri);
    }
}