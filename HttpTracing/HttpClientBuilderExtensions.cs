using System;
using System.Net.Http;
using HttpTracing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IHttpClientBuilder"/> extension methods.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds and register <see cref="HttpTracingDelegatingHandler{T}"/> to enable tracing of requests/responses.
        /// </summary>
        /// <typeparam name="TClient">
        /// The type of the typed client.
        /// Will determine the category of the logger (System.Net.Http.HttpClient.T.TraceHandler).
        /// </typeparam>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <param name="isResponseSuccessful">
        /// A function to allow customization of the evaluation of a successful response.
        /// Defaults to <see cref="HttpResponseMessage.IsSuccessStatusCode"/>.
        /// </param>
        /// <returns>The updated <see cref="IHttpClientBuilder"/>.</returns>
        public static IHttpClientBuilder AddHttpTracing<TClient>(this IHttpClientBuilder builder, Func<HttpResponseMessage, bool> isResponseSuccessful = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddTransient(sp => new HttpTracingDelegatingHandler<TClient>(sp.GetRequiredService<ILoggerFactory>(), isResponseSuccessful));

            return builder.AddHttpMessageHandler<HttpTracingDelegatingHandler<TClient>>();
        }
    }
}
