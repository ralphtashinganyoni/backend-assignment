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

        public PlayerWagerService(IMessageProducer queuePublisher, IWagerRepository wagerRepository)
        {
            _messageProducer = queuePublisher;
            _wagerRepository = wagerRepository;
        }

        public async Task<IEnumerable<WagerSummaryDto>> GetPlayerWagersAsync(Guid playerId, int pageSize, int pageNumber)
        {
            return await _wagerRepository.GetPlayerWagersAsync(playerId, pageSize, pageNumber);
        }

        public async Task<IEnumerable<TopSpenderDto>> GetTopSpendersAsync(int count)
        {
            return await _wagerRepository.GetTopSpendersAsync(count);
        }

        public void PublishWagerToQueue(WagerDto wager)
        {
            _messageProducer.SendMessage<WagerDto>(wager);
        }
    }
}
