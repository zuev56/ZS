using Zs.Common.Models;
using Zs.VkActivity.Data.Models;

namespace Zs.VkActivity.Common.Abstractions;

public interface IUserManager
{
    Task<Result<List<User>>> AddUsersAsync(params string[] screenNames);
    Task<Result> UpdateUsersAsync(params int[] userIds);
    Task<Result<List<User>>> AddFriendsOf(int userId);
    Task<Result<User>> GetUserAsync(int userId);
}