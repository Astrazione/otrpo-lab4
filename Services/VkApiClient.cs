using lab4.Model;
using lab4.Model.Responces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace lab4.Services
{
    internal class VkApiClient
    {
        readonly HttpClient _httpClient = new();
        readonly SemaphoreSlim _semaphore;
        readonly string _accessToken;
        readonly string _apiVersion;
        int delay;
        object _lock = new object();

        private async Task FreeSemaphore()
        {
            await Task.Delay(delay);
            _semaphore.Release();
        }

        internal VkApiClient(string accessToken, string apiVersion, int requestsPerSec = 3)
        {
            delay = 1000;
            _semaphore = new SemaphoreSlim(requestsPerSec);
            _accessToken = accessToken;
            _apiVersion = apiVersion;
        }

        internal async Task<IEnumerable<User>> GetUsersInfo(IEnumerable<long> userId)
        {
            await _semaphore.WaitAsync();

            string url = $"https://api.vk.com/method/users.get?user_ids={string.Join(',', userId)}&fields=sex,home_town,city,screen_name&access_token={_accessToken}&v={_apiVersion}";
            var response = await _httpClient.GetAsync(url);

            _ = Task.Run(async () =>
            {
                await Task.Delay(delay);
                _semaphore.Release();
            });

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var content = JsonSerializer.Deserialize<UsersResponse>(result);

                if (content is null || content.response is null || content.response.Count == 0)
                {
                    await Logger.E($"Ошибка при получении информации по группам\n{GetError(result)}");
                    return [];
                }

                return User.CollectFromResponse(content!);
            }
            else
            {
                await Logger.E($"Произошла ошибка при попытке найти данные о пользователях");
                return [];
            }
        }

        internal async Task<IEnumerable<Group>> GetGroupsInfo(IEnumerable<long> groupIds)
        {
            await _semaphore.WaitAsync();

            string url = $"https://api.vk.com/method/groups.getById?group_ids={string.Join(',', groupIds)}&access_token={_accessToken}&v={_apiVersion}";
            var response = await _httpClient.GetAsync(url);

            _ = Task.Run(async () =>
            {
                await Task.Delay(delay);
                _semaphore.Release();
            });

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var content = JsonSerializer.Deserialize<GroupResponse>(result);

                if (content is null || content.response is null || content.response.groups.Count == 0)
                {
                    await Logger.E($"Ошибка при получении информации по группам\n{GetError(result)}");
                    return [];
                }

                return content.response.groups;
            }
            else
            {
                await Logger.E($"Ошибка при попытке найти данные о группах");
                return [];
            }
        }


        internal async Task<List<long>> GetFollowers(long userId)
        {
            await _semaphore.WaitAsync();

            string url = $"https://api.vk.com/method/users.getFollowers?user_id={userId}&count=100&access_token={_accessToken}&v={_apiVersion}";
            var response = await _httpClient.GetAsync(url);

            _ = Task.Run(async () =>
            {
                await Task.Delay(delay);
                _semaphore.Release();
            });

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var content = JsonSerializer.Deserialize<FollowersResponse>(result);

                if (content is null || content.response is null || content.response.items.Count == 0)
                {
                    await Logger.W($"У пользователя с id {userId} не было найдено подписчиков");
                    return [];
                }

                return content.response.items;
            }
            else
            {
                await Logger.E($"Ошибка при попытке найти подписчиков у пользователя {userId} (Error Code: {response.StatusCode})");
                return [];
            }
        }

        internal string GetError(string response)
        {
            var jsonResponse = JsonDocument.Parse(response);

            if (jsonResponse.RootElement.TryGetProperty("error", out var errorElement) && errorElement.TryGetProperty("error_msg", out var errorMessage))
                if (errorElement.TryGetProperty("error_text", out var errorText))
                    return $"Ошибка:\t{errorMessage}\nТекст:\t{errorText}";
                else
                    return $"Ошибка:\t{errorMessage}";

            return "";
        }

        internal async Task<Subscriptions> GetSubscriptions(long userId)
        {
            // В подписках могут быть как пользователи, так и группы
            await _semaphore.WaitAsync();

            string url = $"https://api.vk.com/method/users.getSubscriptions?user_id={userId}&count=50&access_token={_accessToken}&v={_apiVersion}";
            var response = await _httpClient.GetAsync(url);

            _ = Task.Run(async () =>
            {
                await Task.Delay(delay);
                _semaphore.Release();
            });

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var content = JsonSerializer.Deserialize<SubscriptionsResponse>(result);
                var (users, groups) = (new List<long>(), new List<long>());

                if (content is null || content.response is null)
                    await Logger.E($"Ошибка при получении подписок у id{userId}\n{GetError(result)}");
                else
                {
                    if (content?.response.users.items.Count == 0)
                        await Logger.W($"У пользователя id{userId} не было найдено подписок (users)");
                    else
                        users = content?.response.users.items;

                    if (content?.response.groups.items.Count == 0)
                        await Logger.W($"У пользователя id{userId} не было найдено подписок (group)");
                    else
                        groups = content?.response.groups.items;
                }

                return new Subscriptions(users ?? [], groups ?? []);
            }
            else
            {
                await Logger.E($"Ошибка запроса при попытке найти подписчиков у пользователя {userId} (Error Code: {response.StatusCode})");
                return new Subscriptions();
            }
        }

        internal async Task<string> GetUserIdFromUsername(string username)
        {
            await _semaphore.WaitAsync();

            string url = $"https://api.vk.com/method/users.get?user_ids={username}&access_token={_accessToken}&v={_apiVersion}";
            var response = await _httpClient.GetStringAsync(url);

            _ = Task.Run(async () =>
            {
                await Task.Delay(delay);
                _semaphore.Release();
            });

            var jsonResponse = JsonDocument.Parse(response);

            if (jsonResponse.RootElement.TryGetProperty("response", out var responseElement) && responseElement.GetArrayLength() > 0)
                return responseElement[0].GetProperty("id").ToString();

            await Logger.W("Nickname/id пользователя не найден");

            return "";
        }

        internal async Task<bool> CheckConnection(string logIfSuccess = "")
        {
            string url = $"https://api.vk.com/method/users.get?user_id=1&access_token={_accessToken}&v={_apiVersion}";
            var response = await _httpClient.GetStringAsync(url);

            string error = GetError(response);

            if (error == "")
            {
                if (logIfSuccess != "")
                    await Logger.I(logIfSuccess);
                return true;
            }
            else
            {
                await Logger.E(error);
                return false;
            }
        }

        public async Task<ConcurrentDictionary<long, RichUser>> GetFollowersAndSubscriptionsInit(long userId, int recursionLevel = 1)
        {
            ConcurrentDictionary<long, RichUser> users = new();
            await GetFollowersAndSubscriptionsAsync(userId, recursionLevel, users);
            return users;
        }

        private async Task GetFollowersAndSubscriptionsAsync(long userId, int recursionLevel, ConcurrentDictionary<long, RichUser> users)
        {
            if (users.ContainsKey(userId))
                return;

            if (recursionLevel == 0)
            {
                users.TryAdd(userId, new RichUser() { Id = userId });
                return;
            }

            var user = new RichUser()
            {
                Id = userId,
                Followers = await GetFollowers(userId),
                Subscriptions = await GetSubscriptions(userId)
            };

            users.TryAdd(userId, user);

            await Parallel.ForEachAsync(user.Followers, async (followerId, _) =>
                await GetFollowersAndSubscriptionsAsync(followerId, recursionLevel - 1, users));
        }

        public async Task FillUsersWithData(RichUser[] users)
        {
            var usersData = (await GetUsersInfo(users.Select(u => u.Id))).ToList();

            if (usersData.Count != users.Length)
                return;

            for (int i = 0; i < users.Length; i++)
            {
                users[i].ScreenName = usersData[i].ScreenName;
                users[i].Sex = usersData[i].Sex;
                users[i].HomeTown = usersData[i].HomeTown;
                users[i].Name = usersData[i].Name;
            }
        }
    }
}
