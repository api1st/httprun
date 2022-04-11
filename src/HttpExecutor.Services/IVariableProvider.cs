using System.Collections.Generic;

namespace HttpExecutor.Services
{
    public interface IVariableProvider
    {
        IEnumerable<string> UnresolvedVariableLookups { get; }

        void ResetLookupWarnings();

        string Register(string name, string value);

        string Resolve(string content);

        void FixFileVariables();
    }
}