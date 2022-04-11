using System.Collections.Generic;

namespace HttpExecutor.Abstractions
{
    public class HttpFile
    {
        public HttpFile()
        {
            Blocks = new List<ExecutionBlock>();
        }

        public ICollection<ExecutionBlock> Blocks { get; }
    }
}