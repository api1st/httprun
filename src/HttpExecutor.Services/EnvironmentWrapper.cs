using System;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class EnvironmentWrapper : IEnvironment
    {
        public void Exit(int code)
        {
            Environment.Exit(code);
        }

        public string? GetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }
    }
}