using System;
using System.Net.Http;
using System.Threading.Tasks;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestSender : IRequestSender
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRequestVariableProvider _requestVariableProvider;
        private readonly IAppOptions _options;
        private readonly IHttpRequestPreparer _requestPreparer;
        private readonly IMultipartHttpRequestPreparer _multipartHttpRequestPreparer;

        public RequestSender(IHttpClientFactory httpClientFactory, IRequestVariableProvider requestVariableProvider, IAppOptions options, IHttpRequestPreparer requestPreparer, IMultipartHttpRequestPreparer multipartHttpRequestPreparer)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _requestVariableProvider = requestVariableProvider ?? throw new ArgumentNullException(nameof(requestVariableProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _requestPreparer = requestPreparer ?? throw new ArgumentNullException(nameof(requestPreparer));
            _multipartHttpRequestPreparer = multipartHttpRequestPreparer ?? throw new ArgumentNullException(nameof(multipartHttpRequestPreparer));
        }

        public async Task<(IHttpRequest, IHttpResponse)> SendAsync(IHttpRequest request)
        {
            HttpClient client;
            
            if (_options.Follow300Responses)
            {
                client = _httpClientFactory.CreateClient("follow-redirect");
            }
            else
            {
                client = _httpClientFactory.CreateClient("no-follow-redirect");
            }

            HttpRequestMessage httpRequestMessage;

            if (request.IsMultipartFormData)
            {
                httpRequestMessage = _multipartHttpRequestPreparer.BuildMultipartFormDataRequest(request, client);
            }
            else
            {
                httpRequestMessage = _requestPreparer.BuildRequest(request, client);
            }

            HttpResponseMessage httpResponseMessage;
            try
            {
                client.Timeout = TimeSpan.FromMilliseconds(_options.RequestTimeout);
                httpResponseMessage = await client.SendAsync(httpRequestMessage);
            }
            catch (TaskCanceledException)
            {
                return (request, new FailedHttpResponse { Body = "HTTP Request timeout." });
            }
            catch (HttpRequestException hre)
            {
                return (request, new FailedHttpResponse { Body = hre.Message });
            }

            // Potential for special case here with redirects.
            // Dotnet core does not support automatic following of redirects from https -> http endpoints.
            // If the response is a 3xx status, and the auto follow is turned on, then display a warning message.
            if (_options.Follow300Responses && ((int)httpResponseMessage.StatusCode >= 300 &&
                                                (int)httpResponseMessage.StatusCode < 400))
            {
                // Response didn't get redirected as expected
                return (request, new FailedHttpResponse { Body = "Following of HTTP Redirect failed, possibly due to https->http redirection." });
            }

            var httpResponse = new HttpResponse();

            httpResponse.StatusCode = (int)httpResponseMessage.StatusCode;
            httpResponse.StatusPhrase = httpResponseMessage.ReasonPhrase;

            foreach (var responseHeader in httpResponseMessage.Headers)
            {
                httpResponse.Headers.Add(new HttpHeader { Name = responseHeader.Key, Value = string.Join(", ", responseHeader.Value) });
            }

            httpResponse.Body = await httpResponseMessage.Content.ReadAsStringAsync();

            foreach (var contentHeader in httpResponseMessage.Content.Headers)
            {
                httpResponse.Headers.Add(new HttpHeader { Name = contentHeader.Key, Value = string.Join(", ", contentHeader.Value) });
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                _requestVariableProvider.RegisterRequest(request.Name, request);
                _requestVariableProvider.RegisterResponse(request.Name, httpResponse);
            }

            return (request, httpResponse);
        }
    }
}