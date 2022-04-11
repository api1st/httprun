using System;
using System.Text.RegularExpressions;

namespace HttpExecutor.Services
{
    public class RandomIntVariableResolver : IDynamicVariableResolver
    {
        private const string VariableRegex = "{{\\$randomInt (\\d*) (\\d*)}}";

        public bool Resolvable(string content)
        {
            var regex = new Regex(VariableRegex);
            return regex.IsMatch(content);
        }

        public string Resolve(string content)
        {
            var regex = new Regex(VariableRegex);
            var matches = regex.Match(content);
            var min = int.Parse(matches.Groups[1].Value);
            var max = int.Parse(matches.Groups[2].Value);
            return regex.Replace(content, new Random().Next(min, max).ToString(), 1);
        }
    }
}