using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HttpTracing
{
    /// <summary>
    /// Helper class that manages <see cref="ILogger"/> events for <see cref="HttpTracingDelegatingHandler"/>.
    /// </summary>
    public static class HttpTracingDelegatingHandlerLoggerExtensions
    {
        private const string RequestMessageFormat = @"
{RequestMethod} {RequestUri} {RequestHttpVersion}
{RequestHeaders}
{RequestBody}";

        private const string ResponseMessageFormat = @"
{ResponseHttpVersion} {ResponseStatusCode} {ResponseReason}
{ResponseHeaders}
{ResponseBody}";

        private static readonly Action<ILogger, HttpMethod, Uri, string, string, string, Exception> _requestSuccessful =
            LoggerMessage.Define<HttpMethod, Uri, string, string, string>(
                LogLevel.Trace,
                new EventId(200, "RequestSuccessful"),
                RequestMessageFormat);

        private static readonly Action<ILogger, HttpMethod, Uri, string, string, string, Exception> _requestError =
            LoggerMessage.Define<HttpMethod, Uri, string, string, string>(
                LogLevel.Warning,
                new EventId(201, "RequestError"),
                RequestMessageFormat);

        private static readonly Action<ILogger, string, int, string, string, string, Exception> _responseSuccessful =
            LoggerMessage.Define<string, int, string, string, string>(
                LogLevel.Trace,
                new EventId(210, "ResponseSuccessful"),
                ResponseMessageFormat);

        private static readonly Action<ILogger, string, int, string, string, string, Exception> _responseError =
            LoggerMessage.Define<string, int, string, string, string>(
                LogLevel.Warning,
                new EventId(211, "ResponseError"),
                ResponseMessageFormat);

        /// <summary>
        /// Traces the full <paramref name="request"/> when the interaction is sucessful.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RequestSuccessful(this ILogger logger, HttpRequestMessage request)
        {
            if (request == null)
            {
                return;
            }

            _requestSuccessful(
                logger,
                request.Method,
                request.RequestUri,
                $"HTTP/{request.Version}",
                request.AllHeadersAsString(),
                await SafeContentAsString(request.Content).ConfigureAwait(false),
                null);
        }

        /// <summary>
        /// Traces the full <paramref name="request"/> when the interaction is on error.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="ex">The <see cref="Exception"/> if any.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RequestError(this ILogger logger, HttpRequestMessage request, Exception ex = null)
        {
            if (request == null)
            {
                return;
            }

            _requestError(
                logger,
                request.Method,
                request.RequestUri,
                $"HTTP/{request.Version}",
                request.AllHeadersAsString(),
                await SafeContentAsString(request.Content).ConfigureAwait(false),
                ex);
        }

        /// <summary>
        /// Traces the full <paramref name="response"/> when the interaction is sucessful.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ResponseSuccessful(this ILogger logger, HttpResponseMessage response)
        {
            if (response == null)
            {
                return;
            }

            _responseSuccessful(
                logger,
                $"HTTP/{response.Version}",
                (int)response.StatusCode,
                response.ReasonPhrase,
                response.AllHeadersAsString(),
                response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync().ConfigureAwait(false),
                null);
        }

        /// <summary>
        /// Traces the full <paramref name="response"/> when the interaction is on error.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <param name="ex">The <see cref="Exception"/> if any.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ResponseError(this ILogger logger, HttpResponseMessage response, Exception ex = null)
        {
            if (response == null)
            {
                return;
            }

            _responseError(
                logger,
                $"HTTP/{response.Version}",
                (int)response.StatusCode,
                response.ReasonPhrase,
                response.AllHeadersAsString(),
                response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync().ConfigureAwait(false),
                ex);
        }

        private static async Task<string> SafeContentAsString(HttpContent content)
        {
            if (content is null)
            {
                return string.Empty;
            }

            try
            {
                return await content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                // This is thrown when the content stream has already been consumed.
                return $"[InvalidOperationException: Request content is not buffered ({ex.Message}). Use the bufferRequests parameter to allow the reading.]";
            }
        }
    }
}
