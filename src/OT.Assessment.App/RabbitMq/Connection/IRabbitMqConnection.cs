using RabbitMQ.Client;

namespace OT.Assessment.App.RabbitMq.Connection
{
    public interface IRabbitMqConnection
    {
        IConnection Connection { get; }
    }
}
