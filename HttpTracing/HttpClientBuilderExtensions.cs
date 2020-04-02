using System;
using System.Net.Http;
using HttpTracing;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IHttpClientBuilder"/> extension methods.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="HttpTracingDelegatingHandler"/> to enable tracing of requests/responses.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <param name="categoryName">
        /// The category name to use for logging.
        /// Defaults to "System.Net.Http.HttpClient.{HttpClient.Name}.TraceHandler".</param>
        /// <param name="isResponseSuccessful">
        /// A function to allow customization of the evaluation of a successful response.
        /// Defaults to <see cref="HttpResponseMessage.IsSuccessStatusCode"/>.
        /// </param>
        /// <param name="bufferRequests">
        /// When set to true, will actively buffer the requests bodies.
        /// This is to be used when you want to see the content of requests
        /// when using serializer that are forward-only.
        /// This will impact performance and memory consumption, but is probably fine if you are
        /// in a typical run-of-the-mill scenario.
        /// </param>
        /// <param name="logger">
        /// Use a custom <see cref="ILogger"/> instead of the one provided by the <see cref="ILoggerFactory"/>.
        /// When used, the <paramref name="categoryName"/> is ineffective.
        /// </param>
        /// <returns>The updated <see cref="IHttpClientBuilder"/>.</returns>
        public static IHttpClientBuilder AddHttpTracing(
            this IHttpClientBuilder builder,
            string categoryName = null,
            Func<HttpResponseMessage, bool> isResponseSuccessful = null,
            bool bufferRequests = false,
            ILogger logger = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(categoryName))
            {
                categoryName = HttpTracingDelegatingHandler.LoggerCategory(builder.Name);
            }

            return builder.AddHttpMessageHandler(
                sp => new HttpTracingDelegatingHandler(
                    logger: logger ?? sp.GetRequiredService<ILoggerFactory>().CreateLogger(categoryName),
                    isResponseSuccessful: isResponseSuccessful,
                    bufferRequests: bufferRequests));
        }
    }
}
