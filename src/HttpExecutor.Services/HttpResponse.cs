using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;
using Pastel;

namespace HttpExecutor.Services
{
    public class HttpResponse : IHttpResponse
    {
        public HttpResponse()
        {
            Headers = new List<IHttpHeader>();
        }

        public int StatusCode { get; set; }

        public string StatusPhrase { get; set; }

        public ICollection<IHttpHeader> Headers { get; }

        public string Body { get; set; }
        
        public override string ToString()
        {
            var ret = new List<string>();

            ret.Add($"{"HTTP/1.1".Pastel(ResponseColours.Normal)} {StatusCode.ToString().Pastel(ResponseColours.StatusCode)} {StatusPhrase.Pastel(ResponseColours.Normal)}");

            foreach (var header in Headers)
            {
                ret.Add($"{header.Name.Pastel(ResponseColours.HeaderName)}{":".Pastel(ResponseColours.Normal)} {header.Value.Pastel(ResponseColours.HeaderValue)}");
            }

            ret.Add("");

            if (Headers.Any(x => x.Name.ToLower() == "content-type" && x.Value.Contains("application/json")))
            {
                ret.Add(ColourisePrettyJson(Body));
            }
            else
            {
                ret.Add(Body);
            }

            return string.Join(Environment.NewLine, ret);
        }

        private static string ColourisePrettyJson(string original)
        {
            original = JsonPrettyPrint.FormatJson(original);

            return Regex.Replace(
                original,
                @"(¤(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\¤])*¤(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)".Replace('¤', '"'),
                match => {
                    var cls = "number";
                    if (Regex.IsMatch(match.Value, @"^¤".Replace('¤', '"')))
                    {
                        if (Regex.IsMatch(match.Value, ":$"))
                        {
                            cls = "key";
                        }
                        else
                        {
                            cls = "string";
                        }
                    }
                    else if (Regex.IsMatch(match.Value, "true|false"))
                    {
                        cls = "boolean";
                    }
                    else if (Regex.IsMatch(match.Value, "null"))
                    {
                        cls = "null";
                    }

                    switch (cls)
                    {
                        case "key":
                            return match.Value.Pastel(ResponseColours.JsonPropertyName);

                        case "string":
                            return match.Value.Pastel(ResponseColours.JsonPropertyValueString);

                        default:
                        case "number":
                        case "boolean":
                        case "null":
                            return match.Value.Pastel(ResponseColours.JsonPropertyValueStrut);
                    }
                });
        }
    }
}