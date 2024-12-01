namespace lab4.Model
{
    internal interface INamed
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? ScreenName { get; set; }
    }
}
