using System;

namespace HttpExecutor.Abstractions
{
    public interface IDateTimeNowProvider
    {
        DateTime UtcNow();
    }
}