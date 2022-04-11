using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class VariableLine : IBlockLine
    {
        private const string GroupsRegex = "^@([a-zA-Z_]?[a-zA-Z0-9_]*)\\s*=\\s*(.*)$";

        public VariableLine(string value, IBlockLine previous, int lineNumber)
        {
            Raw = value;
            Previous = previous;
            LineNumber = lineNumber;
        }

        public LineType LineType => LineType.VariableDefinition;

        public string Raw { get; }

        public IBlockLine Previous { get; set; }

        public int LineNumber { get; }

        public string VariableName => new Regex(GroupsRegex).Match(Raw).Groups[1].Value;

        public string VariableValue => new Regex(GroupsRegex).Match(Raw).Groups[2].Value;
    }
}