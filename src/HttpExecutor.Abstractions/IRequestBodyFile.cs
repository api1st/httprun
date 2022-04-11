namespace HttpExecutor.Abstractions
{
    public interface IRequestBodyFile
    {
        string Encoding { get; }

        bool ResolveVariables { get; }

        byte[] Data { get; }
    }
}