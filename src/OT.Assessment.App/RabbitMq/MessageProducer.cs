using OT.Assessment.Common.RabbitMq.Connection;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OT.Assessment.App.RabbitMq
{
    public class MessageProducer : IMessageProducer
    {
        private readonly IRabbitMqConnection _connection;

        public MessageProducer(IRabbitMqConnection connection)
        {
            _connection = connection;
        }
        public async Task SendMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare("wager-exchange-1", ExchangeType.Direct);
            channel.QueueDeclare(
    queue: "wager-1",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);
            channel.QueueBind("wager-1", "wager-exchange-1", "wager-routing-key-1", null);
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            await Task.Run(() => channel.BasicPublish(
               exchange: "wager-exchange-1",
               routingKey: "wager-routing-key-1",
               basicProperties: null,
               body: body));
        }
    }
}
