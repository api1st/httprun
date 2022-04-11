using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IRequestBodyFileParser
    {
        (bool isFile, bool exists) IsFileInput(string line);

        IRequestBodyFile FileContent(string line);
    }
}