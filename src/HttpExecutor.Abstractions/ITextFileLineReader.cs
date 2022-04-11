using System.Threading;
using System.Threading.Tasks;

namespace HttpExecutor.Abstractions
{
    public interface ITextFileLineReader
    {
        Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default(CancellationToken));
    }
}