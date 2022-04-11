namespace HttpExecutor.Abstractions
{
    public class Warning
    {
        public Warning(string name, WarningType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public WarningType Type { get; }
    }
}