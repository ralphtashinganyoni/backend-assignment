using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using OT.Assessment.Consumer.Data.DTOs;
using OT.Assessment.Consumer.Services;

namespace OT.Assessment.Consumer.RabbitMq
{
    public class MessageConsumer : BackgroundService
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MessageConsumer(ILogger<MessageConsumer> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _channel.QueueDeclare(queue: "casino_wager_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Consumer started");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message: {0}", message);

                // Deserialize the message
                var wager = JsonSerializer.Deserialize<WagerDto>(message);

                if (wager != null)
                {
                    // Save to database
                    await SaveToDatabase(wager);
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "casino_wager_queue", autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task SaveToDatabase(WagerDto wager)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IWagerRepository>();
                await dbContext.SaveWager(wager);
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
