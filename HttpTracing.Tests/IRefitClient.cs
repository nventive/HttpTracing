using System.Net.Http;
using System.Threading.Tasks;
using HttpTracing.Tests.Server;
using Refit;

namespace HttpTracing.Tests
{
    public interface IRefitClient
    {
        [Get("/json")]
        Task<SampleModel> GetJson(string name = null);

        [Post("/json")]
        Task<SampleModel> PostJson(SampleModel model);

        [Get("/binary")]
        Task<HttpResponseMessage> GetBinary();

        [Get("/status")]
        Task<HttpResponseMessage> GetStatus(int? statusCode = null);
    }
}
