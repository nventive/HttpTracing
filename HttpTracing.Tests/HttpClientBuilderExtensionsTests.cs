using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HttpTracing.Tests
{
    public class HttpClientBuilderExtensionsTests
    {
        [Fact]
        public void ItShouldRegisterWithDefaultName()
        {
            var httpClientName = "TheHttpClientName";
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddHttpClient(httpClientName)
                .AddHttpTracing();

            var provider = services.BuildServiceProvider();

            Action act = () => provider.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName);
            act.Should().NotThrow();
        }

        [Fact]
        public void ItShouldRegisterTypeClientWithDefaultName()
        {
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddHttpClient<TypeTestClient>()
                .AddHttpTracing();

            var provider = services.BuildServiceProvider();

            Action act = () => provider.GetRequiredService<TypeTestClient>();
            act.Should().NotThrow();
        }

        [SuppressMessage("Microsoft.Performance", "CA1812: Avoid uninstantiated internal classes", Justification = "Test Registration Only")]
        private class TypeTestClient
        {
            public TypeTestClient(
                HttpClient client)
            {
                Client = client;
            }

            public HttpClient Client { get; }
        }
    }
}
