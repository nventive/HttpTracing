using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using HttpTracing.Tests.Server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HttpTracing.Tests
{
    [Collection(ServerCollection.Name)]
    public class HttpTracingDelegatingHandlerTests
    {
        private readonly ServerFixture _fixture;
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();

        public HttpTracingDelegatingHandlerTests(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ItShouldLogWhenTracingEnabled()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(true);
            var client = CreateHttpClient();

            var response = await client.GetAsync(ApiController.JsonUri);
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be(SampleModel.DefaultName);

            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 200), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
            _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.Is<EventId>(e => e.Id == 210), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
        }

        [Fact]
        public async Task ItShouldNotLogWhenTracingDisabledAndNoErrors()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(false)
                .Verifiable();
            var client = CreateHttpClient();

            var response = await client.GetAsync(ApiController.JsonUri);
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be(SampleModel.DefaultName);

            _loggerMock.Verify();
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ItShouldLogBinary()
        {
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace))
                .Returns(true);
            var client = CreateHttpClient();

            var response = await client.GetAsync(ApiController.BinaryUri);
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
            var client = CreateHttpClient();

            var response = await client.GetAsync($"{ApiController.StatusCodeUri}?statusCode=500");
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            _loggerMock.Verify(x => x.Log(LogLevel.Warning, It.Is<EventId>(e => e.Id == 201), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
            _loggerMock.Verify(x => x.Log(LogLevel.Warning, It.Is<EventId>(e => e.Id == 211), It.IsAny<object>(), null, It.IsAny<Func<object, Exception, string>>()));
        }

        private HttpClient CreateHttpClient()
            => new HttpClient(
                new HttpTracingDelegatingHandler(_loggerMock.Object) { InnerHandler = new HttpClientHandler() })
            {
                BaseAddress = _fixture.ServerUri,
            };
    }
}
