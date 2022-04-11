using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class MultipartHttpRequestPreparer : IMultipartHttpRequestPreparer
    {
        private readonly IVariableProvider _variableProvider;
        private readonly IAppOptions _options;

        public MultipartHttpRequestPreparer(IVariableProvider variableProvider, IAppOptions options)
        {
            _variableProvider = variableProvider ?? throw new ArgumentNullException(nameof(variableProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public HttpRequestMessage BuildMultipartFormDataRequest(IHttpRequest request, HttpClient client)
        {
            var httpRequestMessage = new HttpRequestMessage();
            var contentHeaders = new List<IHttpHeader>();

            httpRequestMessage.Method = MapMethod(request.Verb);

            // Request url should not start with /
            var resolvedUrl = request.Path;

            var boundary = string.Empty;

            httpRequestMessage.RequestUri = new Uri(resolvedUrl.TrimStart('/'), UriKind.Relative);

            foreach (var header in request.Headers)
            {
                if (header.Name.Equals("host", StringComparison.InvariantCultureIgnoreCase))
                {
                    client.BaseAddress = new Uri($"{request.Scheme}{header.Value}/");
                }

                if (header.Name.StartsWith("Content", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (header.Name.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Needs to have the boundary in it.
                        var boundaryParser = new RequestContentTypeMultipartMimeParser();
                        boundary = boundaryParser.GetBoundary(header.Value);
                    }

                    contentHeaders.Add(header);
                }
                else
                {
                    httpRequestMessage.Headers.Add(header.Name, header.Value);
                }
            }

            var multipartData = DeserialiseBody(request.Body, boundary);

            var multipartFormDataContent = new MultipartFormDataContent(boundary);

            foreach (var part in multipartData)
            {
                HttpContent content;

                if (part.File != null)
                {
                    if (part.File.ResolveVariables)
                    {
                        var data = Encoding.UTF8.GetString(part.File.Data);
                        var resolvedData = _variableProvider.Resolve(data);
                        content = new ByteArrayContent(Encoding.UTF8.GetBytes(resolvedData));
                    }
                    else
                    {
                        content = new ByteArrayContent(part.File.Data);
                    }
                }
                else
                {
                    content = new StringContent(part.Value);
                }

                foreach (var header in part.Headers)
                {
                    // Ensure we don't add any headers that may already have been set (i.e. StringContent sets "Content-Type")
                    if (!content.Headers.Contains(header.Name))
                    {
                        content.Headers.Add(header.Name, header.Value);
                    }
                    else
                    {
                        // TODO - Add a warning to state that the user specified header has not been set.
                    }
                }

                multipartFormDataContent.Add(content);
            }

            httpRequestMessage.Content = multipartFormDataContent;

            foreach (var header in contentHeaders)
            {
                switch (header.Name.ToLower())
                {
                    case "content-type":
                        // Mustn't add this header, it is added automatically with the boundary.
                        break;

                    default:
                        if (!httpRequestMessage.Content.Headers.Contains(header.Name))
                        {
                            httpRequestMessage.Content.Headers.Add(header.Name, header.Value);
                        }
                        else
                        {
                            // TODO - Add a warning to state that the user specified header has not been set.
                        }
                        break;
                }
            }

            return httpRequestMessage;
        }

        private ICollection<MultipartData> DeserialiseBody(string body, string boundary)
        {
            var lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var parts = new List<MultipartData>();

            var fileParser = new RequestBodyFileParser();

            var current = new MultipartData();
            var contentLines = new StringBuilder();
            var headers = true;

            foreach (var line in lines)
            {
                if (line == $"--{boundary}" || line == $"--{boundary}--")
                {
                    // Only add part if there is any content
                    if (contentLines.Length > 0 || current.File != null)
                    {
                        current.Value = contentLines.ToString();
                        parts.Add(current);
                    }

                    if (line == $"--{boundary}--")
                    {
                        // End of content
                        break;
                    }

                    current = new MultipartData();
                    contentLines = new StringBuilder();
                    headers = true;
                    continue;
                }

                if (headers && string.IsNullOrWhiteSpace(line))
                {
                    // Header/content boundary
                    headers = false;
                    continue;
                }

                if (headers)
                {
                    var header = new RequestHeaderLine(line, null, 0); // Line number doesn't matter here
                    current.Headers.Add(new HttpHeader { Name = header.HeaderName, Value = header.HeaderValue });
                }
                else
                {
                    var (isFile, exists) = fileParser.IsFileInput(line);
                    if (string.IsNullOrWhiteSpace(current.Value) && isFile)
                    {
                        if (exists)
                        {
                            current.File = fileParser.FileContent(line);
                        }
                        else
                        {
                            // Looks very much like a file, but that file doesn't exist
                            if (_options.TerminateOnFileAccessFailure)
                            {
                                throw new Exception($"Could not find the file specified '{line}'");
                            }
                            else
                            {
                                if (contentLines.Length > 0)
                                {
                                    contentLines.AppendLine();
                                }

                                contentLines.Append(line);
                            }
                        }
                    }
                    else
                    {
                        if (contentLines.Length > 0)
                        {
                            contentLines.AppendLine();
                        }

                        contentLines.Append(line);
                    }
                }
            }

            return parts;
        }

        public class MultipartData
        {
            public MultipartData()
            {
                Headers = new List<IHttpHeader>();
            }

            public ICollection<IHttpHeader> Headers { get; }

            public string Value { get; set; }

            public IRequestBodyFile File { get; set; }
        }

        private static HttpMethod MapMethod(string verb)
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