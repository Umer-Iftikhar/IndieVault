namespace IndieVault.Tests.TestHelpers
{
    // Temporary folder used as wwwroot for file upload tests; deleted on dispose.
    public sealed class TempWebRoot : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "indievault-tests", Guid.NewGuid().ToString());

        public TempWebRoot() => Directory.CreateDirectory(Path);

        public string GameFolder(int gameId) => System.IO.Path.Combine(Path, "images", "games", gameId.ToString());

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
