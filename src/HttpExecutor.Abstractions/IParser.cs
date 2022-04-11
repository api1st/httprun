namespace HttpExecutor.Abstractions
{
    public interface IParser
    {
        HttpFile Parse(string[] lines);
    }
}