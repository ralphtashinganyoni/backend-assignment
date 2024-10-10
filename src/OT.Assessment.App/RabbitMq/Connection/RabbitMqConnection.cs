using RabbitMQ.Client;

namespace OT.Assessment.App.RabbitMq.Connection
{
    using Microsoft.Extensions.Options;
    using OT.Assessment.Common.RabbitMq.Config;
    using RabbitMQ.Client;
    using System;

    public class RabbitMqConnection : IRabbitMqConnection, IDisposable
    {
        private readonly RabbitMqConfigSettings _configSettings;
        private IConnection _connection;

        public IConnection Connection => _connection;

        public RabbitMqConnection(IOptions<RabbitMqConfigSettings> configOptions)
        {
            _configSettings = configOptions.Value ?? throw new ArgumentNullException(nameof(configOptions));
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configSettings.HostName,
                Port = _configSettings.Port,
                UserName = _configSettings.UserName,
                Password = _configSettings.Password,
            };

            _connection = factory.CreateConnection();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }

}
