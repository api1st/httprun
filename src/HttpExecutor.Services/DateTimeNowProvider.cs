using System;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class DateTimeNowProvider : IDateTimeNowProvider
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}