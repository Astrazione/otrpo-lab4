namespace lab4.Model.Responces
{
    internal class SubscriptionsResponse
    {
        public Response response { get; set; }

        public class Response
        {
            public Users users { get; set; }
            public Groups groups { get; set; }
        }
    }

    public class Groups
    {
        public int count { get; set; }
        public List<long> items { get; set; }
    }

    public class Users
    {
        public int count { get; set; }
        public List<long> items { get; set; }
    }
}
