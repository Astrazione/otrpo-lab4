using System.Text.Json.Serialization;

namespace lab4.Model
{
    public class Group : INamed
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("screen_name")]
        public string? ScreenName { get; set; }
    }
}
