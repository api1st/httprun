using System.Threading.Tasks;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class ThreadWrapper : ISleeper
    {
        public async Task SleepAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }
    }
}