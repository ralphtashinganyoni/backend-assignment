using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OT.Assessment.Common;
using OT.Assessment.Common.RabbitMq.Config;
using OT.Assessment.Consumer.RabbitMq;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    })
    .ConfigureServices((context, services) =>
    {
        ServiceRegistration.ConfigureServices(services);
        services.Configure<RabbitMqConfigSettings>(context.Configuration.GetSection("RabbitMq"));
        services.AddHostedService<MessageConsumer>();
        services.AddLogging(config => config.AddConsole());
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started at {Time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

await host.RunAsync();
logger.LogInformation("Application ended at {Time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);