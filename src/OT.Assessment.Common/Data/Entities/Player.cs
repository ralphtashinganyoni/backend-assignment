namespace OT.Assessment.Consumer.Data.Entities
{
    public class Player
    {
        public Guid AccountId { get; set; }
        public string Username { get; set; }
        public string CountryCode { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; } = DateTimeOffset.Now;
    }
}
