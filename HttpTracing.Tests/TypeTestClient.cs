using System.Net.Http;

namespace HttpTracing.Tests
{
    public class TypeTestClient
    {
        public TypeTestClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }
    }
}
