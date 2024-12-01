using lab4.Model;
using lab4.Model.Responces;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using System.Text;

namespace lab4.Services
{
    internal partial class GraphDbProvider
    {
        public async Task<string> SelectAllUsersCount()
        {
            var users = await _client.Cypher
                .Match("(u:User)")
                .Return(u => u.Count())
                .ResultsAsync;

            StringBuilder stringBuilder = new StringBuilder();
            return $"Всего пользователей: {users.First()}";
        }

        public async Task<string> SelectAllGroupsCount()
        {
            var groups = await _client.Cypher
                .Match("(g:Group)")
                .Return(g => g.Count())
                .ResultsAsync;

            return $"Всего групп: {groups.First()}";
        }

        public async Task<string> SelectTop5UsersByFollowers()
        {
            var users = await _client.Cypher
                .Match("(u:User)<-[:Follow]-(f:User)")
                .With("u, COUNT(f) AS FollowerCount")
                .Return(u => new
                {
                    User = u.As<User>(),
                    FollowerCount = Return.As<int>("FollowerCount")
                })
                .OrderByDescending("FollowerCount")
                .Limit(5)
                .ResultsAsync;

            var sb = new StringBuilder("Топ 5 пользователей по количеству фолловеров:\n");
            foreach (var user in users)
               sb.AppendLine($"{user.User.Id}: {user.User.ScreenName} ({user.User.Name}) - {user.FollowerCount} фолловеров");
            
            return sb.ToString();
        }

        public async Task<string> SelectTop5PopularGroups()
        {
            var groups = await _client.Cypher
                .Match("(g:Group)<-[:Subscribe]-(u:User)")
                .With("g, COUNT(u) AS SubscriberCount")
                .Return(g => new
                {
                    Group = g.As<Group>(),
                    SubscriberCount = Return.As<int>("SubscriberCount")
                })
                .OrderByDescending("SubscriberCount")
                .Limit(5)
                .ResultsAsync;

            var sb = new StringBuilder("Топ 5 самых популярных групп:\n");
            foreach (var group in groups)
                sb.AppendLine($"{group.Group.Id}: {group.Group.Name} ({group.Group.ScreenName}) - {group.SubscriberCount} подписчиков");

            return sb.ToString();
        }

        public async Task<string> SelectMutualFollowers()
        {
            var mutuals = await _client.Cypher
                .Match("(u1:User)<-[:Follow]->(u2:User)")
                .Return((u1, u2) => new
                {
                    User1 = u1.As<User>(),
                    User2 = u2.As<User>()
                }).ResultsAsync;

            var sb = new StringBuilder("Пользователи, которые фолоуверы друг друга:");
            foreach (var pair in mutuals)
                sb.AppendLine($"{pair.User1.ScreenName} <-> {pair.User2.ScreenName}");

            return sb.ToString();
        }
    }
}
