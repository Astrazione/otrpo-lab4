namespace lab4.Model
{
    internal class Subscriptions
    {
        internal IEnumerable<long> UserIds { get; set; }
        internal IEnumerable<long> GroupIds { get; set; }

        public Subscriptions(IEnumerable<long> users, IEnumerable<long> groups)
        {
            UserIds = users;
            GroupIds = groups;
        }

        public Subscriptions()
        {
            UserIds = [];
            GroupIds = [];
        }
    }
}
