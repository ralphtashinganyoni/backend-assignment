namespace OT.Assessment.Consumer.Data.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid WagerId { get; set; }
        public Guid TransactionTypeId { get; set; }
        public Guid? ExternalReferenceId { get; set; }
    }
}
