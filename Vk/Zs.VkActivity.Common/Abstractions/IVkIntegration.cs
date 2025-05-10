using Zs.VkActivity.Common.Models.VkApi;

namespace Zs.VkActivity.Common.Abstractions;

public interface IVkIntegration
{
    /// <param name="screenNames">User IDs or ScreenNames</param>
    Task<List<UserResponse>> GetUsersWithActivityInfoAsync(string[] screenNames);

    /// <param name="screenNames">User IDs or ScreenNames</param>
    Task<List<UserResponse>> GetUsersWithFullInfoAsync(string[] screenNames);

    /// <summary>Get friend IDs for specific user</summary>
    Task<int[]> GetFriendIds(int userId);
}