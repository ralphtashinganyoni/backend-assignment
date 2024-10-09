using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using OT.Assessment.Consumer.Services;
using OT.Assessment.Common.Data.DTOs;

namespace OT.Assessment.Consumer.RabbitMq
{
    public class MessageConsumer : BackgroundService
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IModel _channel;
        private IConnection _connection;
        private readonly ConnectionFactory _factory;

        public MessageConsumer(ILogger<MessageConsumer> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            // Initialize RabbitMQ ConnectionFactory
            _factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true // Enable async message handling
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _logger.LogInformation("MessageConsumer service is stopping"));

            // Establish RabbitMQ connection and channel
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
                            // Save the wager to the database
                            await SaveToDatabaseAsync(wager, stoppingToken);
                        }

                        // Acknowledge message as processed
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Message acknowledged successfully.");
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to deserialize message: {Message}", message);
                        _channel.BasicNack(ea.DeliveryTag, false, false); // Reject and discard the message
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message: {Message}", message);
                        // Optionally requeue the message based on failure type
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

                _channel.QueueDeclare(queue: "casino_wager_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("RabbitMQ connection and channel established successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection.");
                throw;
            }

            // Close connections on cancellation
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
