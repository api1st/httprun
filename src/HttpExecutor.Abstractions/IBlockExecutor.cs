using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpExecutor.Abstractions
{
    public interface IBlockExecutor
    {
        Task<(IEnumerable<Warning>, IHttpRequest, IHttpResponse)> ExecuteAsync(ExecutionBlock block);
    }
}