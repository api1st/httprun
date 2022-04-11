using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class ConsoleWrapper : IConsole
    {
        public string ReadLine()
        {
            return System.Console.ReadLine();
        }

        public void WriteLine(string text)
        {
            System.Console.WriteLine(text);
        }

        public void WriteLine()
        {
            System.Console.WriteLine();
        }
    }
}