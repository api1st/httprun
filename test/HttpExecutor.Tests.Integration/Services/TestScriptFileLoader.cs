using System.Threading;
using System.Threading.Tasks;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Tests.Integration.Services
{
    public class TestScriptFileLoader : ITextFileLineReader
    {
        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            return System.IO.File.ReadAllLinesAsync(path, cancellationToken);
        }
    }
}