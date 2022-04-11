using System.Threading.Tasks;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Tests.Integration.Services
{
    public class TestSleeper : ISleeper
    {
        public Task SleepAsync(int milliseconds)
        {
            // Do nothing
            return Task.CompletedTask;
        }
    }
}