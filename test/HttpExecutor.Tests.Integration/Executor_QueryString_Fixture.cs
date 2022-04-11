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
    public class Executor_QueryString_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private HttpFile _httpFile;
        private IBlockExecutor _subject;
        private IVariableProvider _variableProvider;

        public Executor_QueryString_Fixture(ITestOutputHelper outputHelper)
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
            var scriptContent = reader.ReadAllLinesAsync("Scripts/4-QueryStrings.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);

            _variableProvider = provider.GetRequiredService<IVariableProvider>();

            _subject = provider.GetRequiredService<IBlockExecutor>();
        }

        [Fact]
        public async void Execute_SplitQueryString_Resolves()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/get?one=yes", _variableProvider.Resolve("{{fourQuery1.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_SplitQueryString_Resolves_2()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/get?one=yes&two=no", _variableProvider.Resolve("{{fourQuery2.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_SplitQueryString_Resolves_3()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(2));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/get?one=yes&two=no", _variableProvider.Resolve("{{fourQuery3.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_SplitQueryString_Resolves_4()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/get?one=yes&two=no&three=16", _variableProvider.Resolve("{{fourQuery4.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_SplitQueryString_Resolves_5()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(4));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/get?one=yes&two=no&three=16", _variableProvider.Resolve("{{fourQuery5.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_SplitQueryString_Resolves_6()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(5));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/get?one=yes&two=no&three=16", _variableProvider.Resolve("{{fourQuery6.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_SplitQueryString_PostBody_Resolves_7()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(6));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("Hello, this is the body.", _variableProvider.Resolve("{{fourQuery7.response.body.$.data}}"));
        }

        [Fact]
        public async void Execute_SplitQueryString_Variables_Replacement_8()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(7));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/get?one=Yes&two=no&three=16", _variableProvider.Resolve("{{fourQuery8.response.body.$.url}}"));
        }
    }
}
