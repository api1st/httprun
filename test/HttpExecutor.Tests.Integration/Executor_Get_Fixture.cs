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
    public class Executor_Get_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private readonly HttpFile _httpFile;
        private readonly IBlockExecutor _subject;
        private readonly IVariableProvider _variableProvider;
        private readonly Mock<IAppOptions> _appOptions;

        public Executor_Get_Fixture(ITestOutputHelper outputHelper)
        {
            _appOptions = new Mock<IAppOptions>();
            _appOptions.Setup(x => x.AddUserAgent).Returns(true);
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
            var scriptContent = reader.ReadAllLinesAsync("Scripts/1-GETs.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);

            _variableProvider = provider.GetRequiredService<IVariableProvider>();

            _subject = provider.GetRequiredService<IBlockExecutor>();
        }

        [Fact]
        public async void Execute_Request_Named_Can_resolve_response_variable()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("http://httpbin.org/get", _variableProvider.Resolve("{{oneGet1.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Request_2nd_In_script_Reference_1st_response()
        {
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));
            var headerValue = _variableProvider.Resolve("{{oneGet1.response.headers.content-length}}");
            
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal($"http://httpbin.org/get?id={headerValue}", _variableProvider.Resolve("{{oneGet2.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Request_Invalid_request_reference()
        {
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));
            
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(2));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("http://httpbin.org/get?id={{oneGetInvalid.response.headers.content-length}}", _variableProvider.Resolve("{{oneGet3.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_VariableOnlyBlock_Resolves()
        {
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));

            Assert.Equal("http://httpbin.org/get", _variableProvider.Resolve("{{url}}"));
        }

        [Fact]
        public async void Execute_Variable_in_header_value_Resolves()
        {
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(4));

            // Httpbin capitalises the property names, should actually be "X-Executor-test" (lowercase T)
            Assert.Equal("application/json", _variableProvider.Resolve("{{oneGet4.response.body.$.headers.X-Executor-Test}}"));
        }

        [Fact]
        public async void Execute_Variable_in_header_name_Resolves()
        {
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));
            await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(5));

            Assert.NotNull(result.Item2?.Headers.FirstOrDefault(x => x.Name == "X-Ash"));
            Assert.Equal("Has a value", _variableProvider.Resolve("{{oneGet5.response.body.$.headers.X-Ash}}"));
        }

        [Fact]
        public async void Execute_FullUriInPath_Resolves()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(6));

            Assert.Equal("httpbin.org", _variableProvider.Resolve("{{oneGet6.response.body.$.headers.Host}}"));
        }

        [Fact]
        public async void Execute_302_follow_redirect_https_to_https()
        {
            _appOptions.Setup(x => x.Follow300Responses).Returns(true);

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(8));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("https://httpbingo.org/get", _variableProvider.Resolve("{{oneGet8.response.body.$.url}}"));
        }

        // [Fact] Bug at httpbin.org means redirects don't work properly at the moment.
        // httpbingo won't redirect outside of its own domain..
        private async void Execute_302_follow_redirect_https_to_http()
        {
            _appOptions.Setup(x => x.Follow300Responses).Returns(true);

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(9));

            Assert.Equal("Following of HTTP Redirect failed, possibly due to https->http redirection.", result.Item2?.Body);
        }

        [Fact]
        public async void Execute_302_dont_follow_redirect_https_to_https()
        {
            _appOptions.Setup(x => x.Follow300Responses).Returns(false);

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(8));

            Assert.Equal(302, result.Item3?.StatusCode);
        }

        [Fact]
        public async void Execute_Get_short_timeout_errors()
        {
            _appOptions.Setup(x => x.RequestTimeout).Returns(1000);
            _appOptions.Setup(x => x.Follow300Responses).Returns(true);

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(10));

            Assert.Equal("HTTP Request timeout.", result.Item3?.Body);
        }

        [Fact]
        public async void Execute_Get_with_At_sign()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(11));

            Assert.Equal("http://httpbin.org/anything/me@you.com", _variableProvider.Resolve("{{oneGet11.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Get_with_colon_sign()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(12));

            Assert.Equal("http://httpbin.org/anything/me:you", _variableProvider.Resolve("{{oneGet12.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Get_Path_Is_Variable()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(13));

            Assert.Equal("http://httpbin.org/anything/hello", _variableProvider.Resolve("{{oneGet13.response.body.$.url}}"));
        }
        
        [Fact]
        public async void Execute_Get_Https_in_Path()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(14));

            Assert.Equal("https://httpbingo.org/get", _variableProvider.Resolve("{{oneGetHttps14.response.body.$.url}}"));
        }

        [Fact]
        public async void Execute_Get_User_Agent_Added()
        {
            _appOptions.Setup(x => x.AddUserAgent).Returns(true);
            _appOptions.Setup(x => x.UserAgent).Returns("httprun");

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(15));
            
            Assert.Equal("httprun", _variableProvider.Resolve("{{oneGetUserAgent15.response.body.$.headers.User-Agent}}"));
        }

        [Fact]
        public async void Execute_Get_User_Agent_Not_Added_Due_to_options()
        {
            _appOptions.Setup(x => x.AddUserAgent).Returns(false);
            _appOptions.Setup(x => x.UserAgent).Returns("httprun");

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(15));

            // The search jsonPath means it wasn't found.
            Assert.Equal("oneGetUserAgent15.response.body.$.headers.User-Agent", _variableProvider.Resolve("{{oneGetUserAgent15.response.body.$.headers.User-Agent}}"));
        }

        [Fact]
        public async void Execute_Get_User_Agent_Not_Added_Due_to_options_2()
        {
            _appOptions.Setup(x => x.AddUserAgent).Returns(true);
            _appOptions.Setup(x => x.UserAgent).Returns("");

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(15));

            // The search jsonPath means it wasn't found.
            Assert.Equal("oneGetUserAgent15.response.body.$.headers.User-Agent", _variableProvider.Resolve("{{oneGetUserAgent15.response.body.$.headers.User-Agent}}"));
        }

        [Fact]
        public async void Execute_Get_User_Agent_Not_Added()
        {
            _appOptions.Setup(x => x.AddUserAgent).Returns(true);
            _appOptions.Setup(x => x.UserAgent).Returns("httprun");

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(16));

            Assert.Equal("my-custom-agent", _variableProvider.Resolve("{{oneGetUserAgent16.response.body.$.headers.User-Agent}}"));
        }
    }
}
