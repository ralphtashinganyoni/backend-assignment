using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using OT.Assessment.Consumer.Data.DTOs;


namespace OT.Assessment.Consumer.Services
{
    public class WagerRepository : IWagerRepository
    {
        private readonly string _connectionString;

        public WagerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task SaveWager(WagerDto wagerDto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert into Players table if not exists
                        string insertPlayerSql = @"
                    IF NOT EXISTS (SELECT 1 FROM Players WHERE AccountId = @AccountId)
                    BEGIN
                        INSERT INTO Players (AccountId, Username, CountryCode, CreatedDateTime)
                        VALUES (@AccountId, @Username, @CountryCode, SYSDATETIMEOFFSET())
                    END";

                        await connection.ExecuteAsync(insertPlayerSql, new
                        {
                            wagerDto.AccountId,
                            wagerDto.Username,
                            wagerDto.CountryCode
                        }, transaction);

                        // 2. Insert into Games table if not exists
                        string insertGameSql = @"
                    IF NOT EXISTS (SELECT 1 FROM Games WHERE GameName = @GameName)
                    BEGIN
                        INSERT INTO Games (GameId, GameName)
                        VALUES (NEWID(), @GameName) -- Assuming new GameId is generated for each new game
                    END";

                        await connection.ExecuteAsync(insertGameSql, new
                        {
                            wagerDto.GameName
                        }, transaction);

                        // 3. Insert into Providers table if not exists
                        string insertProviderSql = @"
                    IF NOT EXISTS (SELECT 1 FROM Providers WHERE ProviderName = @Provider)
                    BEGIN
                        INSERT INTO Providers (ProviderId, ProviderName)
                        VALUES (NEWID(), @Provider) -- Assuming new ProviderId is generated for each new provider
                    END";

                        await connection.ExecuteAsync(insertProviderSql, new
                        {
                            wagerDto.Provider
                        }, transaction);

                        // 4. Insert into CasinoWagers table
                        string insertWagerSql = @"
                    INSERT INTO CasinoWagers 
                    (WagerId, GameId, ProviderId, AccountId, Amount, CreatedDateTime, NumberOfBets, Duration, SessionData, BrandId)
                    VALUES 
                    (@WagerId, 
                     (SELECT GameId FROM Games WHERE GameName = @GameName), 
                     (SELECT ProviderId FROM Providers WHERE ProviderName = @Provider), 
                     @AccountId, @Amount, @CreatedDateTime, @NumberOfBets, @Duration, @SessionData, @BrandId)";

                        await connection.ExecuteAsync(insertWagerSql, new
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

                        // 5. Insert into Transactions table
                        string insertTransactionSql = @"
                    INSERT INTO Transactions 
                    (TransactionId, WagerId, TransactionTypeId, ExternalReferenceId)
                    VALUES 
                    (@TransactionId, @WagerId, @TransactionTypeId, @ExternalReferenceId)";

                        await connection.ExecuteAsync(insertTransactionSql, new
                        {
                            wagerDto.TransactionId,
                            wagerDto.WagerId,
                            wagerDto.TransactionTypeId,
                            wagerDto.ExternalReferenceId
                        }, transaction);

                        // Commit transaction after successful inserts
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error saving wager", ex);
                    }
                }
            }
        }
    }

}
