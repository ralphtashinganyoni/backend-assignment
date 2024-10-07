using Newtonsoft.Json;

namespace OT.Assessment.Consumer.Data.DTOs
{
    public class WagerDto
    {
        [JsonProperty("wagerId")]
        public string WagerId { get; set; }

        [JsonProperty("theme")]
        public string Theme { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("gameName")]
        public string GameName { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("brandId")]
        public string BrandId { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("externalReferenceId")]
        public string ExternalReferenceId { get; set; }

        [JsonProperty("transactionTypeId")]
        public string TransactionTypeId { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("numberOfBets")]
        public int NumberOfBets { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("sessionData")]
        public string SessionData { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
    }
}
