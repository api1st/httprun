using System.Threading;
using System.Threading.Tasks;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class TextFileLineReader : ITextFileLineReader
    {
        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            return System.IO.File.ReadAllLinesAsync(path, cancellationToken);
        }
    }
}