using System;
using System.Text.RegularExpressions;

namespace HttpExecutor.Services
{
    public class GuidVariableResolver : IDynamicVariableResolver
    {
        private const string VariableRegex = "{{\\$guid}}";

        public bool Resolvable(string content)
        {
            var regex = new Regex(VariableRegex);
            return regex.IsMatch(content);
        }

        public string Resolve(string content)
        {
            var regex = new Regex(VariableRegex);
            return regex.Replace(content, Guid.NewGuid().ToString("D"), 1);
        }
    }
}