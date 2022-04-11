namespace HttpExecutor.Abstractions
{
    public class HttpHeader : IHttpHeader
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}