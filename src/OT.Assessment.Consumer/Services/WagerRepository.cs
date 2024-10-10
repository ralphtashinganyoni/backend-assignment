using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OT.Assessment.Common.Data.DTOs;


namespace OT.Assessment.Consumer.Services
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
                            await InsertWagerAsync(connection, transaction, wagerDto);
                            await InsertTransactionAsync(connection, transaction, wagerDto);

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
            IF NOT EXISTS (SELECT 1 FROM Games WHERE GameName = @GameName)
            BEGIN
                INSERT INTO Games (GameId, GameName)
                VALUES (NEWID(), @GameName)
            END";

            await connection.ExecuteAsync(sql, new { GameName = gameName }, transaction);
            _logger.LogInformation($"Ensured Game {gameName} exists.");
        }

        private async Task EnsureProviderExistsAsync(SqlConnection connection, SqlTransaction transaction, string providerName)
        {
            const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM Providers WHERE ProviderName = @Provider)
            BEGIN
                INSERT INTO Providers (ProviderId, ProviderName)
                VALUES (NEWID(), @Provider)
            END";

            await connection.ExecuteAsync(sql, new { Provider = providerName }, transaction);
            _logger.LogInformation($"Ensured Provider {providerName} exists.");
        }

        private async Task InsertWagerAsync(SqlConnection connection, SqlTransaction transaction, WagerDto wagerDto)
        {
            const string sql = @"
            INSERT INTO CasinoWagers 
            (WagerId, GameId, ProviderId, AccountId, Amount, CreatedDateTime, NumberOfBets, Duration, SessionData, BrandId)
            VALUES 
            (@WagerId, 
            (SELECT GameId FROM Games WHERE GameName = @GameName), 
            (SELECT ProviderId FROM Providers WHERE ProviderName = @Provider), 
            @AccountId, @Amount, @CreatedDateTime, @NumberOfBets, @Duration, @SessionData, @BrandId)";

            await connection.ExecuteAsync(sql, new
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
                wagerDto.BrandId
            }, transaction);

            _logger.LogInformation($"Wager {wagerDto.WagerId} inserted.");
        }

        private async Task InsertTransactionAsync(SqlConnection connection, SqlTransaction transaction, WagerDto wagerDto)
        {
            const string sql = @"
            INSERT INTO Transactions 
            (TransactionId, WagerId, TransactionTypeId, ExternalReferenceId)
            VALUES 
            (@TransactionId, @WagerId, @TransactionTypeId, @ExternalReferenceId)";

            await connection.ExecuteAsync(sql, new
            {
                wagerDto.TransactionId,
                wagerDto.WagerId,
                wagerDto.TransactionTypeId,
                wagerDto.ExternalReferenceId
            }, transaction);

            _logger.LogInformation($"Transaction {wagerDto.TransactionId} inserted for Wager {wagerDto.WagerId}.");
        }
    }
}
