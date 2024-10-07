using Dapper;
using Microsoft.Data.SqlClient;
using OT.Assessment.App.Model.DTOs;

namespace OT.Assessment.App.Model.Repository
{
    public class WagerRepository : IWagerRepository
    {
        private readonly string _connectionString;

        public WagerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IEnumerable<WagerSummaryDto>> GetPlayerWagersAsync(Guid playerId, int pageSize, int pageNumber)
        {
            var sql = @"
        SELECT WagerId, GameId, ProviderId, Amount, CreatedDateTime, NumberOfBets, Duration 
        FROM CasinoWagers 
        WHERE AccountId = @PlayerId
        ORDER BY CreatedDateTime DESC
        OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY";

            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<WagerSummaryDto>(sql, new { PlayerId = playerId, PageSize = pageSize, Page = pageNumber });
            }
        }

        public async Task<IEnumerable<TopSpenderDto>> GetTopSpendersAsync(int count)
        {
            var sql = @"
        SELECT TOP (@Count) AccountId, SUM(Amount) AS TotalAmountSpend 
        FROM CasinoWagers 
        GROUP BY AccountId 
        ORDER BY TotalAmountSpend DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                return  await connection.QueryAsync<TopSpenderDto>(sql, new { Count = count });
            }
        }
    }
}
