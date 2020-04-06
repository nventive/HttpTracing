using System;
using System.Net.Http;

namespace HttpTracing
{
    /// <summary>
    /// Custom configuration for a specific <see cref="HttpClient"/>.
    /// </summary>
    public class HttpMessageHandlerTracingConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether to enable tracing for the <see cref="HttpClient"/>.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the category name to use for logging.
        /// Defaults to "System.Net.Http.HttpClient.{HttpClient.Name}.TraceHandler".
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets a function to allow customization of the evaluation of a successful response.
        /// Defaults to <see cref="HttpResponseMessage.IsSuccessStatusCode"/>.
        /// </summary>
        public Func<HttpResponseMessage, bool> IsResponseSuccessful { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to actively buffer the requests bodies.
        /// This is to be used when you want to see the content of requests
        /// when using serializer that are forward-only.
        /// This will impact performance and memory consumption, but is probably fine if you are
        /// in a typical run-of-the-mill scenario.
        /// </summary>
        public bool BufferRequests { get; set; }
    }
}
