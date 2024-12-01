using lab4.Model;
using lab4.Services;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace lab4
{
	class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				await ProcessMain(args);
			}
			catch (ArgumentNullException ex)
			{
                await Logger.E(ex.Message);
			}
			catch (ArgumentException ex)
			{
				await Logger.E(ex.Message);
			}
		}

		static async Task ProcessMain(string[] args)
		{
			(string accessToken, string userIdStr, string resultFilePath, 
			 QueryType query, string uri, string login, string password, int requestsPerSec) = await ParamsParser.Parse(args);

			short recursionLevel = 2;

			GraphDbProvider graph = new(login, password, uri);

			if (query != QueryType.NoQuery)
			{
				await ProcessQueries(query, resultFilePath, graph);
				return;
			}

			VkApiClient client = new(accessToken, "5.199", requestsPerSec);
			if (!await client.CheckConnection())
				return;

			long userId = await GetUserId(client, userIdStr);

            var watch = Stopwatch.StartNew();
            var (users, userChunks, userInfoFillingTask) = await GetUsersWithDataAsync(userId, client, recursionLevel);
			var groups = await GetGroupsAsync(client, users);

			await userInfoFillingTask;
			var usersFilled = userChunks.SelectMany(ch => ch);
			watch.Stop();
			await Logger.I($"Получена информация о {users.Count} пользователях и {groups.Count()} группах за {watch.Elapsed.TotalSeconds:f2} секунд");

			await FillDatabaseAsync(graph, usersFilled, groups);
		}

		static async Task<long> GetUserId(VkApiClient client, string userIdStr)
		{
            long userId = long.Parse(await client.GetUserIdFromUsername(userIdStr));

            if (userId.ToString() != userIdStr)
                await Logger.I($"Ник пользователя: {userIdStr}");
            await Logger.I($"Id пользователя: {userId}");

			return userId;
        }

		static async Task<IEnumerable<Group>> GetGroupsAsync(VkApiClient client, ConcurrentDictionary<long, RichUser> users)
		{
            var groupIds = users.SelectMany(u => u.Value.Subscriptions?.GroupIds ?? []).ToHashSet();
            var groupIdChunks = groupIds.Chunk(300);
            var groupChunks = new BlockingCollection<IEnumerable<Group>>();
            await Parallel.ForEachAsync(groupIdChunks, async (groupChunk, _) => groupChunks.Add(await client.GetGroupsInfo(groupChunk)));
			return groupChunks.SelectMany(ch => ch);
        }

		static async Task<(ConcurrentDictionary<long, RichUser>, IEnumerable<RichUser[]>, Task)> GetUsersWithDataAsync(long userId, VkApiClient client, short recursionLevel)
		{
            await Logger.I($"Поиск подписчиков и подписок с уровнем углубления {recursionLevel}");
            var users = await client.GetFollowersAndSubscriptionsInit(userId, recursionLevel);
            await Logger.I($"Поиск подписчиков и подписок завершён");

            var usersChuncks = users.Values.Chunk(300);
			return (users, usersChuncks, Parallel.ForEachAsync(usersChuncks, async (chunk, _) => await client.FillUsersWithData(chunk)));
        }

		static async Task FillDatabaseAsync(GraphDbProvider graph, IEnumerable<RichUser> usersFilled, IEnumerable<Group> groups)
		{
            await Logger.I("Запись информации в базу данных");
            var watch = Stopwatch.StartNew();

            var usersTask = Parallel.ForEachAsync(usersFilled, async (user, _) =>
            await graph.AddUserNode(user));
            var groupsTask = Parallel.ForEachAsync(groups, async (group, _) =>
                await graph.AddGroupNode(group));

			await usersTask;
            await Logger.I("Пользователи загружены");

            await groupsTask;
            await Logger.I("Группы загружены");

            List<Task> relationTasks = [];

            foreach (var user in usersFilled)
            {
                relationTasks.Add(Parallel.ForEachAsync(user.Followers, async (follower, _)
                    => await graph.CreateFollowRelationship(user.Id, follower)));

                relationTasks.Add(Parallel.ForEachAsync(user.Subscriptions.UserIds, async (subUser, _)
                    => await graph.CreateSubscribeRelationshipUser(user.Id, subUser)));

                relationTasks.Add(Parallel.ForEachAsync(user.Subscriptions.GroupIds, async (group, _)
                    => await graph.CreateSubscribeRelationship(user.Id, group)));
            }

            await Task.WhenAll(relationTasks);
			watch.Stop();
            await Logger.I($"Информация была успешно записана в базу данных за {watch.Elapsed.TotalSeconds:f2} секунд");
        }

		static async Task ProcessQueries(QueryType queryType, string resultFilePath, GraphDbProvider graph)
		{
			string queryResult;

			switch (queryType)
			{
				case QueryType.UsersCount:
                    queryResult = await graph.SelectAllUsersCount();
					break;
				case QueryType.GroupsCount:
                    queryResult = await graph.SelectAllGroupsCount();
					break;
				case QueryType.Top5FollowersUsers:
                    queryResult = await graph.SelectTop5UsersByFollowers();
					break;
				case QueryType.Top5PopGroups:
                    queryResult = await graph.SelectTop5PopularGroups();
					break;
				case QueryType.MutualFollowers:
                    queryResult = await graph.SelectMutualFollowers();
                    break;
				default:
					throw new NotImplementedException("Неподдерживаемый тип запроса");
			}

			if (resultFilePath.ToLower() == "log")
			{
				await Logger.I(queryResult);
				return;
			}

			using (var resultFile = File.CreateText(resultFilePath))
				resultFile.Write(queryResult);
        }
	}
}