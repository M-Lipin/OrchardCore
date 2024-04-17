#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Media.Indexes;
using OrchardCore.Media.Models;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Media.Services;

public class MediaInfoService : IMediaInfoService
{
    private readonly ISession _session;
    private readonly UserManager<IUser> _userManager;
    private readonly IAuthorizationService _authorizationService;

    public MediaInfoService(
        ISession session,
        UserManager<IUser> userManager,
        IAuthorizationService authorizationService)
    {
        _session = session;
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    public async Task<MediaInfo?> GetUserMediaInfoAsync(string username, string filePath)
    {
        var user = await GetUserByUsernameAsync(username);
        return await _session
            .Query<MediaInfo, MediaInfoIndex>(index => index.UserId == user.UserId && index.Path == filePath)
            .FirstOrDefaultAsync();
    }

    public async Task<MediaInfo?> GetUserMediaInfoByPathAsync(string filePath)
    {
        return await _session
            .Query<MediaInfo, MediaInfoIndex>(index => index.Path == filePath)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<MediaInfo>> GetUserMediaInfosAsync(string username)
    {
        var user = await GetUserByUsernameAsync(username);
        return await _session
            .Query<MediaInfo, MediaInfoIndex>(index => index.UserId == user.UserId)
            .ListAsync();
    }

    public async Task<bool> IsUserFileOwnerAsync(string username, params string[] filePaths)
    {
        int countFoundPaths = 0;
        var user = await GetUserByUsernameAsync(username);

        foreach (var filePath in filePaths)
        {
            var mediaInfo = await _session
                .Query<MediaInfo, MediaInfoIndex>(index => index.UserId == user.UserId)
                .Where(p => p.Path == filePath)
                .FirstOrDefaultAsync();
            if (mediaInfo != null)
            {
                countFoundPaths++;
            }
        }

        if (countFoundPaths == filePaths.Length)
        {
            return true;
        }

        return false;
    }

    public async Task AddUserMediaInfoAsync(string username, string filePath)
    {
        var user = await GetUserByUsernameAsync(username);
        var mediaInfo = new MediaInfo
        {
            UserId = user.UserId,
            Path = filePath
        };
        await _session.SaveAsync(mediaInfo);
        await _session.SaveChangesAsync();
    }

    private async Task<User> GetUserByUsernameAsync(string username)
    {
        _userManager.NormalizeName(username);
        return await _session
            .Query<User, UserIndex>(userIndex => userIndex.NormalizedUserName == username)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> TryDeleteMediaInfoAsync(ClaimsPrincipal user, string filePath)
    {
        if (user.Identity?.Name == null)
        {
            return false;
        }

        var mediaInfo = await GetUserMediaInfoByPathAsync(filePath);
        var userInfo = await GetUserByUsernameAsync(user.Identity.Name);

        if (mediaInfo == null)
        {
            return false;
        }

        if( mediaInfo.UserId != userInfo.UserId && !await _authorizationService.AuthorizeAsync(user, Permissions.ManageAllUsersMedia))
        {
            return false;
        }

        _session.Delete(mediaInfo);
        await _session.SaveChangesAsync();
        return true;
    }
}
