using Dapper;
using Microsoft.Data.SqlClient;
using OT.Assessment.App.Model.DTOs;

namespace OT.Assessment.App.Model.Repository
{
    public class WagerRepository : IWagerRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<WagerRepository> _logger;

        public WagerRepository(IConfiguration configuration, ILogger<WagerRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<IEnumerable<WagerSummaryDto>> GetPlayerWagersAsync(Guid playerId, int pageSize, int pageNumber)
        {
            _logger.LogInformation("Fetching wagers for player {PlayerId} with page size {PageSize} and page number {PageNumber}.", playerId, pageSize, pageNumber);

            var sql = @"
            SELECT WagerId, GameId, ProviderId, Amount, CreatedDateTime, NumberOfBets, Duration 
            FROM CasinoWagers 
            WHERE AccountId = @PlayerId
            ORDER BY CreatedDateTime DESC
            OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var wagers = await connection.QueryAsync<WagerSummaryDto>(sql, new { PlayerId = playerId, PageSize = pageSize, Page = pageNumber });
                    _logger.LogInformation("Successfully retrieved {Count} wagers for player {PlayerId}.", wagers.Count(), playerId);
                    return wagers;
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL error occurred while fetching wagers for player {PlayerId}.", playerId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching wagers for player {PlayerId}.", playerId);
                throw;
            }
        }

        public async Task<IEnumerable<TopSpenderDto>> GetTopSpendersAsync(int count)
        {
            _logger.LogInformation("Fetching top {Count} spenders.", count);

            var sql = @"
            SELECT TOP (@Count) AccountId, SUM(Amount) AS TotalAmountSpend 
            FROM CasinoWagers 
            GROUP BY AccountId 
            ORDER BY TotalAmountSpend DESC";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var topSpenders = await connection.QueryAsync<TopSpenderDto>(sql, new { Count = count });
                    _logger.LogInformation("Successfully retrieved top spenders count: {Count}.", topSpenders.Count());
                    return topSpenders;
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL error occurred while fetching top spenders.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching top spenders.");
                throw;
            }
        }
    }

}
