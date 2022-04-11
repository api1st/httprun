using System;
using HttpExecutor.Abstractions;
using Xunit.Abstractions;

namespace HttpExecutor.Tests.Integration.Services
{
    public class TestConsole : IConsole
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestConsole(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public string ReadLine()
        {
            return "y";
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
            _outputHelper.WriteLine(text);
        }

        public void WriteLine()
        {
            Console.WriteLine();
            _outputHelper.WriteLine("");
        }
    }
}