using Consul;
using TaskScheduler.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
    // Replace with the appropriate Consul address
    consulConfig.Address = new Uri("http://localhost:8500");
}));

builder.Services.AddSingleton<ConsulService>();
builder.Services.AddSingleton<ElectionService>();
builder.Services.AddHostedService<ScheduledJobService>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Registers the node with Consul.
var consulService = app.Services.GetRequiredService<ConsulService>();
await consulService.RegisterService();

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

