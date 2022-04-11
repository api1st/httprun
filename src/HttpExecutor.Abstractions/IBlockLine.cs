namespace HttpExecutor.Abstractions
{
    public interface IBlockLine
    {
        LineType LineType { get; }

        string Raw { get; }

        IBlockLine? Previous { get; set; }

        int LineNumber { get; }
    }
}