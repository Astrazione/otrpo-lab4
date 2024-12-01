namespace lab4.Model
{
	internal class RichUser : User
	{
		public List<long> Followers { get; set; } = [];
		public Subscriptions Subscriptions { get; set; } = new Subscriptions();
	}
}
