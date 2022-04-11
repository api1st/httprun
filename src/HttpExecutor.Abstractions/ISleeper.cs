using System.Threading.Tasks;

namespace HttpExecutor.Abstractions
{
    public interface ISleeper
    {
        Task SleepAsync(int milliseconds);
    }
}