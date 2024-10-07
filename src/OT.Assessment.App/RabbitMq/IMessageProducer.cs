namespace OT.Assessment.App.RabbitMq
{
    public interface IMessageProducer
    {
        Task SendMessage<T>(T message);
    }
}
