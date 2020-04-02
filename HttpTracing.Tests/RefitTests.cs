using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using HttpTracing.Tests.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Refit;
using Xunit;

namespace HttpTracing.Tests
{
    [Collection(ServerCollection.Name)]
    public class RefitTests
    {
        private readonly ServerFixture _fixture;
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();

        public RefitTests(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ItShouldLogWhenTracingEnabled()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(true);
            var client = CreateRefitClient();

            var result = await client.GetJson("MyName");
            result.Name.Should().Be("MyName");

            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 200), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 210), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
        }

        [Fact]
        public async Task ItShouldLogWhenTracingEnabledOnPost()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(true);
            var client = CreateRefitClient();
            var inputModel = new SampleModel
            {
                Name = "TheName",
            };

            var result = await client.PostJson(inputModel);
            result.Should().BeEquivalentTo(inputModel);

            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 200), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 210), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
        }

        [Fact]
        public async Task ItShouldNotLogWhenTracingDisabledAndNoErrors()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(false)
                .Verifiable();
            var client = CreateRefitClient();

            var result = await client.GetJson();
            result.Name.Should().Be(SampleModel.DefaultName);

            _loggerMock.Verify();
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ItShouldLogBinary()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(true);
            var client = CreateRefitClient();

            var response = await client.GetBinary();
            var result = await response.Content.ReadAsByteArrayAsync();
            result.Should().BeEquivalentTo(await File.ReadAllBytesAsync(typeof(ApiController).Assembly.Location));

            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 200), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 210), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
        }

        [Fact]
        public async Task ItShouldLogWhenThereIsAnError()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning))
                .Returns(true);
            var client = CreateRefitClient();

            var response = await client.GetStatus(500);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            _loggerMock.Verify(x => x.Log(LogLevel.Warning, It.Is<EventId>(e => e.Id == 201), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
            _loggerMock.Verify(x => x.Log(LogLevel.Warning, It.Is<EventId>(e => e.Id == 211), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
        }

        private IRefitClient CreateRefitClient()
        {
            var services = new ServiceCollection();
            services
                .AddHttpClient<IRefitClient>(options =>
                {
                    options.BaseAddress = _fixture.ServerUri;
                })
                .AddHttpTracing(
                    logger: _loggerMock.Object,
                    bufferRequests: true);

            services.AddTransient(
                sp => RestService.For<IRefitClient>(
                    client: sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(IRefitClient)),
                    settings: new RefitSettings
                    {
                        ContentSerializer = new SystemTextJsonContentSerializer(
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            }),
                    }));

            return services.BuildServiceProvider().GetRequiredService<IRefitClient>();
        }
}
}
