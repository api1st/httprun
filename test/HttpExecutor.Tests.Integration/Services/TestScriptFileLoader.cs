using System;
using System.Threading;
using System.Threading.Tasks;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Tests.Integration.Services
{
    public class TestScriptFileLoader : ITextFileLineReader
    {
        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            Console.WriteLine($"Current working directory: {System.Environment.CurrentDirectory}");
            Console.WriteLine($"File to load: '{System.Environment.CurrentDirectory}/{path}'");
            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("The file does not exist.");
            }
            return System.IO.File.ReadAllLinesAsync(path, cancellationToken);
        }
    }
}