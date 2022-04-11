namespace HttpExecutor.Abstractions
{
    public interface IConsole
    {
        string ReadLine();

        void WriteLine(string text);

        void WriteLine();
    }
}