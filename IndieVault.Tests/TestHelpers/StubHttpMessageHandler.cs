using System.Net;
using System.Text;

namespace IndieVault.Tests.TestHelpers
{
    // Returns canned responses per request path so HttpClient-based services can be tested offline.
    public class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, (HttpStatusCode Status, string Body)> _responses = new();

        public List<HttpRequestMessage> Requests { get; } = new();

        public StubHttpMessageHandler Respond(string pathAndQueryStart, string json, HttpStatusCode status = HttpStatusCode.OK)
        {
            _responses[pathAndQueryStart] = (status, json);
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var path = request.RequestUri!.PathAndQuery;
            // Longest matching prefix wins, so "/users/x/repos" beats "/users/x"
            var match = _responses.Keys.Where(k => path.StartsWith(k)).OrderByDescending(k => k.Length).FirstOrDefault();
            if (match == null)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var (status, body) = _responses[match];
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
