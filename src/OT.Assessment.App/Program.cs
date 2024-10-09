using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OT.Assessment.App.Model.Repository;
using OT.Assessment.App.RabbitMq;
using OT.Assessment.App.Services;
using OT.Assessment.Common.RabbitMq.Config;
using OT.Assessment.Common.RabbitMq.Connection;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckl
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.Configure<RabbitMqConfigSettings>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton(a => a.GetRequiredService<IOptions<RabbitMqConfigSettings>>());

builder.Services.AddSingleton<IRabbitMqConnection>( new RabbitMqConnection());
builder.Services.AddScoped<IMessageProducer, MessageProducer>();
builder.Services.AddTransient<IWagerRepository, WagerRepository>();
builder.Services.AddScoped<IPlayerWagerService, PlayerWagerService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
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
