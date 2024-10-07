namespace OT.Assessment.App.Model.DTOs
{
    public class WagerSummaryDto
    {
        public Guid WagerId { get; set; }
        public string GameName { get; set; }
        public string Provider { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
