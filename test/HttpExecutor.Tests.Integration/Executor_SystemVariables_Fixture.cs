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
    public class Executor_SystemVariables_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private readonly HttpFile _httpFile;
        private readonly IBlockExecutor _subject;
        private readonly IVariableProvider _variableProvider;
        private readonly IDateTimeNowProvider _dateTimeNowProvider;
        private readonly IEnvironment _environment;

        public Executor_SystemVariables_Fixture(ITestOutputHelper outputHelper)
        {
            var services = new ServiceCollection();
            services.AddSingleton(outputHelper);
            services.AddExecutorServices();
            services.AddTransient<IConsole, TestConsole>();
            services.AddTransient<ISleeper, TestSleeper>();
            services.AddSingleton<IEnvironment, TestEnvironment>();
            services.AddSingleton<IAppOptions>(new AppOptions { RequestTimeout = 30000 });
            services.AddSingleton<IDateTimeNowProvider, TestDateTimeNowProvider>();

            var provider = services.BuildServiceProvider();

            var reader = new TestScriptFileLoader();
            var scriptContent = reader.ReadAllLinesAsync("Scripts/2-SystemVariables.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);

            _variableProvider = provider.GetRequiredService<IVariableProvider>();

            _subject = provider.GetRequiredService<IBlockExecutor>();

            _dateTimeNowProvider = provider.GetRequiredService<IDateTimeNowProvider>();

            _environment = provider.GetRequiredService<IEnvironment>();
        }

        [Fact]
        public async void Execute_Guid_Resolves()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            Assert.True(Guid.TryParse(_variableProvider.Resolve("{{myGuid}}"), out Guid _));
        }

        [Fact]
        public async void Execute_RandomInt_0_1()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            var value = _variableProvider.Resolve("{{myRandomInt1}}");
            Assert.InRange<int>(int.Parse(value), 0, 1);
        }

        [Fact]
        public async void Execute_RandomInt_5_10()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            var value = _variableProvider.Resolve("{{myRandomInt2}}");
            Assert.InRange<int>(int.Parse(value), 5, 10);
        }

        [Fact]
        public async void Execute_RandomInt_4_4()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            var value = _variableProvider.Resolve("{{myRandomInt3}}");
            Assert.InRange<int>(int.Parse(value), 4, 4);
        }

        [Theory]
        [InlineData("myTimeStamp1", 1577836800)]
        [InlineData("myTimeStamp2", 1577836801)]
        [InlineData("myTimeStamp3", 1577837800)]
        [InlineData("myTimeStamp4", 1577776800)]
        [InlineData("myTimeStamp5", 1577818800)]
        [InlineData("myTimeStamp6", 1578182400)]
        [InlineData("myTimeStamp7", 1577232000)]
        [InlineData("myTimeStamp8", 1583020800)]
        [InlineData("myTimeStamp9", 1514764800)]
        public async void Execute_Timestamp(string variableName, long expectedTimestamp)
        {
            var nowDate = new DateTime(2020, 01, 01);
            ((TestDateTimeNowProvider)_dateTimeNowProvider).SetNow(nowDate);

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(2));

            var value = _variableProvider.Resolve($"{{{{{variableName}}}}}");
            Assert.Equal(expectedTimestamp, long.Parse(value));
        }

        [Theory]
        [InlineData("myDateTime1", "Wed, 01 Jan 2020 00:00:00 GMT")]
        [InlineData("myDateTime2", "2020-01-01T00:00:00.0000000")]
        [InlineData("myDateTime3", "01-01-2020")]
        [InlineData("myDateTime4", "2020-01-01")]
        [InlineData("myDateTime5", "Wed, 01 Jan 2020 00:00:01 GMT")]
        [InlineData("myDateTime6", "Wed, 01 Jan 2020 00:25:00 GMT")]
        [InlineData("myDateTime7", "Tue, 31 Dec 2019 15:40:00 GMT")]
        [InlineData("myDateTime8", "Tue, 31 Dec 2019 21:00:00 GMT")]
        [InlineData("myDateTime9", "Sun, 29 Dec 2019 00:00:00 GMT")]
        [InlineData("myDateTime10", "Wed, 15 Jan 2020 00:00:00 GMT")]
        [InlineData("myDateTime11", "Sun, 01 Dec 2019 00:00:00 GMT")]
        [InlineData("myDateTime12", "Sun, 01 Jan 2023 00:00:00 GMT")]
        [InlineData("myDateTime13", "2020-01-01T00:00:01.0500000")]
        [InlineData("myDateTime14", "2020-01-01T00:25:00.0000000")]
        [InlineData("myDateTime15", "2019-12-31T15:40:00.0000000")]
        [InlineData("myDateTime16", "2019-12-31T21:00:00.0000000")]
        [InlineData("myDateTime17", "2019-12-29T00:00:00.0000000")]
        [InlineData("myDateTime18", "2020-01-15T00:00:00.0000000")]
        [InlineData("myDateTime19", "2019-12-01T00:00:00.0000000")]
        [InlineData("myDateTime20", "2023-01-01T00:00:00.0000000")]
        [InlineData("myDateTime21", "01-01-2020 120001")]
        [InlineData("myDateTime22", "01-01-2020 122500")]
        [InlineData("myDateTime23", "31-12-2019 034000")]
        [InlineData("myDateTime24", "31-12-2019 090000")]
        [InlineData("myDateTime25", "29-12-2019 120000")]
        [InlineData("myDateTime26", "15-01-2020 120000")]
        [InlineData("myDateTime27", "01-12-2019 120000")]
        [InlineData("myDateTime28", "01-01-2023 120000")]
        [InlineData("myDateTime29", "2020-01-01 120001")]
        [InlineData("myDateTime30", "2020-01-01 122500")]
        [InlineData("myDateTime31", "2019-12-31 034000")]
        [InlineData("myDateTime32", "2019-12-31 090000")]
        [InlineData("myDateTime33", "2019-12-29 120000")]
        [InlineData("myDateTime34", "2020-01-15 120000")]
        [InlineData("myDateTime35", "2019-12-01 120000")]
        [InlineData("myDateTime36", "2023-01-01 120000")]
        [InlineData("myDateTime37", "2020-01-01T12:00:00.0000+00:00")]
        public async void Execute_DateTime(string variableName, string expectedOutput)
        {
            var nowDate = new DateTime(2020, 01, 01);
            ((TestDateTimeNowProvider)_dateTimeNowProvider).SetNow(nowDate);

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));

            var value = _variableProvider.Resolve($"{{{{{variableName}}}}}");
            Assert.Equal(expectedOutput, value);
        }

        [Theory]
        [InlineData("myLocalDateTime1", "Wed, 01 Jan 2020 00:00:00 GMT")]
        [InlineData("myLocalDateTime2", "2020-01-01T00:00:00.0000000+00:00")]
        [InlineData("myLocalDateTime3", "01-01-2020")]
        [InlineData("myLocalDateTime4", "2020-01-01")]
        [InlineData("myLocalDateTime5", "Wed, 01 Jan 2020 00:00:01 GMT")]
        [InlineData("myLocalDateTime6", "Wed, 01 Jan 2020 00:25:00 GMT")]
        [InlineData("myLocalDateTime7", "Tue, 31 Dec 2019 15:40:00 GMT")]
        [InlineData("myLocalDateTime8", "Tue, 31 Dec 2019 21:00:00 GMT")]
        [InlineData("myLocalDateTime9", "Sun, 29 Dec 2019 00:00:00 GMT")]
        [InlineData("myLocalDateTime10", "Wed, 15 Jan 2020 00:00:00 GMT")]
        [InlineData("myLocalDateTime11", "Sun, 01 Dec 2019 00:00:00 GMT")]
        [InlineData("myLocalDateTime12", "Sun, 01 Jan 2023 00:00:00 GMT")]
        [InlineData("myLocalDateTime13", "2020-01-01T00:00:01.0500000")]
        [InlineData("myLocalDateTime14", "2020-01-01T00:25:00.0000000")]
        [InlineData("myLocalDateTime15", "2019-12-31T15:40:00.0000000")]
        [InlineData("myLocalDateTime16", "2019-12-31T21:00:00.0000000")]
        [InlineData("myLocalDateTime17", "2019-12-29T00:00:00.0000000")]
        [InlineData("myLocalDateTime18", "2020-01-15T00:00:00.0000000")]
        [InlineData("myLocalDateTime19", "2019-12-01T00:00:00.0000000")]
        [InlineData("myLocalDateTime20", "2023-01-01T00:00:00.0000000")]
        [InlineData("myLocalDateTime21", "01-01-2020 120001")]
        [InlineData("myLocalDateTime22", "01-01-2020 122500")]
        [InlineData("myLocalDateTime23", "31-12-2019 034000")]
        [InlineData("myLocalDateTime24", "31-12-2019 090000")]
        [InlineData("myLocalDateTime25", "29-12-2019 120000")]
        [InlineData("myLocalDateTime26", "15-01-2020 120000")]
        [InlineData("myLocalDateTime27", "01-12-2019 120000")]
        [InlineData("myLocalDateTime28", "01-01-2023 120000")]
        [InlineData("myLocalDateTime29", "2020-01-01 120001")]
        [InlineData("myLocalDateTime30", "2020-01-01 122500")]
        [InlineData("myLocalDateTime31", "2019-12-31 034000")]
        [InlineData("myLocalDateTime32", "2019-12-31 090000")]
        [InlineData("myLocalDateTime33", "2019-12-29 120000")]
        [InlineData("myLocalDateTime34", "2020-01-15 120000")]
        [InlineData("myLocalDateTime35", "2019-12-01 120000")]
        [InlineData("myLocalDateTime36", "2023-01-01 120000")]
        public async void Execute_LocalDateTime(string variableName, string expectedOutput)
        {
            var nowDate = new DateTime(2020, 01, 01);
            ((TestDateTimeNowProvider)_dateTimeNowProvider).SetNow(nowDate);

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(4));

            var value = _variableProvider.Resolve($"{{{{{variableName}}}}}");
            Assert.Equal(expectedOutput, value);
        }

        [Fact]
        public async void Execute_EnvironmentVariable_Resolves()
        {
            ((TestEnvironment)_environment).Register("USERNAME", "Ash");

            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(5));
            
            var value = _variableProvider.Resolve("{{myEnvVariable1}}");
            Assert.Equal("Ash", value);
        }
    }
}
