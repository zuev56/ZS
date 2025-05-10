﻿using System.Net.Http.Json;
using Zs.Common.Models;
using Zs.VkActivity.Common.Abstractions;
using Zs.VkActivity.Common.Models.VkApi;

namespace Zs.VkActivity.Common.Services;

public sealed class VkIntegration : IVkIntegration
{
    // https://dev.vk.com/reference/objects/user
    private const string BaseUrl = "https://api.vk.com/method/";
    private readonly string _getUsersUrl;
    private readonly string _getFriendsUrl;
    private const string FieldsForGettingUserActivity = "online,last_seen";
    private const string FieldsForGettingFullUserInfo = "activities,about,books,bdate,career,connections,contacts,city,country," +
        "domain,education,exports,has_photo,has_mobile,home_town,photo_50,sex,site,schools,screen_name,verified,games,interests," +
        "maiden_name,military,movies,music,nickname,occupation,personal,quotes,relation,relatives,timezone,tv,universities,deactivated";

    private static readonly SemaphoreSlim _semaphore = new(1, 32);
    private static readonly TimeSpan _apiAccessTimeout = TimeSpan.FromSeconds(3);
    private static DateTime _lastApiAccessTime = DateTime.UtcNow;

    private readonly HttpClient _httpClient = new();
    public static readonly TimeSpan ApiAccessMinInterval = TimeSpan.FromSeconds(0.35);

    public VkIntegration(string token, string version)
    {
        ArgumentNullException.ThrowIfNull(nameof(token));
        ArgumentNullException.ThrowIfNull(nameof(version));

        _getUsersUrl = $"{BaseUrl}users.get?access_token={token}&v={version}&lang=ru";
        _getFriendsUrl = $"{BaseUrl}friends.get?access_token={token}&v={version}&lang=ru";
    }

    public async Task<List<UserResponse>> GetUsersWithActivityInfoAsync(string[] userScreenNames)
    {
        ArgumentNullException.ThrowIfNull(userScreenNames);

        if (userScreenNames.Length == 0)
            throw new ArgumentException("UserIds array couldn't be empty", nameof(userScreenNames));

        var url = $"{_getUsersUrl}&fields={FieldsForGettingUserActivity}&user_ids={string.Join(',', userScreenNames)}";

        return await GetVkUsersAsync(url);
    }

    private async Task<List<UserResponse>> GetVkUsersAsync(string url)
    {
        var responseResult = await GetResponseAsync<UsersResponse>(url, _httpClient).ConfigureAwait(false);

        if (!responseResult.Successful || responseResult.Value!.Users == null)
            throw new InvalidOperationException("GetVkUsersAsync error");

        return responseResult.Value.Users;
    }

    private static async Task<Result<TResponse?>> GetResponseAsync<TResponse>(string url, HttpClient httpClient)
        where TResponse : VkApiResponse
    {
        if (await _semaphore.WaitAsync(_apiAccessTimeout))
        {
            try
            {
                if (DateTime.UtcNow.Subtract(_lastApiAccessTime) < ApiAccessMinInterval)
                    await Task.Delay(ApiAccessMinInterval).ConfigureAwait(false);

                _lastApiAccessTime = DateTime.UtcNow;
                var httpResponseMessage = await httpClient.GetAsync(url).ConfigureAwait(false);

                var response = await httpResponseMessage.Content.ReadFromJsonAsync<TResponse>();

                if (response is not { Error: null })
                {
                    throw new InvalidOperationException(
                        "Unable to get correct response from Vk API"
                        + response?.Error != null
                            ? $": {response!.Error.Message} (Code: {response.Error.Code})"
                            : string.Empty);
                }

                return response;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return Result.Fail<TResponse?>("VK API access timeout error");
    }

    public async Task<List<UserResponse>> GetUsersWithFullInfoAsync(string[] userScreenNames)
    {
        ArgumentNullException.ThrowIfNull(userScreenNames);

        if (userScreenNames.Length == 0)
            throw new ArgumentException("UserIds array couldn't be empty", nameof(userScreenNames));

        var url = $"{_getUsersUrl}&fields={FieldsForGettingFullUserInfo}&user_ids={string.Join(',', userScreenNames)}";

        return await GetVkUsersAsync(url);
    }

    public async Task<int[]> GetFriendIds(int userId)
    {
        var url = $"{_getFriendsUrl}&user_id={userId}";

        var responseResult = await GetResponseAsync<FriendsResponse>(url, _httpClient).ConfigureAwait(false);

        if (!responseResult.Successful || responseResult.Value!.Data?.FriendIds == null)
            throw new InvalidOperationException("GetFriendIds error");

        return responseResult.Value.Data.FriendIds;
    }
}
