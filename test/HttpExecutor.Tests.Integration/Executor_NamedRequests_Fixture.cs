using System.Linq;
using HttpExecutor.Abstractions;
using HttpExecutor.Ioc;
using HttpExecutor.Services;
using HttpExecutor.Tests.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace HttpExecutor.Tests.Integration
{
    public class Executor_NamedRequests_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private HttpFile _httpFile;
        private IBlockExecutor _subject;
        private IVariableProvider _variableProvider;
        private IAppOptions _appOptions;

        public Executor_NamedRequests_Fixture(ITestOutputHelper outputHelper)
        {
            _appOptions = new AppOptions {RequestTimeout = 30000};

            var services = new ServiceCollection();
            services.AddSingleton(outputHelper);
            services.AddExecutorServices();
            services.AddTransient<IConsole, TestConsole>();
            services.AddTransient<ISleeper, TestSleeper>();
            services.AddTransient<IEnvironment, TestEnvironment>();
            services.AddSingleton<IAppOptions>(_appOptions);
            services.AddTransient<IDateTimeNowProvider, TestDateTimeNowProvider>();

            var provider = services.BuildServiceProvider();

            var reader = new TestScriptFileLoader();
            var scriptContent = reader.ReadAllLinesAsync("Scripts/7-NamedRequests.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);

            _variableProvider = provider.GetRequiredService<IVariableProvider>();

            _subject = provider.GetRequiredService<IBlockExecutor>();
        }

        [Fact]
        public async void Execute_Request_Named_Can_resolve_response_variable()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/get", _variableProvider.Resolve("{{sevenGet1.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Request_Trailing_Space()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal($"http://httpbin.org/get", _variableProvider.Resolve("{{sevenGet2.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Request_IntermediateSpaces()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(2));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal($"http://httpbin.org/get", _variableProvider.Resolve("{{sevenGet3.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Tabs_no_spaces()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal($"http://httpbin.org/get", _variableProvider.Resolve("{{sevenGet4.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Trailing_tab()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(4));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal($"http://httpbin.org/get", _variableProvider.Resolve("{{sevenGet5.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Duplicated_name()
        {
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(4));
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(5));

            Assert.Equal("http://httpbin.org/anything", _variableProvider.Resolve("{{sevenGet5.response.body.$.url}}"));
        }
    }
}
