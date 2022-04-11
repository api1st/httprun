using System.Collections.Generic;

namespace HttpExecutor.Abstractions
{
    public class ExecutionBlock
    {
        public ExecutionBlock()
        {
            Lines = new List<IBlockLine>();
        }

        public ICollection<IBlockLine> Lines { get; }
    }
}
