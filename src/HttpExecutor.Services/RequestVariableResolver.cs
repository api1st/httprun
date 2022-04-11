using System;
using System.Linq;
using System.Text;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestVariableResolver : IRequestVariableResolver
    {
        private readonly IVariableProvider _variableProvider;
        private readonly IAuthorisationHeaderResolver _authorisationHeaderResolver;

        public RequestVariableResolver(IVariableProvider variableProvider, IAuthorisationHeaderResolver authorisationHeaderResolver)
        {
            _variableProvider = variableProvider ?? throw new ArgumentNullException(nameof(variableProvider));
            _authorisationHeaderResolver = authorisationHeaderResolver ?? throw new ArgumentNullException(nameof(authorisationHeaderResolver));
        }

        public IHttpRequest ResolveVariables(IHttpRequest request)
        {
            // Fix file level variables
            _variableProvider.FixFileVariables();

            var resolved = new HttpRequest();
            resolved.Verb = request.Verb;
            resolved.Path = _variableProvider.Resolve(request.Path);
            resolved.Name = request.Name;
            resolved.IsMultipartFormData = request.IsMultipartFormData;
            resolved.RequiresConfirmation = request.RequiresConfirmation;

            // Firstly try to process the Host
            var hostResolved = _variableProvider.Resolve(request.Host);

            if (string.IsNullOrWhiteSpace(request.Scheme))
            {
                resolved.Scheme = hostResolved.EndsWith(":443") ? "https://" : "http://";
            }
            else
            {
                resolved.Scheme = request.Scheme;
            }

            // As we were unable to add the host information in the request builder (variables might need resolving)
            // Check we have at a minimum, a host header.
            if (!request.Headers.Any(x => x.Name.Equals("host", StringComparison.InvariantCultureIgnoreCase)))
            {
                request.Headers.Add(new HttpHeader { Name = "Host", Value = hostResolved });
            }

            foreach (var header in request.Headers)
            {
                var headerName = _variableProvider.Resolve(header.Name);
                var headerValue = _variableProvider.Resolve(header.Value);

                if (headerName.Equals("authorization", StringComparison.InvariantCultureIgnoreCase))
                {
                    var requestUri = request.Path;
                    var hostHeader = request.Headers.FirstOrDefault(x => x.Name.Equals("host", StringComparison.InvariantCultureIgnoreCase));

                    if (hostHeader != null)
                    {
                        requestUri = "//" + hostResolved + "/" + resolved.Path;
                    }

                    var adjustedAuthValue = _authorisationHeaderResolver.ProcessAuth(headerValue, request.Verb, requestUri);

                    resolved.Headers.Add(new HttpHeader { Name = headerName, Value = adjustedAuthValue });
                }
                else
                {
                    resolved.Headers.Add(new HttpHeader { Name = headerName, Value = headerValue });
                }
            }

            if (request.Files.Any())
            {
                foreach (var file in request.Files)
                {
                    // variables within the file may need to be resolved
                    if (file.ResolveVariables)
                    {
                        var resolvedFile = new RequestBodyFile();

                        var originalFileContent = LoadStringWithEncoding(file.Data, file.Encoding);

                        var resolvedFileContent = _variableProvider.Resolve(originalFileContent);

                        resolvedFile.Data = Encoding.UTF8.GetBytes(resolvedFileContent);
                        
                        resolved.Files.Add(resolvedFile);
                    }
                    else
                    {
                        resolved.Files.Add(file);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.Body))
            {
                resolved.Body = _variableProvider.Resolve(request.Body);
            }

            return resolved;
        }

        private string LoadStringWithEncoding(byte[] data, string encoding)
        {
            if (string.IsNullOrWhiteSpace(encoding))
            {
                // use the default utf8
                return Encoding.UTF8.GetString(data);
            }

            foreach (var encodingOption in Encoding.GetEncodings())
            {
                if (encodingOption.Name == encoding)
                {
                    return encodingOption.GetEncoding().GetString(data);
                }
            }

            return null;
        }
    }
}