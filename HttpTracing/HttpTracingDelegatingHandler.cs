using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HttpTracing
{
    /// <summary>
    /// <see cref="DelegatingHandler"/> that allows complete tracing of HttpClient request / responses.
    /// Please be aware of the performance / memory implications of doing full request/response tracing.
    /// </summary>
    public class HttpTracingDelegatingHandler : DelegatingHandler
    {
        /// <summary>
        /// Gets the prefix for the default logger category (System.Net.Http.HttpClient).
        /// </summary>
        public static readonly string LogCategoryPrefix = "System.Net.Http.HttpClient";

        /// <summary>
        /// Gets the suffix for the default logger category (TraceHandler).
        /// </summary>
        public static readonly string LogCategorySuffix = "TraceHandler";

        private readonly ILogger _logger;
        private readonly Func<HttpResponseMessage, bool> _isResponseSuccessful;
        private readonly bool _bufferRequests;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTracingDelegatingHandler"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
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
        public HttpTracingDelegatingHandler(
            ILogger logger,
            Func<HttpResponseMessage, bool> isResponseSuccessful = null,
            bool bufferRequests = false)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isResponseSuccessful = isResponseSuccessful ?? ((HttpResponseMessage response) => response.IsSuccessStatusCode);
            _bufferRequests = bufferRequests;
        }

        /// <summary>
        /// Creates a <see cref="ILogger"/> category name based on <see cref="LogCategoryPrefix"/>.<paramref name="name"/>.<see cref="LogCategorySuffix"/>.
        /// </summary>
        /// <param name="name">The name associated with the <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="ILogger"/> category.</returns>
        public static string LoggerCategory(string name) => $"{LogCategoryPrefix}.{name}.{LogCategorySuffix}";

        /// <summary>
        /// Creates a <see cref="ILogger"/> category name based on <see cref="LogCategoryPrefix"/>.<typeparamref name="T"/>.<see cref="LogCategorySuffix"/>.
        /// </summary>
        /// <typeparam name="T">The type to infer the name from.</typeparam>
        /// <returns>The <see cref="ILogger"/> category.</returns>
        public static string LoggerCategory<T>() => LoggerCategory(typeof(T).Name);

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                if (_bufferRequests && request.Content != null)
                {
                    var newRequestContent = new StreamContent(
                        await request.Content.ReadAsStreamAsync().ConfigureAwait(false));
                    foreach (var requestHeader in request.Content.Headers)
                    {
                        newRequestContent.Headers.Add(requestHeader.Key, requestHeader.Value);
                    }

                    request.Content = newRequestContent;
                }

                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                var isSuccessfull = _isResponseSuccessful(response);

                if (isSuccessfull && _logger.IsEnabled(LogLevel.Trace))
                {
                    await _logger.RequestSuccessful(request).ConfigureAwait(false);
                    await _logger.ResponseSuccessful(response).ConfigureAwait(false);
                }

                if (!isSuccessfull)
                {
                    await _logger.RequestError(request).ConfigureAwait(false);
                    await _logger.ResponseError(response).ConfigureAwait(false);
                }

                return response;
            }
            catch (Exception ex)
            {
                await _logger.RequestError(request, ex).ConfigureAwait(false);
                throw;
            }
        }
    }
}
