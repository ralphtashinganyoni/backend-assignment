using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OT.Assessment.Common.Data.DTOs;

namespace OT.Assessment.Common.Data.Repositories
{
    public class WagerRepository : IWagerRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<WagerRepository> _logger;

        public WagerRepository(IConfiguration configuration, ILogger<WagerRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DatabaseConnection");
            _logger = logger;
        }

        public async Task SaveWager(WagerDto wagerDto)
        {
            if (wagerDto == null)
            {
                _logger.LogError("WagerDto is null");
                throw new ArgumentNullException(nameof(wagerDto));
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            await EnsurePlayerExistsAsync(connection, transaction, wagerDto);
                            await EnsureGameExistsAsync(connection, transaction, wagerDto.GameName);
                            await EnsureProviderExistsAsync(connection, transaction, wagerDto.Provider);
                            var Id = await InsertWagerAsync(connection, transaction, wagerDto);
                            await InsertTransactionAsync(connection, transaction, wagerDto, Id);
                            await transaction.CommitAsync();
                            _logger.LogInformation($"Wager {wagerDto.WagerId} successfully saved.");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, $"Error saving wager {wagerDto.WagerId}. Transaction rolled back.");
                            throw;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL error occurred while saving wager.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving wager.");
                throw;
            }
        }

        public async Task<IEnumerable<WagerSummaryDto>> GetPlayerWagersAsync(Guid playerId, int pageSize, int pageNumber)
        {
            _logger.LogInformation("Fetching wagers for player {PlayerId} with page size {PageSize} and page number {PageNumber}.", playerId, pageSize, pageNumber);

            var sql = @"
        SELECT 
            Wagers.Id AS ClientWagerId, 
            Games.Name AS GameName, 
            Providers.Name AS Provider, 
            Wagers.Amount, 
            Wagers.CreatedDateTime, 
            Wagers.NumberOfBets, 
            Wagers.Duration 
        FROM 
            Wagers
        INNER JOIN 
            Providers ON Wagers.ProviderId = Providers.Id
        INNER JOIN 
            Games ON Wagers.GameId = Games.Id
        WHERE 
            Wagers.AccountId = @PlayerId
        ORDER BY 
            Wagers.CreatedDateTime DESC
        OFFSET (@Page - 1) * @PageSize ROWS 
        FETCH NEXT @PageSize ROWS ONLY";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var wagers = await connection.QueryAsync<WagerSummaryDto>(sql, new
                    {
                        PlayerId = playerId,
                        PageSize = pageSize,
                        Page = pageNumber
                    });
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
            SELECT TOP (@Count) Wagers.AccountId, Username, SUM(Amount) AS TotalAmountSpend 
            FROM Wagers 
            INNER JOIN Players ON Wagers.AccountId = Players.AccountId
            GROUP BY Wagers.AccountId, Username
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

        private async Task EnsurePlayerExistsAsync(SqlConnection connection, SqlTransaction transaction, WagerDto wagerDto)
        {
            const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM Players WHERE AccountId = @AccountId)
            BEGIN
                INSERT INTO Players (AccountId, Username, CountryCode, CreatedDateTime)
                VALUES (@AccountId, @Username, @CountryCode, SYSDATETIMEOFFSET())
            END";

            await connection.ExecuteAsync(sql, new
            {
                wagerDto.AccountId,
                wagerDto.Username,
                wagerDto.CountryCode
            }, transaction);

            _logger.LogInformation($"Ensured Player {wagerDto.AccountId} exists.");
        }

        private async Task EnsureGameExistsAsync(SqlConnection connection, SqlTransaction transaction, string gameName)
        {
            const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM Games WHERE Name = @GameName)
            BEGIN
                INSERT INTO Games (Id, Name)
                VALUES (NEWID(), @GameName)
            END";

            await connection.ExecuteAsync(sql, new { GameName = gameName }, transaction);
            _logger.LogInformation($"Ensured Game {gameName} exists.");
        }

        private async Task EnsureProviderExistsAsync(SqlConnection connection, SqlTransaction transaction, string providerName)
        {
            const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM Providers WHERE Name = @Provider)
            BEGIN
                INSERT INTO Providers (Id, Name)
                VALUES (NEWID(), @Provider)
            END";

            await connection.ExecuteAsync(sql, new { Provider = providerName }, transaction);
            _logger.LogInformation($"Ensured Provider {providerName} exists.");
        }

        private async Task<Guid> InsertWagerAsync(SqlConnection connection, SqlTransaction transaction, WagerDto wagerDto)
        {
            const string sql = @"
            INSERT INTO Wagers 
            (ClientWagerId, GameId, ProviderId, AccountId, Amount, CreatedDateTime, NumberOfBets, Duration, SessionData, BrandId, CountryCode)
            OUTPUT INSERTED.Id 
            VALUES 
            (@WagerId, 
            (SELECT Id FROM Games WHERE Name = @GameName), 
            (SELECT Id FROM Providers WHERE Name = @Provider), 
            @AccountId, @Amount, @CreatedDateTime, @NumberOfBets, @Duration, @SessionData, @BrandId, @CountryCode
            )";

            Guid Id =  await connection.ExecuteScalarAsync<Guid>(sql, new
            {
                wagerDto.WagerId,
                wagerDto.GameName,
                wagerDto.Provider,
                wagerDto.AccountId,
                wagerDto.Amount,
                wagerDto.CreatedDateTime,
                wagerDto.NumberOfBets,
                wagerDto.Duration,
                wagerDto.SessionData,
                wagerDto.BrandId,
                wagerDto.CountryCode
            }, transaction);

            _logger.LogInformation($"Wager {wagerDto.WagerId} inserted.");
            return Id;
        }

        private async Task InsertTransactionAsync(SqlConnection connection, SqlTransaction transaction, WagerDto wagerDto, Guid wagerId)
        {
            const string sql = @"
                                INSERT INTO Transactions 
                                (ClientTransactionId, WagerId, TransactionTypeId, ExternalReferenceId)
                                VALUES 
                                (@TransactionId, @WagerId, @TransactionTypeId, @ExternalReferenceId)";

            await connection.ExecuteAsync(sql, new
            {
                wagerDto.TransactionId,
                WagerId = wagerId, 
                wagerDto.TransactionTypeId,
                wagerDto.ExternalReferenceId
            }, transaction);

            _logger.LogInformation($"Transaction {wagerDto.TransactionId} inserted for Wager {wagerId}.");
        }

    }
}
