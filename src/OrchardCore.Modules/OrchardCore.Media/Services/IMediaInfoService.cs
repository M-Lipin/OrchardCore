#nullable enable
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.Services;

public interface IMediaInfoService
{
    public Task<MediaInfo?> GetUserMediaInfoAsync(string username, string filePath);

    public Task<MediaInfo?> GetUserMediaInfoByPathAsync(string filePath);

    public Task<IEnumerable<MediaInfo>> GetUserMediaInfosAsync(string username);

    public Task<bool> IsUserFileOwnerAsync(string username, params string[] filePaths);

    public Task AddUserMediaInfoAsync(string username, string filePath);

    public Task<bool> TryDeleteMediaInfoAsync(ClaimsPrincipal user, string filePath);
}
