using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace lab4.Model.Responces
{
    internal class UsersResponse
    {
        public List<Response> response { get; set; }

        internal class City
        {
            public string title { get; set; }
        }

        internal class Response
        {
            public int id { get; set; }

            public string home_town { get; set; }

            public int sex { get; set; }

            public string screen_name { get; set; }

            public string first_name { get; set; }

            public string last_name { get; set; }

            public City city { get; set; }
        }
    }
}
