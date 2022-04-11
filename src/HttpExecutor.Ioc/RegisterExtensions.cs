using System.Net.Http;
using System.Security.Authentication;
using HttpExecutor.Abstractions;
using HttpExecutor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HttpExecutor.Ioc
{
    public static class RegisterExtensions
    {
        public static IServiceCollection AddExecutorServices(this IServiceCollection services)
        {
            services.AddTransient<IParser, Parser>();
            services.AddTransient<ILineParser, LineParser>();
            services.AddTransient<ILineTypeDecoder, LineTypeDecoder>();
            services.AddTransient<IRequestVerbLineDecoder, RequestVerbLineDecoder>();
            services.AddTransient<IBlockExecutor, BlockExecutor>();
            services.AddTransient<IRequestBuilder, RequestBuilder>();
            services.AddTransient<IRequestSender, RequestSender>();

            services.AddScoped<IVariableProvider, VariableProvider>();
            services.AddScoped<IRequestVariableProvider, RequestVariableProvider>();
            services.AddScoped<IRequestVariableResolver, RequestVariableResolver>();
            services.AddTransient<IAuthorisationHeaderResolver, AuthorisationHeaderResolver>();

            services.AddTransient<IConfirmationReader, ConfirmationReader>();

            services.AddTransient<IDatejsToDotNetFormatConverter, DateJsToDotNetFormatConverter>();
            services.AddTransient<IRequestBodyFileParser, RequestBodyFileParser>();

            services.AddTransient<IMultipartHttpRequestPreparer, MultipartHttpRequestPreparer>();
            services.AddTransient<IHttpRequestPreparer, HttpRequestPreparer>();

            // Dynamic Resolvers
            services.AddTransient<IDynamicVariableResolver, GuidVariableResolver>();
            services.AddTransient<IDynamicVariableResolver, RandomIntVariableResolver>();
            services.AddTransient<IDynamicVariableResolver, EnvironmentVariableResolver>();
            services.AddTransient<IDynamicVariableResolver, TimeStampVariableResolver>();
            services.AddTransient<IDynamicVariableResolver, DateTimeVariableResolver>();
            services.AddTransient<IDynamicVariableResolver, LocalDateTimeVariableResolver>();

            services.AddHttpClient("follow-redirect").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
                };
            });

            services.AddHttpClient("no-follow-redirect").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
                };
            });
            
            return services;
        }
    }
}
