namespace HttpExecutor.Services
{
    public interface IDynamicVariableResolver
    {
        bool Resolvable(string content);

        string Resolve(string content);
    }
}