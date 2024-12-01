using lab4.Model;
using lab4.Model.Responces;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace lab4.Services
{
    public enum QueryType
    {
        UsersCount,
        GroupsCount,
        Top5FollowersUsers,
        Top5PopGroups,
        MutualFollowers,
        NoQuery
    }

    internal partial class GraphDbProvider
	{
        readonly SemaphoreSlim _semaphore;
        readonly IGraphClient _client;

		public GraphDbProvider(string username = "neo4j", string password = "password", string uri = "bolt://localhost:7687")
		{
            _client = new BoltGraphClient(new Uri(uri), username, password);
            _semaphore = new SemaphoreSlim(20);

            try
            {
                _client.ConnectAsync().Wait();
                Logger.I("Соединение с Neo4j установлено!").Wait();
            }
            catch (NeoException ex)
            {
                Logger.E($"Ошибка подключения: {ex.Message}").Wait();
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task AddUserNode(User user)
        {
            _semaphore.Wait();
            await _client.Cypher
                .Create("(u:User {id: $id, screen_name: $screenName, name: $name, sex: $sex, home_town: $homeTown})")
                .WithParam("id", user.Id)
                .WithParam("screenName", user.ScreenName)
                .WithParam("name", user.Name)
                .WithParam("sex", user.Sex)
                .WithParam("homeTown", user.HomeTown)
                .ExecuteWithoutResultsAsync();
            _semaphore.Release();
        }

        public async Task AddGroupNode(Group group)
        {
            _semaphore.Wait();
            await _client.Cypher
                .Create("(g:Group {id: $id, name: $name, screen_name: $screenName})")
                .WithParam("id", group.Id)
                .WithParam("name", group.Name)
                .WithParam("screenName", group.ScreenName)
                .ExecuteWithoutResultsAsync();
            _semaphore.Release();
        }

        public async Task CreateFollowRelationship(long userId1, long userId2)
        {
            _semaphore.Wait();
            await _client.Cypher
                .Match("(u1:User {id: $userId1}), (u2:User {id: $userId2})")
                .Create("(u1)<-[:Follow]-(u2)")
                .WithParams(new { userId1, userId2 })
                .ExecuteWithoutResultsAsync();
            _semaphore.Release();
        }

        public async Task CreateSubscribeRelationship(long userId, long groupId)
        {
            _semaphore.Wait();
            await _client.Cypher
                .Match("(u:User {id: $userId}), (g:Group {id: $groupId})")
                .Create("(u)-[:Subscribe]->(g)")
                .WithParams(new { userId, groupId })
                .ExecuteWithoutResultsAsync();
            _semaphore.Release();
        }

        public async Task CreateSubscribeRelationshipUser(long userId1, long userId2)
        {
            _semaphore.Wait();
            await _client.Cypher
                .Match("(u1:User {id: $userId1}), (u2:User {id: $userId2})")
                .Create("(u1)-[:Subscribe]->(u2)")
                .WithParams(new { userId1, userId2 })
                .ExecuteWithoutResultsAsync();
            _semaphore.Release();
        }
    }
}
