namespace lab4.Model.Responces
{

    public class Response
    {
        public List<Group> groups { get; set; }
    }

    public class GroupResponse
    {
        public Response response { get; set; }
    }
}
