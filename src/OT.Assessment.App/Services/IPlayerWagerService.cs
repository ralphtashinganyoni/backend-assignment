using OT.Assessment.App.Model.DTOs;
using OT.Assessment.Common.Data.DTOs;

namespace OT.Assessment.App.Services
{
    public interface IPlayerWagerService
    {
        void PublishWagerToQueue(WagerDto wager);
        Task<IEnumerable<WagerSummaryDto>> GetPlayerWagersAsync(Guid playerId, int pageSize, int pageNumber);
        Task<IEnumerable<TopSpenderDto>> GetTopSpendersAsync(int count);
    }
}
