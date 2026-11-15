using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces
{
    public interface IGameService
    {
        Task<GameFormDataDto> GetFormDataAsync();
        Task UploadGameAsync(GameUploadDto uploadDto, string userId);
        Task<List<MyGameDto>> GetMyGamesAsync(string userId);
    }
}
