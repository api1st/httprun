namespace HttpExecutor.Abstractions
{
    public interface ILineParser
    {
        IBlockLine Parse(string line, IBlockLine lastLine, int lineNumber);
    }
}