using System;
using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class EnvironmentVariableResolver : IDynamicVariableResolver
    {
        private readonly IEnvironment _environment;
        private const string VariableRegex = "{{\\$processEnv ([_a-zA-Z][_a-zA-Z0-9]*)}}";

        public EnvironmentVariableResolver(IEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public bool Resolvable(string content)
        {
            var regex = new Regex(VariableRegex);
            return regex.IsMatch(content);
        }

        public string Resolve(string content)
        {
            var regex = new Regex(VariableRegex);
            var matches = regex.Match(content);
            var envVarName = matches.Groups[1].Value;
            return regex.Replace(content, _environment.GetEnvironmentVariable(envVarName), 1);
        }
    }
}