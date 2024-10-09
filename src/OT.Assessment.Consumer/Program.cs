using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OT.Assessment.Common.RabbitMq.Config;
using OT.Assessment.Consumer.RabbitMq;
using OT.Assessment.Consumer.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqConfigSettings>(
        context.Configuration.GetSection("RabbitMq"));

        services.AddSingleton(a => a.GetRequiredService<IOptions<RabbitMqConfigSettings>>());
        services.AddHostedService<MessageConsumer>(); 
        services.AddSingleton<IWagerRepository, WagerRepository>(); 
        services.AddLogging(config => config.AddConsole());
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started {time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

await host.RunAsync();

logger.LogInformation("Application ended {time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);