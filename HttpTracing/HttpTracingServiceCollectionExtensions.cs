using System;
using System.Net.Http;
using HttpTracing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class HttpTracingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="HttpTracingDelegatingHandler"/> to enable tracing of requests/responses
        /// to ALL <see cref="HttpClient"/> created by the <see cref="IHttpClientFactory"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">
        /// Custom configuration factory per builder.
        /// If you return null for any value, the default is applied.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddHttpTracingToAllHttpClients(
            this IServiceCollection services,
            Func<IServiceProvider, HttpMessageHandlerBuilder, HttpMessageHandlerTracingConfiguration> configuration = null)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, TracingHttpMessageHandlerBuilderFilter>(
                    sp => new TracingHttpMessageHandlerBuilderFilter(
                        sp,
                        sp.GetRequiredService<ILoggerFactory>(),
                        configuration)));
            return services;
        }
    }
}
