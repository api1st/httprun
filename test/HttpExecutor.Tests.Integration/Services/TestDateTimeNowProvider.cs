using System;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Tests.Integration.Services
{
    public class TestDateTimeNowProvider : IDateTimeNowProvider
    {
        private DateTime _now;

        public void SetNow(DateTime now)
        {
            _now = now;
        }

        public DateTime UtcNow()
        {
            return _now;
        }
    }
}