using Microsoft.Identity.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

namespace OT.Assessment.Consumer.Data.Entities
{
    public class Wager
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public Guid ProviderId { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; } = DateTimeOffset.Now;
        public int NumberOfBets { get; set; }
        public long Duration { get; set; }
        public string SessionData { get; set; }
    }
}
