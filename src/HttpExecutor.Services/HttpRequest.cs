using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;
using Pastel;

namespace HttpExecutor.Services
{
    public class HttpRequest : IHttpRequest
    {
        public HttpRequest()
        {
            Headers = new List<IHttpHeader>();
            Files = new List<IRequestBodyFile>();
        }

        public string Name { get; set; }

        public string Verb { get; set; }

        public string Path { get; set; }

        public string Scheme { get; set; }

        public string Host { get; set; }

        public bool IsMultipartFormData { get; set; }

        public ICollection<IHttpHeader> Headers { get; }

        public string Body { get; set; }
        
        public ICollection<IRequestBodyFile> Files { get; set; }

        public bool RequiresConfirmation { get; set; }

        public override string ToString()
        {
            var ret = new List<string>();

            ret.Add($"{Verb.Pastel(RequestColours.Verb)} {Path.Pastel(RequestColours.Path)} {"HTTP/1.1".Pastel(RequestColours.Normal)}");

            foreach (var header in Headers)
            {
                ret.Add($"{header.Name.Pastel(RequestColours.HeaderName)}{":".Pastel(RequestColours.Normal)} {header.Value.Pastel(RequestColours.HeaderValue)}");
            }

            ret.Add("");

            if (string.IsNullOrWhiteSpace(Body) && Files.Any())
            {
                var file = Files.First();
                ret.Add($"[File content of {file.Data.Length} bytes]".Pastel(Color.DimGray));
            }
            else
            {
                if (Headers.Any(x => x.Name.ToLower() == "content-type" && x.Value.Contains("application/json")))
                {
                    ret.Add(ColourisePrettyJson(Body));
                }
                else
                {
                    ret.Add(Body);
                }
            }

            ret.Add("");

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
                            return match.Value.Pastel(RequestColours.JsonPropertyName);

                        case "string":
                            return match.Value.Pastel(RequestColours.JsonPropertyValueString);

                        default:
                        case "number":
                        case "boolean":
                        case "null":
                            return match.Value.Pastel(RequestColours.JsonPropertyValueStrut);
                    }
                });
        }
    }
}