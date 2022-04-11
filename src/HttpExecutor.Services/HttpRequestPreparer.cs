using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class HttpRequestPreparer : IHttpRequestPreparer
    {
        private readonly IAppOptions _options;

        public HttpRequestPreparer(IAppOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public HttpRequestMessage BuildRequest(IHttpRequest request, HttpClient client)
        {
            var httpRequestMessage = new HttpRequestMessage();
            var contentHeaders = new List<IHttpHeader>();

            httpRequestMessage.Method = MapMethod(request.Verb);

            // Request url should not start with /
            var resolvedUrl = request.Path;

            httpRequestMessage.RequestUri = new Uri(resolvedUrl.TrimStart('/'), UriKind.Relative);

            var specifiesUserAgent = false;

            foreach (var header in request.Headers)
            {
                if (header.Name.Equals("host", StringComparison.InvariantCultureIgnoreCase))
                {
                    client.BaseAddress = new Uri($"{request.Scheme}{header.Value}/");
                }

                if (header.Name.Equals("user-agent", StringComparison.InvariantCultureIgnoreCase))
                {
                    specifiesUserAgent = true;
                }

                if (header.Name.StartsWith("Content", StringComparison.InvariantCultureIgnoreCase))
                {
                    contentHeaders.Add(header);
                }
                else
                {
                    httpRequestMessage.Headers.Add(header.Name, header.Value);
                }
            }

            if (!specifiesUserAgent && _options.AddUserAgent && !string.IsNullOrWhiteSpace(_options.UserAgent))
            {
                httpRequestMessage.Headers.Add("User-Agent", _options.UserAgent);
            }

            var hasBody = false;

            if (request.Files.Any())
            {
                var file = request.Files.First();
                httpRequestMessage.Content = new ByteArrayContent(file.Data);
                hasBody = true;
            }
            else if (!string.IsNullOrWhiteSpace(request.Body))
            {
                httpRequestMessage.Content = new StringContent(request.Body.TrimEnd());
                hasBody = true;
            }

            if (hasBody)
            {
                foreach (var header in contentHeaders)
                {
                    switch (header.Name.Trim().ToLower())
                    {
                        case "content-type":
                            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(header.Value.Trim());
                            break;

                        default:
                            httpRequestMessage.Content.Headers.Add(header.Name.Trim(), header.Value.Trim());
                            break;
                    }
                }
            }

            return httpRequestMessage;
        }

        private HttpMethod MapMethod(string verb)
        {
            switch (verb)
            {
                case "GET":
                    return HttpMethod.Get;
                case "PUT":
                    return HttpMethod.Put;
                case "POST":
                    return HttpMethod.Post;
                case "DELETE":
                    return HttpMethod.Delete;
                case "PATCH":
                    return HttpMethod.Patch;
                case "OPTIONS":
                    return HttpMethod.Options;
                case "HEAD":
                    return HttpMethod.Head;
                case "TRACE":
                    return HttpMethod.Trace;
            }

            throw new ArgumentOutOfRangeException(nameof(verb), $"'{verb}' not matched to HTTP verb.");
        }
    }
}
