using lab4.Model.Responces;
using System.Text.Json.Serialization;

namespace lab4.Model
{
    internal class User : INamed
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("screen_name")]
        public string? ScreenName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("sex")]
        public int Sex { get; set; }

        [JsonPropertyName("home_town")]
        public string? HomeTown { get; set; }

        public static IEnumerable<User> CollectFromResponse(UsersResponse usersResponse)
        {
            List<User> users = [];

            foreach (var user in usersResponse.response)
            {
                users.Add(new User()
                {
                    Id = user.id,
                    Name = $"{user.first_name} {user.last_name}",
                    ScreenName = user.screen_name,
                    HomeTown = user.home_town ?? user.city?.title,
                    Sex = user.sex
                });
            }

            return users;
        }
    }
}
