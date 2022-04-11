using System.Collections.Generic;
using System.Drawing;
using HttpExecutor.Abstractions;
using Pastel;

namespace HttpExecutor.Services
{
    public class FailedHttpResponse : IHttpResponse
    {
        public int StatusCode => -1;

        public ICollection<IHttpHeader> Headers { get; set; }

        public string Body { get; set; }
        
        public override string ToString()
        {
            return Body.Pastel(Color.Firebrick);
        }
    }
}