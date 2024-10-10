using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OT.Assessment.Common.Data.DTOs;
using OT.Assessment.Common.Data.Repositories;
using OT.Assessment.Common.RabbitMq.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OT.Assessment.Consumer.RabbitMq
{
    public class MessageConsumer : BackgroundService
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly RabbitMqConfigSettings _rabbitMqSettings;
        private IServiceScopeFactory _serviceScopeFactory;
        private IConnection _connection;
        private IModel _channel;


        public MessageConsumer(
            ILogger<MessageConsumer> logger,
            IOptions<RabbitMqConfigSettings> rabbitMqSettings,
            IServiceScopeFactory serviceScopeFactory
)
        {
            _logger = logger;
            _rabbitMqSettings = rabbitMqSettings.Value;
            _serviceScopeFactory = serviceScopeFactory;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqSettings.HostName,
                Port = _rabbitMqSettings.Port,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_rabbitMqSettings.Exchange, ExchangeType.Direct);
            _channel.QueueDeclare(queue: _rabbitMqSettings.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(_rabbitMqSettings.Queue, _rabbitMqSettings.Exchange, _rabbitMqSettings.RoutingKey);

            _logger.LogInformation("RabbitMQ connection and channel established successfully.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _logger.LogInformation("MessageConsumer is stopping."));

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Cancellation requested, stopping message processing.");
                    return;
                }

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message from RabbitMQ: {Message}", message);

                try
                {
                    var wager = JsonSerializer.Deserialize<WagerDto>(message);
                    if (wager != null)
                    {
                        await ProcessMessageAsync(wager, stoppingToken);
                    }

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Message acknowledged successfully.");
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize message: {Message}", message);
                    _channel.BasicNack(ea.DeliveryTag, false, false); // Discard the message
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", message);
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue the message
                }
            };

            _channel.BasicConsume(queue: _rabbitMqSettings.Queue, autoAck: false, consumer: consumer);

            await Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(WagerDto wager, CancellationToken stoppingToken)
        {
            if (wager == null)
            {
                _logger.LogWarning("Attempted to process a null WagerDto.");
                return;
            }

            _logger.LogInformation("Processing wager: {WagerId}", wager.WagerId);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var wagerRepository = scope.ServiceProvider.GetRequiredService<IWagerRepository>();
                await wagerRepository.SaveWager(wager);
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
            _logger.LogInformation("RabbitMQ resources disposed.");
        }
    }


}
