using System;
using System.Linq;
using HttpExecutor.Abstractions;
using HttpExecutor.Ioc;
using HttpExecutor.Services;
using HttpExecutor.Tests.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace HttpExecutor.Tests.Integration
{
    public class Executor_Post_Fixture : IClassFixture<PostFileBaseFixture>
    {
        private readonly HttpFile _httpFile;
        private readonly IBlockExecutor _subject;
        private readonly IVariableProvider _variableProvider;

        public Executor_Post_Fixture(ITestOutputHelper outputHelper)
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
            var scriptContent = reader.ReadAllLinesAsync("Scripts/3-POSTs.http").Result;

            var parser = provider.GetRequiredService<IParser>();
            _httpFile = parser.Parse(scriptContent);

            _variableProvider = provider.GetRequiredService<IVariableProvider>();

            _subject = provider.GetRequiredService<IBlockExecutor>();
        }

        [Fact]
        public async void Execute_POST_NoBody_200_Response()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(0));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("0", _variableProvider.Resolve("{{postNoBody1.response.body.$.headers.Content-Length}}"));
        }

        [Fact]
        public async void Execute_POST_PlainTextBody()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(1));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("Hello, this is the body.", _variableProvider.Resolve("{{postPlainText2.response.body.$.data}}").Trim());
        }

        [Fact]
        public async void Execute_POST_NoContentType()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(2));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("Hello, this is the body.", _variableProvider.Resolve("{{postNoContentType3.response.body.$.data}}").Trim());
        }

        [Fact]
        public async void Execute_POST_File_TextPlain()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(3));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("This is a text file with some content", _variableProvider.Resolve("{{postFileContentText4.response.body.$.data}}"));
        }

        [Fact]
        public async void Execute_POST_File_Image_Png()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(4));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("563", _variableProvider.Resolve("{{postFileContentBinary5.response.body.$.headers.Content-Length}}"));
        }

        [Fact]
        public async void Execute_POST_File_Missing()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(5));

            Assert.Equal(200, result.Item3?.StatusCode);

            // When no suitable file is found, it uses the text as normal body content.
            Assert.Equal("< Scripts/invalid.txt", _variableProvider.Resolve("{{postFileContentMissing6.response.body.$.data}}").Trim());
        }

        [Fact]
        public async void Execute_POST_File_TextPlain_NestedRelativeDirectory()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(6));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("This is a text file with some content - Child", _variableProvider.Resolve("{{postFileContentText7.response.body.$.data}}"));
        }

        [Fact]
        public async void Execute_POST_File_TextPlain_WithVariableReplacement()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(7));

            Assert.Equal(200, result.Item3?.StatusCode);
            // Can't use _variableResolver to read the value, as it will resolve the unresolved variables
            var json = JObject.Parse(result.Item3.Body);
            if (json == null)
            {
                throw new NullReferenceException("result was not json.");
            }
            Assert.Equal("This is a text file with some content Hello it has been replaced", json.SelectToken("$.data")?.Value<string>());
        }

        [Fact]
        public async void Execute_POST_File_TextPlain_WithVariableReplacement_NotReplaced()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(8));

            Assert.Equal(200, result.Item3?.StatusCode);

            // Can't use the variable resolver here or it will do replacements on the returned payload too.
            var responseBody = JObject.Parse(result.Item3.Body);
            if (responseBody == null)
            {
                throw new NullReferenceException("result was not json.");
            }
            Assert.Equal("This is a text file with some content {{myLocalVariable}}", responseBody.GetValue("data")?.Value<string>());
        }

        [Fact]
        public async void Execute_POST_File_TextPlain_WithInvalidVariableReplacement()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(9));

            Assert.Equal(200, result.Item3?.StatusCode);
            // Can't use _variableResolver to read the value, as it will resolve the unresolved variables
            var json = JObject.Parse(result.Item3.Body);
            if (json == null)
            {
                throw new NullReferenceException("result was not json.");
            }
            Assert.Equal("This is a text file with some content {{invalidVariable}}", json.SelectToken("$.data")?.Value<string>());
        }
        
        [Fact]
        public async void Execute_POST_ExtraSpacesAfterMediaType()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(10));

            Assert.Equal(200, result.Item3?.StatusCode);
        }

        [Fact]
        public async void Execute_POST_xwwwformurlencoded_with_spaces()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(11));

            Assert.Equal(200, result.Item3?.StatusCode);
            Assert.Equal("This is a password that contains spaces!", _variableProvider.Resolve("{{postForm12.response.body.$.form.password}}"));
        }

        [Fact]
        public async void Execute_POST_xwwwformurlencoded_with_spaces_to_https()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(12));

            Assert.Equal(401, result.Item3?.StatusCode);
        }

        [Fact]
        public async void Execute_POST_xwwwformurlencoded_with_spaces_to_https_variables_for_Host_and_port()
        {
            var result = await _subject.ExecuteAsync(_httpFile.Blocks.ElementAt(13));

            Assert.Equal(401, result.Item3?.StatusCode);
        }
    }
}
