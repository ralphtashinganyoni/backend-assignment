namespace OT.Assessment.App.Model.DTOs
{
    public class TopSpenderDto
    {
        public Guid AccountId { get; set; }
        public string Username { get; set; }
        public decimal TotalAmountSpend { get; set; }
    }
}
