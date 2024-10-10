using OT.Assessment.Common.Data.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OT.Assessment.Common.Data.Repositories
{
    public interface IWagerRepository
    {
        Task<IEnumerable<WagerSummaryDto>> GetPlayerWagersAsync(Guid playerId, int pageSize, int pageNumber);
        Task<IEnumerable<TopSpenderDto>> GetTopSpendersAsync(int count);
        Task SaveWager(WagerDto wager);

    }
}
