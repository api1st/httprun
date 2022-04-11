using System.Linq;
using HttpExecutor.Abstractions;
using HttpExecutor.Ioc;
using HttpExecutor.Services;
using HttpExecutor.Tests.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HttpExecutor.Tests.Integration
{
    public class Executor_VariableResolution_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private HttpFile _httpFile;
        private IBlockExecutor _subject;
        private IVariableProvider _variableProvider;
        private Mock<IAppOptions> _appOptions;

        public Executor_VariableResolution_Fixture(ITestOutputHelper outputHelper)
        {
            _appOptions = new Mock<IAppOptions>();
            _appOptions.Setup(x => x.RequestTimeout).Returns(30000);

            var services = new ServiceCollection();
            services.AddSingleton(outputHelper);
            services.AddExecutorServices();
            services.AddTransient<IConsole, TestConsole>();
            services.AddTransient<ISleeper, TestSleeper>();
            services.AddTransient<IEnvironment, TestEnvironment>();
            services.AddSingleton<IAppOptions>(_appOptions.Object);
            services.AddTransient<IDateTimeNowProvider, TestDateTimeNowProvider>();

            var provider = services.BuildServiceProvider();

            var reader = new TestScriptFileLoader();
            var scriptContent = reader.ReadAllLinesAsync("8-VariableResolution.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);

            _variableProvider = provider.GetRequiredService<IVariableProvider>();

            _subject = provider.GetRequiredService<IBlockExecutor>();
        }

        [Fact]
        public async void Execute_Request_Named_Can_resolve_response_variable()
        {
            // Execute datasource
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            // execute test
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/anything/1", _variableProvider.Resolve("{{eightVariables1.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Request_Named_Can_resolve_response_variable_2()
        {
            // Execute datasource
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            // execute test
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(2));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/anything/1", _variableProvider.Resolve("{{eightVariables2.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Request_Named_Can_resolve_response_variable_3()
        {
            // Execute datasource
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            // execute test
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/anything/1", _variableProvider.Resolve("{{eightVariables3.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Request_Named_Can_resolve_response_variable_5()
        {
            // Execute datasource
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(4));

            // execute test
            var resolved = _variableProvider.Resolve("{{dataSource2.response.body.$.results[?(@.version=='1.0.0' && @.distribute==true)].id}}");

            Assert.Equal("60226a065c485be40604bd2f", resolved);
        }

        [Fact]
        public async void Execute_Request_warnings_when_invalid_variable()
        {
            _appOptions.Setup(x => x.TerminateOnVariableResolutionFailure).Returns(true);

            // Execute test
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(6));

            Assert.Equal(1, result.Item1.Count());
        }

        [Fact]
        public async void Execute_Request_warnings_when_invalid_variable_doesnt_execute_request()
        {
            _appOptions.Setup(x => x.TerminateOnVariableResolutionFailure).Returns(true);

            // Execute test
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(6));

            Assert.Null(result.Item3);
        }

        [Fact]
        public async void Execute_Request_with_spaces_in_quoted_jsonPath_selector()
        {
            // Execute datasource
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            // execute test
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(7));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("http://httpbin.org/anything/1", _variableProvider.Resolve("{{eightVariables7.response.body.$.url}}"));
        }
    }
}
