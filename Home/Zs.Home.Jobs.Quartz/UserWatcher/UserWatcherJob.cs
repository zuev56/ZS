// using System.Text;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Options;
// using Zs.Common.Extensions;
// using Zs.Common.Services.Http;
// using Zs.Home.Application.Features.VkUsers;
//
// namespace Zs.Home.Jobs.Quartz.UserWatcher;
//
// internal sealed class UserWatcherJob
// {
//     private readonly IUserWatcher _userWatcher;
//     private readonly IOptions<UserWatcherJobSettings> _options;
//
//     public UserWatcherJob(IUserWatcher userWatcher, IOptions<UserWatcherJobSettings> options)
//     {
//         _userWatcher = userWatcher;
//         _options = options;
//     }
//
//     internal void Execute()
//     {
//         // TODO: Джоб выявляет неактивных пользователей, формирует сообщение и отправляет его
//         // А нахрена это выносить из бота? Зачем тогда бот нужен будет, только как прокладка для взаимодействия с пользователем?
//         // Тогда джобы буду от него зависить
//     }
//
//     private async Task<string> DetectInactiveUsersAsync()
//     {
//         var result = new StringBuilder();
//         foreach (var userId in _options.Value.TrackedIds)
//         {
//             var inactiveTime = await _userWatcher.GetInactiveTimeAsync(userId);
//             if (inactiveTime < _options.Value.InactiveHoursLimit.Hours())
//                 continue;
//
//             var user = await GetUserAsync(userId);
//             if (user == null)
//                 continue;
//
//             var userName = $"{user.FirstName} {user.LastName}";
//             result.AppendLine($@"User {userName} is not active for {inactiveTime:hh\:mm\:ss}");
//         }
//
//         return result.ToString().Trim();
//     }
//
//     private async Task<User?> GetUserAsync(int userId)
//     {
//         var url = $"{_options.Value.VkActivityApiUri}/api/users/{userId}";
//         var user = await Request.Create(url).GetAsync<User>();
//         return user;
//     }
// }
