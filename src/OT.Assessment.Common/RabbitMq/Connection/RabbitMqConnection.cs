﻿using RabbitMQ.Client;

namespace OT.Assessment.Common.RabbitMq.Connection
{
    public class RabbitMqConnection : IRabbitMqConnection, IDisposable
    {
        private IConnection _connection;
        public IConnection Connection => _connection;

        public RabbitMqConnection()
        {
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };
            //factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
            _connection = factory.CreateConnection();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
