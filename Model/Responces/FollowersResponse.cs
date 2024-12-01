namespace lab4.Model.Responces
{
    internal class FollowersResponse
    {
        public Response response { get; set; }

        public class Response
        {
            public int count { get; set; }
            public List<long> items { get; set; }
        }
    }
}
