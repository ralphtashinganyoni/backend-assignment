using RabbitMQ.Client;

namespace OT.Assessment.Common.RabbitMq.Connection
{
    public interface IRabbitMqConnection
    {
        IConnection Connection { get; }
    }
}
