using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
