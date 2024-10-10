namespace OT.Assessment.Common.Data.DTOs
{
    public class WagerSummaryDto
    {
        public Guid WagerId { get; set; }
        public string GameName { get; set; }
        public string Provider { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
    }
}
