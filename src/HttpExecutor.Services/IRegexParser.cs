using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public interface IRegexParser
    {
        bool IsMatch(string value);

        IBlockLine Decode(string value, IBlockLine previous, int lineNumber);
    }
}