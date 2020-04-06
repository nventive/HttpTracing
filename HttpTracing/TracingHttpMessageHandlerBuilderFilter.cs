using System;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HttpTracing
{
    /// <summary>
    /// <see cref="IHttpMessageHandlerBuilderFilter"/> that adds <see cref="HttpTracingDelegatingHandler"/>
    /// to all <see cref="HttpClient"/>.
    /// </summary>
    public class TracingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Func<IServiceProvider, HttpMessageHandlerBuilder, HttpMessageHandlerTracingConfiguration> _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TracingHttpMessageHandlerBuilderFilter"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="configuration">Custom configuration factory per builder.</param>
        public TracingHttpMessageHandlerBuilderFilter(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            Func<IServiceProvider, HttpMessageHandlerBuilder, HttpMessageHandlerTracingConfiguration> configuration = null)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return (builder) =>
            {
                // Run other configuration first, we want to decorate.
                next(builder);

                var config = _configuration?.Invoke(_serviceProvider, builder) ?? new HttpMessageHandlerTracingConfiguration();

                if (config.Enabled)
                {
                    builder.AdditionalHandlers.Add(
                        new HttpTracingDelegatingHandler(
                            _loggerFactory.CreateLogger(config.CategoryName ?? HttpTracingDelegatingHandler.LoggerCategory(builder.Name)),
                            isResponseSuccessful: config.IsResponseSuccessful,
                            bufferRequests: config.BufferRequests));
                }
            };
        }
    }
}
