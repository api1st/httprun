using System;
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
    public class Executor_File_Missing_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private HttpFile _httpFile;
        private IBlockExecutor _subject;
        private IVariableProvider _variableProvider;
        private IAppOptions _appOptions;
        
        public Executor_File_Missing_Fixture(ITestOutputHelper outputHelper)
        {
            _appOptions = new AppOptions {RequestTimeout = 30000, TerminateOnFileAccessFailure = true};

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
            var scriptContent = reader.ReadAllLinesAsync("Scripts/9-Missing-File.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);

            _variableProvider = provider.GetRequiredService<IVariableProvider>();

            _subject = provider.GetRequiredService<IBlockExecutor>();
        }

        [Fact]
        public async void Execute_Request_Multipart_single_file()
        {
            await Assert.ThrowsAsync<Exception>(async () => await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0)));
        }
    }
}
