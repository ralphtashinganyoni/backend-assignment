using OT.Assessment.App.Model.DTOs;

namespace OT.Assessment.App.Model.Repository
{
    public interface IWagerRepository
    {
        Task<IEnumerable<WagerSummaryDto>> GetPlayerWagersAsync(Guid playerId, int pageSize, int pageNumber);
        Task<IEnumerable<TopSpenderDto>> GetTopSpendersAsync(int count);
    }
}
