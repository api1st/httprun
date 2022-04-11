using System.Linq;
using HttpExecutor.Abstractions;
using HttpExecutor.Ioc;
using HttpExecutor.Services;
using HttpExecutor.Tests.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace HttpExecutor.Tests.Integration
{
    public class Executor_FormMultipart_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private HttpFile _httpFile;
        private IBlockExecutor _subject;
        private IVariableProvider _variableProvider;
        private IAppOptions _appOptions;
        
        public Executor_FormMultipart_Fixture(ITestOutputHelper outputHelper)
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
            var scriptContent = reader.ReadAllLinesAsync("6-FormMultipart.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);

            _variableProvider = provider.GetRequiredService<IVariableProvider>();

            _subject = provider.GetRequiredService<IBlockExecutor>();
        }

        [Fact]
        public async void Execute_Request_Multipart_single_value()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("Hello, this is the message", _variableProvider.Resolve("{{sixForm1.response.body.$.form.message}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_multi_value()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("Hello, this is the message", _variableProvider.Resolve("{{sixForm2.response.body.$.form.message}}"));
            Assert.Equal("This is the second message part", _variableProvider.Resolve("{{sixForm2.response.body.$.form.message2}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_single_file()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(2));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("This is a text file with some content", _variableProvider.Resolve("{{sixForm3.response.body.$.files.file}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_multiple_same_file()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("This is a text file with some content", _variableProvider.Resolve("{{sixForm4.response.body.$.files.file}}"));
            Assert.Equal("This is a text file with some content", _variableProvider.Resolve("{{sixForm4.response.body.$.files.file2}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_multiple_different_file()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(4));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("This is a text file with some content", _variableProvider.Resolve("{{sixForm5.response.body.$.files.file}}"));
            Assert.Equal("data:application/octet-stream;base64,iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAALEgAACxIB0t1+/AAAAdVJREFUaN7tmc1thDAQRimBElwCJVBCSvAxR5fgEiiBEiiBErhyIx24A2cc2WhiAf4ZA1rJkZ4UZZPN9/AwHrON1rr5ZJoqUAWqQBWoAlWgxJf++WaAAGZAAdpD2dfM7zDS/yopAGE6YDoIHMLIdK8KQIAWGIAtQ8Bh/r59bQWQjCBILCkSJIF1XVuAA9Jivm9ROd0ukS0AQTtgA7SH+Vn31EoEBSAMA2YUUAHiJDyWcCtBuidIArZEroJewVEpjQSJjiIgMsMbpHdjf53sCcEWSxEYCQKOyZQhkshZBZYkYEtHeLVPQSGJnHIS0QI2/FIo+L+VILTXOUVA3BD+D3Q/pAqoFIEebUxFQQLJN/Ojo0TEqDG/JgBv1hdgeVNAP4CKPSvkCKiCQc1KSMRs2+x902hO/Z4cYFhgWOQHY8zo9hOKgCCGH71BEXcqHjEBKDft5gowypVH4YeLgKE9ZSO10cxz7z7TFJqxOEUgZxyYbPi+0M4uSRuZPYCnCPBA6TwrYCWWyFbJImo/FTMpM6pAG5CYvDO0LDii7x2JNAtdSGxuQyp41Q87UqkHW8NJzYsbw+8d6Y5Hi+7qbw8IyOIPd9HRVD8qUD8fqAJVoApUgSrwqfwCJ6xaZshM+xMAAAAASUVORK5CYII=", _variableProvider.Resolve("{{sixForm5.response.body.$.files.file2}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_Value_then_file()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(5));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("Hello, this is the message", _variableProvider.Resolve("{{sixForm6.response.body.$.form.message}}"));
            Assert.Equal("data:application/octet-stream;base64,iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAALEgAACxIB0t1+/AAAAdVJREFUaN7tmc1thDAQRimBElwCJVBCSvAxR5fgEiiBEiiBErhyIx24A2cc2WhiAf4ZA1rJkZ4UZZPN9/AwHrON1rr5ZJoqUAWqQBWoAlWgxJf++WaAAGZAAdpD2dfM7zDS/yopAGE6YDoIHMLIdK8KQIAWGIAtQ8Bh/r59bQWQjCBILCkSJIF1XVuAA9Jivm9ROd0ukS0AQTtgA7SH+Vn31EoEBSAMA2YUUAHiJDyWcCtBuidIArZEroJewVEpjQSJjiIgMsMbpHdjf53sCcEWSxEYCQKOyZQhkshZBZYkYEtHeLVPQSGJnHIS0QI2/FIo+L+VILTXOUVA3BD+D3Q/pAqoFIEebUxFQQLJN/Ojo0TEqDG/JgBv1hdgeVNAP4CKPSvkCKiCQc1KSMRs2+x902hO/Z4cYFhgWOQHY8zo9hOKgCCGH71BEXcqHjEBKDft5gowypVH4YeLgKE9ZSO10cxz7z7TFJqxOEUgZxyYbPi+0M4uSRuZPYCnCPBA6TwrYCWWyFbJImo/FTMpM6pAG5CYvDO0LDii7x2JNAtdSGxuQyp41Q87UqkHW8NJzYsbw+8d6Y5Hi+7qbw8IyOIPd9HRVD8qUD8fqAJVoApUgSrwqfwCJ6xaZshM+xMAAAAASUVORK5CYII=", _variableProvider.Resolve("{{sixForm6.response.body.$.files.file}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_file_then_value()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(6));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("Hello, this is the message", _variableProvider.Resolve("{{sixForm7.response.body.$.form.message}}"));
            Assert.Equal("data:application/octet-stream;base64,iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAALEgAACxIB0t1+/AAAAdVJREFUaN7tmc1thDAQRimBElwCJVBCSvAxR5fgEiiBEiiBErhyIx24A2cc2WhiAf4ZA1rJkZ4UZZPN9/AwHrON1rr5ZJoqUAWqQBWoAlWgxJf++WaAAGZAAdpD2dfM7zDS/yopAGE6YDoIHMLIdK8KQIAWGIAtQ8Bh/r59bQWQjCBILCkSJIF1XVuAA9Jivm9ROd0ukS0AQTtgA7SH+Vn31EoEBSAMA2YUUAHiJDyWcCtBuidIArZEroJewVEpjQSJjiIgMsMbpHdjf53sCcEWSxEYCQKOyZQhkshZBZYkYEtHeLVPQSGJnHIS0QI2/FIo+L+VILTXOUVA3BD+D3Q/pAqoFIEebUxFQQLJN/Ojo0TEqDG/JgBv1hdgeVNAP4CKPSvkCKiCQc1KSMRs2+x902hO/Z4cYFhgWOQHY8zo9hOKgCCGH71BEXcqHjEBKDft5gowypVH4YeLgKE9ZSO10cxz7z7TFJqxOEUgZxyYbPi+0M4uSRuZPYCnCPBA6TwrYCWWyFbJImo/FTMpM6pAG5CYvDO0LDii7x2JNAtdSGxuQyp41Q87UqkHW8NJzYsbw+8d6Y5Hi+7qbw8IyOIPd9HRVD8qUD8fqAJVoApUgSrwqfwCJ6xaZshM+xMAAAAASUVORK5CYII=", _variableProvider.Resolve("{{sixForm7.response.body.$.files.file}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_file_then_value_then_file2()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(7));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("Hello, this is the message", _variableProvider.Resolve("{{sixForm8.response.body.$.form.message}}"));
            Assert.Equal("data:application/octet-stream;base64,iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAALEgAACxIB0t1+/AAAAdVJREFUaN7tmc1thDAQRimBElwCJVBCSvAxR5fgEiiBEiiBErhyIx24A2cc2WhiAf4ZA1rJkZ4UZZPN9/AwHrON1rr5ZJoqUAWqQBWoAlWgxJf++WaAAGZAAdpD2dfM7zDS/yopAGE6YDoIHMLIdK8KQIAWGIAtQ8Bh/r59bQWQjCBILCkSJIF1XVuAA9Jivm9ROd0ukS0AQTtgA7SH+Vn31EoEBSAMA2YUUAHiJDyWcCtBuidIArZEroJewVEpjQSJjiIgMsMbpHdjf53sCcEWSxEYCQKOyZQhkshZBZYkYEtHeLVPQSGJnHIS0QI2/FIo+L+VILTXOUVA3BD+D3Q/pAqoFIEebUxFQQLJN/Ojo0TEqDG/JgBv1hdgeVNAP4CKPSvkCKiCQc1KSMRs2+x902hO/Z4cYFhgWOQHY8zo9hOKgCCGH71BEXcqHjEBKDft5gowypVH4YeLgKE9ZSO10cxz7z7TFJqxOEUgZxyYbPi+0M4uSRuZPYCnCPBA6TwrYCWWyFbJImo/FTMpM6pAG5CYvDO0LDii7x2JNAtdSGxuQyp41Q87UqkHW8NJzYsbw+8d6Y5Hi+7qbw8IyOIPd9HRVD8qUD8fqAJVoApUgSrwqfwCJ6xaZshM+xMAAAAASUVORK5CYII=", _variableProvider.Resolve("{{sixForm8.response.body.$.files.file}}"));
            Assert.Equal("This is a text file with some content", _variableProvider.Resolve("{{sixForm8.response.body.$.files.file2}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_value_with_variables()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(8));

            Assert.Equal(200, result.Item3.StatusCode);
            // Can't use _variableResolver to read the value, as it will resolve the unresolved variables
            var json = JObject.Parse(result.Item3.Body);
            Assert.Equal("Hello, this is This is the variable value. the message", json.SelectToken("$.form.message")?.Value<string>());
        }

        [Fact]
        public async void Execute_Request_Multipart_multiple_values_with_variables()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(9));

            Assert.Equal(200, result.Item3.StatusCode);
            // Can't use _variableResolver to read the value, as it will resolve the unresolved variables
            var json = JObject.Parse(result.Item3.Body);
            Assert.Equal("Hello, this is This is the variable value. the message", json.SelectToken("$.form.message")?.Value<string>());
            Assert.Equal("Hello, this is This is the other variable value the message", json.SelectToken("$.form.message2")?.Value<string>());
        }

        [Fact]
        public async void Execute_Request_Multipart_filename_containing_variable()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(10));

            Assert.Equal(200, result.Item3.StatusCode);
            // Can't use _variableResolver to read the value, as it will resolve the unresolved variables
            var json = JObject.Parse(result.Item3.Body);
            Assert.Equal("This is a text file with some content", json.SelectToken("$.files.file")?.Value<string>());
        }

        [Fact]
        public async void Execute_Request_Multipart_filename_containing_multiple_variables()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(11));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("This is a text file with some content - Child", _variableProvider.Resolve("{{sixForm12.response.body.$.files.file}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_file_contents_containing_variables_resolved()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(12));

            Assert.Equal(200, result.Item3.StatusCode);
            Assert.Equal("This is a text file with some content Hello it has been replaced", _variableProvider.Resolve("{{sixForm13.response.body.$.files.file}}"));
        }

        [Fact]
        public async void Execute_Request_Multipart_file_contents_containing_variables_unresolved()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(13));

            Assert.Equal(200, result.Item3.StatusCode);
            // Can't use _variableResolver to read the value, as it will resolve the unresolved variables
            var json = JObject.Parse(result.Item3.Body);
            Assert.Equal("This is a text file with some content {{myLocalVariable}}", json.SelectToken("$.files.file")?.Value<string>());
        }

        [Fact]
        public async void Execute_Request_Multipart_filename_with_variable_contents_containing_variables_resolved()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(14));

            Assert.Equal(200, result.Item3.StatusCode);
            // Can't use _variableResolver to read the value, as it will resolve the unresolved variables
            var json = JObject.Parse(result.Item3.Body);
            Assert.Equal("This is a text file with some content {{myLocalVariable}}", json.SelectToken("$.files.file")?.Value<string>());
        }

        [Fact]
        public async void Execute_Request_Multipart_filename_with_variable_contents_containing_variables_resolved_with_content()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(15));

            Assert.Equal(200, result.Item3.StatusCode);
            // Can't use _variableResolver to read the value, as it will resolve the unresolved variables
            var json = JObject.Parse(result.Item3.Body);
            Assert.Equal("This is a text file with some content Successfully replaced value.", json.SelectToken("$.files.file")?.Value<string>());
        }
    }
}
