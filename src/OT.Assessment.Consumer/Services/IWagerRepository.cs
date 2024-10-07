using OT.Assessment.Consumer.Data.DTOs;
using OT.Assessment.Consumer.Data.Entities;

namespace OT.Assessment.Consumer.Services
{
    public interface IWagerRepository
    {
        Task SaveWager(WagerDto wager);
    }
}
