using System.Text;
using Microsoft.AspNetCore.Http;

namespace IndieVault.Tests.TestHelpers
{
    public static class FakeFormFile
    {
        public static IFormFile Create(string fileName, string content = "fake image bytes")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }
    }
}
