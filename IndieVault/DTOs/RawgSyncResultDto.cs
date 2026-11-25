namespace IndieVault.DTOs
{
    public class RawgSyncResultDto
    {
        // New games added to the catalogue
        public int Imported { get; set; }

        // Games imported earlier whose download link was replaced with their real store page
        public int LinksUpdated { get; set; }
    }
}
