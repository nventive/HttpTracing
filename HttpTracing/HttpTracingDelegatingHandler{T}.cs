using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace HttpTracing
{
    /// <summary>
    /// <see cref="DelegatingHandler"/> that allows complete tracing of HttpClient request / responses.
    /// </summary>
    /// <typeparam name="T">The type name to use for the logger category. Similar to the category names used by HttpClient.</typeparam>
    public class HttpTracingDelegatingHandler<T> : HttpTracingDelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTracingDelegatingHandler{T}"/> class.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="isResponseSuccessful">
        /// A function to allow customization of the evaluation of a successful response.
        /// Defaults to <see cref="HttpResponseMessage.IsSuccessStatusCode"/>.
        /// </param>
        public HttpTracingDelegatingHandler(
            ILoggerFactory loggerFactory,
            Func<HttpResponseMessage, bool> isResponseSuccessful = null)
            : base(loggerFactory?.CreateLogger(LoggerCategory<T>()) ?? throw new ArgumentNullException(nameof(loggerFactory)), isResponseSuccessful)
        {
        }
    }
}
