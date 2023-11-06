using System.Linq;
using HttpExecutor.Abstractions;
using HttpExecutor.Ioc;
using HttpExecutor.Tests.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace HttpExecutor.Tests.Integration
{
    public class Executor_Auth_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private readonly HttpFile _httpFile;
        private readonly IBlockExecutor _subject;

        public Executor_Auth_Fixture(ITestOutputHelper outputHelper)
        {
            var services = new ServiceCollection();
            services.AddSingleton(outputHelper);
            services.AddExecutorServices();
            services.AddTransient<IConsole, TestConsole>();
            services.AddTransient<ISleeper, TestSleeper>();
            services.AddTransient<IEnvironment, TestEnvironment>();
            services.AddSingleton<IAppOptions>(new AppOptions { RequestTimeout = 30000 });
            services.AddTransient<IDateTimeNowProvider, TestDateTimeNowProvider>();

            var provider = services.BuildServiceProvider();

            var reader = new TestScriptFileLoader();
            var scriptContent = reader.ReadAllLinesAsync("Scripts/5-Auth.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);
            
            _subject = provider.GetRequiredService<IBlockExecutor>();
        }

        [Fact]
        public async void Execute_Basic_NoAuth_401()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));

            Assert.Equal(401, result.Item3?.StatusCode);
        }

        [Fact]
        public async void Execute_Basic_ColonSeparated_200()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            Assert.Equal(200, result.Item3?.StatusCode);
        }

        [Fact]
        public async void Execute_Basic_Base64Encoded_200()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            Assert.Equal(200, result.Item3?.StatusCode);
        }

        [Fact]
        public async void Execute_Basic_SpaceSeparated_200()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(2));

            Assert.Equal(200, result.Item3?.StatusCode);
        }
    }
}
