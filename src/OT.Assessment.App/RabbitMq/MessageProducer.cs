using Microsoft.Extensions.Options;
using OT.Assessment.Common.RabbitMq.Config;
using OT.Assessment.Common.RabbitMq.Connection;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OT.Assessment.App.RabbitMq
{
    public class MessageProducer : IMessageProducer
    {
        private readonly IRabbitMqConnection _connection;
        private readonly ILogger<MessageProducer> _logger;
        private readonly RabbitMqConfigSettings _rabbitMqSettings;


        public MessageProducer(
            IRabbitMqConnection connection, 
            ILogger<MessageProducer> logger,
            IOptions<RabbitMqConfigSettings> rabbitMqSettings)
        {
            _connection = connection;
            _logger = logger;
            _rabbitMqSettings = rabbitMqSettings.Value;

        }

        public async Task SendMessage<T>(T message)
        {
            if (message == null)
            {
                _logger.LogError("Attempted to send a null message.");
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");
            }

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _rabbitMqSettings.HostName, 
                    Port = _rabbitMqSettings.Port, 
                    UserName = _rabbitMqSettings.UserName,
                    Password = _rabbitMqSettings.Password,
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(_rabbitMqSettings.Exchange, ExchangeType.Direct);
                channel.QueueDeclare(
                    queue: _rabbitMqSettings.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                channel.QueueBind(_rabbitMqSettings.Queue, _rabbitMqSettings.Exchange, _rabbitMqSettings.RoutingKey, null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                await Task.Run(() =>
                {
                    channel.BasicPublish(
                        exchange: _rabbitMqSettings.Exchange,
                        routingKey: _rabbitMqSettings.RoutingKey,
                        basicProperties: null,
                        body: body);
                    _logger.LogInformation("Message sent to RabbitMQ: {MessageType} with routing key: {RoutingKey}", typeof(T).Name, "wager-routing-key-1");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message of type {MessageType} to RabbitMQ.", typeof(T).Name);
                throw; 
            }
        }
    }
}
