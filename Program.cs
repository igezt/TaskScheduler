using Consul;
using TaskScheduler.Communication;
using TaskScheduler.Interfaces;
using TaskScheduler.Queue;
using TaskScheduler.Services;
using TaskScheduler.Strategy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
    // Replace with the appropriate Consul address
    consulConfig.Address = new Uri("http://localhost:8500");
}));

builder.Services.AddSingleton<KafkaConsumer>();
builder.Services.AddSingleton<KafkaProducer>();

builder.Services.AddSingleton<IElectionStrategy, MaxIdElectionStrategy>();
builder.Services.AddSingleton<INodeCommunication, HttpNodeCommunication>();
builder.Services.AddSingleton<IDiscoveryService, ConsulService>();
builder.Services.AddSingleton<ElectionService>();
builder.Services.AddHostedService<ScheduledJobService>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

// Map controller routes
app.MapControllers();

var port = int.Parse(Environment.GetEnvironmentVariable("PORT"));

app.Run($"http://localhost:{port}");
