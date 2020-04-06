using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using HttpTracing.Tests.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HttpTracing.Tests
{
    [Collection(ServerCollection.Name)]
    public class HttpTracingServiceCollectionExtensionsTests
    {
        private readonly ServerFixture _fixture;
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();

        public HttpTracingServiceCollectionExtensionsTests(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ItShouldAddHttpTracingToAllHttpClients()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(true);

            var services = new ServiceCollection();
            services
                .AddLogging(builder =>
                {
                    builder
                        .AddProvider(new MockLoggerProvider(_loggerMock.Object))
                        .AddFilter(_ => true);
                })
                .AddHttpTracingToAllHttpClients()
                .AddHttpClient<TypeTestClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = _fixture.ServerUri;
                });

            var client = services.BuildServiceProvider().GetRequiredService<TypeTestClient>();

            var response = await client.Client.GetAsync($"{ApiController.JsonUri}?name=MyName");
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be("MyName");

            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 200), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 210), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
        }

        [Fact]
        public async Task ItShouldAddHttpTracingToAllHttpClientsWithCustomConfiguration()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(true);

            var services = new ServiceCollection();
            services
                .AddLogging(builder =>
                {
                    builder
                        .AddProvider(new MockLoggerProvider(_loggerMock.Object))
                        .AddFilter(_ => true);
                })
                .AddHttpTracingToAllHttpClients((sp, builder) =>
                {
                    return builder.Name switch
                    {
                        nameof(TypeTestClient) => new HttpMessageHandlerTracingConfiguration { Enabled = false },
                        _ => null,
                    };
                })
                .AddHttpClient<TypeTestClient>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = _fixture.ServerUri;
                });

            var client = services.BuildServiceProvider().GetRequiredService<TypeTestClient>();

            var response = await client.Client.GetAsync($"{ApiController.JsonUri}?name=MyName");
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be("MyName");

            _loggerMock.Verify(
                x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 200), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()),
                Times.Never());
            _loggerMock.Verify(
                x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 210), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()),
                Times.Never());
        }
    }
}
