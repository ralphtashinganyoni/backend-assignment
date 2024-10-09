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

        public MessageProducer(IRabbitMqConnection connection, ILogger<MessageProducer> logger)
        {
            _connection = connection;
            _logger = logger;
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
                    HostName = "localhost", 
                    Port = 5672, 
                    UserName = "guest",
                    Password = "guest",
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare("wager-exchange", ExchangeType.Direct);
                channel.QueueDeclare(
                    queue: "wager-1",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                channel.QueueBind("wager-1", "wager-exchange", "wager-routing-key-1", null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                await Task.Run(() =>
                {
                    channel.BasicPublish(
                        exchange: "wager-exchange-1",
                        routingKey: "wager-routing-key-1",
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
