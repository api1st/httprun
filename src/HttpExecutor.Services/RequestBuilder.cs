using System;
using System.Globalization;
using System.IO;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestBuilder : IRequestBuilder
    {
        private readonly IRequestVariableProvider _requestVariableProvider;
        private readonly IRequestBodyFileParser _requestBodyFileParser;
        private readonly IAppOptions _options;
        private IHttpRequest _current;

        public RequestBuilder(IRequestVariableProvider requestVariableProvider, IRequestBodyFileParser requestBodyFileParser, IAppOptions options)
        {
            _requestVariableProvider = requestVariableProvider ?? throw new ArgumentNullException(nameof(requestVariableProvider));
            _requestBodyFileParser = requestBodyFileParser ?? throw new ArgumentNullException(nameof(requestBodyFileParser));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _current = new HttpRequest();
        }

        public void AddLine(IBlockLine line)
        {
            var request = (HttpRequest) _current;

            switch (line.LineType)
            {
                case LineType.RequestVerb:
                    var verbLine = (RequestVerbLine) line;
                    request.Verb = verbLine.Verb;
                    request.Path = verbLine.Path;
                    request.Scheme = verbLine.Scheme;
                    request.Host = verbLine.Host;
                
                    break;
                    
                case LineType.RequestHeader:
                    var headerLine = (RequestHeaderLine) line;
                    request.Headers.Add(new HttpHeader{ Name = headerLine.HeaderName, Value = headerLine.HeaderValue });

                    // Form MultiPart is a special case
                    if (request.Verb.StartsWith("P", true, CultureInfo.InvariantCulture))
                    {
                        // Is a Post, Put or Patch
                        // Note: this won't detect if there are unresolved variables in the header name or value.
                        if (headerLine.HeaderName.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase) && headerLine.HeaderValue.Contains("multipart/form-data"))
                        {
                            // Is a Multipart post
                            request.IsMultipartFormData = true;
                        }
                    }

                    if (headerLine.HeaderName.Equals("Host", StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.Host = headerLine.HeaderValue;
                    }

                    break;

                case LineType.RequestBody:
                    // Test for File input fields
                    var fileBody = _requestBodyFileParser.IsFileInput(line.Raw);
                    if (!request.IsMultipartFormData && fileBody.isFile)
                    {
                        // Only process files if not form-multipart
                        if (fileBody.exists)
                        {
                            request.Files.Add(_requestBodyFileParser.FileContent(line.Raw));
                        }
                        else
                        {
                            // Warn that the file isn't found.
                            if (_options.TerminateOnFileAccessFailure)
                            {
                                throw new Exception($"Could not find the file specified '{line.Raw}'");
                            }
                            else
                            {
                                var bodyLine = (RequestBodyLine)line;

                                // Only append lines if not the first line of data.
                                if (!string.IsNullOrWhiteSpace(request.Body))
                                {
                                    request.Body += Environment.NewLine;
                                }

                                request.Body += bodyLine.Raw;
                            }
                        }
                    }
                    else
                    {
                        var bodyLine = (RequestBodyLine)line;
                        
                        // Only append lines if not the first line of data.
                        if (!string.IsNullOrWhiteSpace(request.Body))
                        {
                            request.Body += Environment.NewLine;
                        }

                        request.Body += bodyLine.Raw;
                    }

                    break;
            }
        }

        public void SetName(string name)
        {
            ((HttpRequest) _current).Name = name;
        }

        public void RequireConfirmation()
        {
            ((HttpRequest) _current).RequiresConfirmation = true;
        }

        public IHttpRequest Build()
        {
            var toReturn = _current;
            _current = new HttpRequest();

            if (!string.IsNullOrWhiteSpace(toReturn.Name))
            {
                _requestVariableProvider.RegisterRequest(toReturn.Name, toReturn);
            }
            
            return toReturn;
        }
    }
}