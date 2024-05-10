using System.Threading.Tasks;
using Zs.Common.Models;

namespace Zs.VkActivity.Worker.Abstractions;

public interface IActivityLogger
{
    Task<Result> SaveUsersActivityAsync();
    Task<Result> ChangeAllUserActivitiesToUndefinedAsync();
}