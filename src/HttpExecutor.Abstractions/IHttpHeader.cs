namespace HttpExecutor.Abstractions
{
    public interface IHttpHeader
    {
        string Name { get; }

        string Value { get; }
    }
}