namespace HttpExecutor.Abstractions
{
    public interface IEnvironment
    {
        void Exit(int code);

        string? GetEnvironmentVariable(string variable);
    }
}