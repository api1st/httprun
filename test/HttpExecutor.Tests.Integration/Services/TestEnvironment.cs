using System.Collections.Generic;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Tests.Integration.Services
{
    public class TestEnvironment : IEnvironment
    {
        private IDictionary<string, string> _values = new Dictionary<string, string>();

        public void Exit(int code)
        {
            // Do nothing
        }

        public void Register(string name, string value)
        {
            _values.Add(name, value);
        }

        public string? GetEnvironmentVariable(string variable)
        {
            return _values[variable];
        }
    }
}