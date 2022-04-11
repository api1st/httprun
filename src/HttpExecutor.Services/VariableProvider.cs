using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HttpExecutor.Services
{
    public class VariableProvider : IVariableProvider
    {
        private readonly IRequestVariableProvider _requestVariableProvider;
        private readonly IEnumerable<IDynamicVariableResolver> _dynamicVariableResolvers;
        private IList<string> _unresolvedVariables;
        private const string ContainsVariableRegex = "{{([^}]*)}}";

        private IDictionary<string, string> _variables = new Dictionary<string, string>();

        public VariableProvider(IRequestVariableProvider requestVariableProvider, IEnumerable<IDynamicVariableResolver> dynamicVariableResolvers)
        {
            _requestVariableProvider = requestVariableProvider ?? throw new ArgumentNullException(nameof(requestVariableProvider));
            _dynamicVariableResolvers = dynamicVariableResolvers ?? throw new ArgumentNullException(nameof(dynamicVariableResolvers));
            _unresolvedVariables = new List<string>();
        }

        public IEnumerable<string> UnresolvedVariableLookups => _unresolvedVariables;

        public void ResetLookupWarnings()
        {
            _unresolvedVariables = new List<string>();
        }

        public string Register(string name, string value)
        {
            string warning = null;

            if (_variables.ContainsKey(name))
            {
                warning = name;
            }

            _variables[name] = value;

            return warning;
        }

        public string Resolve(string content)
        {
            var original = content;

            var regex = new Regex(ContainsVariableRegex);

            var hasVariableSubstitution = false;

            while (regex.IsMatch(content))
            {
                var processed = false;
                hasVariableSubstitution = true;

                // System variables (start with '$')
                if (_dynamicVariableResolvers != null)
                {
                    foreach (var dynamicResolver in _dynamicVariableResolvers)
                    {
                        if (dynamicResolver.Resolvable(content))
                        {
                            content = dynamicResolver.Resolve(content);
                            processed = true;
                        }
                    }
                }

                // Order of resolution precedence: Request variable (# @name... ) -> File Variable (@myvar = ...) -> Env Variables (VS Code specific, not supported in the cli)

                // Request Variables
                var referencedVariable = regex.Match(content).Groups[1].Value;

                if (referencedVariable.Contains("."))
                {
                    var requestName = referencedVariable.Substring(0, referencedVariable.IndexOf("."));

                    if (_requestVariableProvider.HasNamedRequest(requestName))
                    {
                        content = regex.Replace(content, _requestVariableProvider.Resolve(referencedVariable), 1);
                        processed = true;
                    }
                }

                // File variables
                if (_variables.ContainsKey(referencedVariable))
                {
                    content = regex.Replace(content, _variables[referencedVariable], 1);
                    processed = true;
                }

                if (!processed)
                {
                    break;
                }
            }

            if (hasVariableSubstitution)
            {
                if (original.Equals(content))
                {
                    _unresolvedVariables.Add(original);
                }
            }

            // When no matches, then all variables resolved.
            return content;
        }

        public void FixFileVariables()
        {
            var fixedDictionary = new Dictionary<string, string>();

            foreach (var (name, value) in _variables)
            {
                // May have issue with cyclical references here not being resolved.
                var fixedValue = Resolve(value);
                fixedDictionary.Add(name, fixedValue);
            }

            _variables = fixedDictionary;
        }
    }
}