using OT.Assessment.App.Model.DTOs;
using OT.Assessment.App.Model.Repository;
using OT.Assessment.App.RabbitMq;
using OT.Assessment.Common.Data.DTOs;

namespace OT.Assessment.App.Services
{
    public class PlayerWagerService : IPlayerWagerService
    {
        private readonly IMessageProducer _messageProducer;
        private readonly IWagerRepository _wagerRepository;
        private readonly ILogger<PlayerWagerService> _logger;

        public PlayerWagerService(IMessageProducer queuePublisher, IWagerRepository wagerRepository, ILogger<PlayerWagerService> logger)
        {
            _messageProducer = queuePublisher;
            _wagerRepository = wagerRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<WagerSummaryDto>> GetPlayerWagersAsync(Guid playerId, int pageSize, int pageNumber)
        {
            _logger.LogInformation("Retrieving wagers for player {PlayerId} with page size {PageSize} and page number {PageNumber}.", playerId, pageSize, pageNumber);

            try
            {
                var wagers = await _wagerRepository.GetPlayerWagersAsync(playerId, pageSize, pageNumber);
                _logger.LogInformation("Successfully retrieved {Count} wagers for player {PlayerId}.", wagers.Count(), playerId);
                return wagers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wagers for player {PlayerId}.", playerId);
                throw; 
            }
        }

        public async Task<IEnumerable<TopSpenderDto>> GetTopSpendersAsync(int count)
        {
            _logger.LogInformation("Retrieving top {Count} spenders.", count);

            try
            {
                var topSpenders = await _wagerRepository.GetTopSpendersAsync(count);
                _logger.LogInformation("Successfully retrieved top spenders count: {Count}.", topSpenders.Count());
                return topSpenders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top spenders.");
                throw; 
            }
        }

        public void PublishWagerToQueue(WagerDto wager)
        {
            if (wager == null)
            {
                _logger.LogError("Attempted to publish a null wager to the queue.");
                throw new ArgumentNullException(nameof(wager), "Wager cannot be null");
            }

            _logger.LogInformation("Publishing wager {WagerId} to the message queue.", wager.WagerId);

            try
            {
                _messageProducer.SendMessage<WagerDto>(wager);
                _logger.LogInformation("Successfully published wager {WagerId} to the message queue.", wager.WagerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing wager {WagerId} to the message queue.", wager.WagerId);
                throw; 
            }
        }
    }

}
