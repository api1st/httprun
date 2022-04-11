using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;
using Newtonsoft.Json.Linq;

namespace HttpExecutor.Services
{
    public class RequestVariableProvider : IRequestVariableProvider
    {
        private IDictionary<string, IHttpRequest> _requests = new Dictionary<string, IHttpRequest>();
        private IDictionary<string, IHttpResponse> _responses = new Dictionary<string, IHttpResponse>();

        public bool HasNamedRequest(string name)
        {
            return _requests.ContainsKey(name);
        }

        public string Resolve(string value)
        {
            var parts = Regex.Split(value, "\\.([^.]+?\\[\\?\\(.*\\)\\])\\.?|\\.");

            try
            {
                switch (parts[1])
                {
                    case "request":
                        return ResolveForRequest(string.Join(".", parts.Skip(2)), _requests[parts[0]]);

                    case "response":
                        return ResolveForResponse(string.Join(".", parts.Skip(2)), _responses[parts[0]]);
                }
            }
            catch 
            {
                return value;
            }

            return value;
        }

        private string ResolveForRequest(string pattern, IHttpRequest request)
        {
            var parts = Regex.Split(pattern, "\\.([^.]+?\\[\\?\\(.*\\)\\])\\.?|\\.");

            switch (parts[0])
            {
                case "body":
                    return ResolveForBody(string.Join(".", parts.Skip(1)), request.Body);
                    
                case "headers":
                    return ResolveForHeader(parts[1], request.Headers);

            }

            throw new ArgumentOutOfRangeException(nameof(pattern));
        }

        private string ResolveForResponse(string pattern, IHttpResponse response)
        {
            var parts = Regex.Split(pattern, "\\.([^.]+?\\[\\?\\(.*\\)\\])\\.?|\\.");

            switch (parts[0])
            {
                case "body":
                    return ResolveForBody(string.Join(".", parts.Skip(1)), response.Body);

                case "headers":
                    return ResolveForHeader(parts[1], response.Headers);
                    
            }

            throw new ArgumentOutOfRangeException(nameof(pattern));
        }

        private string ResolveForHeader(string name, ICollection<IHttpHeader> headers)
        {
            return headers.First(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        private string ResolveForBody(string path, string body)
        {
            JContainer jsonBody;

            // NOTE:
            // Newtonsoft can't cope with double quotes in the query.
            
            // May be a Json Object or a Json Array

            if (body.TrimStart()[0] == '[' )
            {
                // Is an array
                jsonBody = JArray.Parse(body);
            }
            else
            {
                jsonBody = JObject.Parse(body);
            }

            // jsonPath arrays "$.results[4].id" or "$.results.4.id", but the NewtonSoft JsonPath implementation doesn't work with non-bracketed ones.
            // However, we don't want to adjust any quoted string parts.
            var quotedComponents = Regex.Matches(path, "\\\\'|'(?:\\\\'|[^'])*'|(\\+)");
            var quotes = quotedComponents.Select(x => x.Value);

            var splittedPath = SplitByStrings(path, quotes);

            var switchRegex = new Regex("\\.(\\d+)\\.");

            var alteredPath = JoinWithStrings(splittedPath.Select(x => switchRegex.Replace(x, "[$1].")).ToArray(), quotes.ToArray());
            
            var token = jsonBody.SelectToken(alteredPath);

            if (token == null)
            {
                throw new ArgumentOutOfRangeException(nameof(path));
            }

            return token.ToString();
        }

        private string[] SplitByStrings(string value, IEnumerable<string> splits)
        {
            if (splits == null) return new[] {value};

            var res = new List<string>();

            foreach (var split in splits)
            {
                var index = value.IndexOf(split, StringComparison.InvariantCulture);

                var part = value.Substring(0, index);

                res.Add(part);

                value = value.Substring(index + split.Length);
            }

            res.Add(value);

            return res.ToArray();
        }

        private string JoinWithStrings(string[] values, string[] splits)
        {
            var ret = string.Empty;

            for (var i = 0; i < values.Count() -1; i++)
            {
                ret += values[i];
                ret += splits[i];
            }

            ret += values.Last();

            return ret;
        }

        public void RegisterRequest(string name, IHttpRequest request)
        {
            _requests[name] = request;
        }

        public void RegisterResponse(string name, IHttpResponse response)
        {
            _responses[name] = response;
        }
    }
}