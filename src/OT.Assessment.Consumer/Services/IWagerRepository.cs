using OT.Assessment.Common.Data.DTOs;

namespace OT.Assessment.Consumer.Services
{
    public interface IWagerRepository
    {
        Task SaveWager(WagerDto wager);
    }
}
