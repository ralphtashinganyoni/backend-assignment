using Microsoft.Extensions.Options;
using OT.Assessment.Common.Data.DTOs;
using OT.Assessment.Common.RabbitMq.Config;
using OT.Assessment.Consumer.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OT.Assessment.Consumer.RabbitMq
{
    public class MessageConsumer : BackgroundService
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IModel _channel;
        private IConnection _connection;
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqConfigSettings _rabbitMqSettings;


        public MessageConsumer(
            ILogger<MessageConsumer> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<RabbitMqConfigSettings> rabbitMqSettings)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _rabbitMqSettings = rabbitMqSettings.Value;

            _factory = new ConnectionFactory()
            {
                HostName = _rabbitMqSettings.HostName,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password,
                DispatchConsumersAsync = true
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _logger.LogInformation("MessageConsumer service is stopping"));

            _logger.LogInformation("Starting RabbitMQ Consumer");
            try
            {
                await InitializeRabbitMQ(stoppingToken);

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
                            await SaveToDatabaseAsync(wager, stoppingToken);
                        }

                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Message acknowledged successfully.");
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to deserialize message: {Message}", message);
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message: {Message}", message);
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                _channel.BasicConsume(queue: "casino_wager_queue", autoAck: false, consumer: consumer);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing RabbitMQ consumer.");
                throw;
            }
        }

        private async Task InitializeRabbitMQ(CancellationToken stoppingToken)
        {
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: _rabbitMqSettings.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("RabbitMQ connection and channel established successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection.");
                throw;
            }

            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Cancellation requested, closing RabbitMQ connection.");
                _channel?.Close();
                _connection?.Close();
            });
        }

        private async Task SaveToDatabaseAsync(WagerDto wager, CancellationToken stoppingToken)
        {
            if (wager == null)
            {
                _logger.LogWarning("Attempted to save a null WagerDto.");
                return;
            }

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var wagerRepository = scope.ServiceProvider.GetRequiredService<IWagerRepository>();

                    // Respect cancellation token
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Cancellation requested before saving to database.");
                        return;
                    }

                    await wagerRepository.SaveWager(wager);
                    _logger.LogInformation("Wager {WagerId} saved to database successfully.", wager.WagerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Wager {WagerId} to database.", wager.WagerId);
                throw;
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
