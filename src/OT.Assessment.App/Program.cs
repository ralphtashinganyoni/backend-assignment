using OT.Assessment.App.RabbitMq.Connection;
using OT.Assessment.App.RabbitMq;
using OT.Assessment.Common;
using OT.Assessment.Common.RabbitMq.Config;
using System.Reflection;
using OT.Assessment.App.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

ServiceRegistration.ConfigureServices(builder.Services);
builder.Services.Configure<RabbitMqConfigSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddScoped<IMessageProducer, MessageProducer>();
builder.Services.AddScoped<IPlayerWagerService, PlayerWagerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.EnableTryItOutByDefault();
        opts.DocumentTitle = "OT Assessment App";
        opts.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
