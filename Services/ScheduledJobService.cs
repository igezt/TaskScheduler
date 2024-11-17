using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskScheduler.Coordinator;
using TaskScheduler.Election;
using TaskScheduler.Queue;

public class ScheduledJobService : BackgroundService
{
    private readonly ILogger<ScheduledJobService> _logger;
    private readonly ICoordinator _nodeCoordinator;

    private readonly int id;

    public ScheduledJobService(ILogger<ScheduledJobService> logger, ICoordinator nodeCoordinator)
    {
        _logger = logger;
        _nodeCoordinator = nodeCoordinator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // _logger.LogInformation("Starting scheduled job.");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "{string} Scheduled job running at: {time}",
                Environment.GetEnvironmentVariable("NODE_ID"),
                DateTimeOffset.Now
            );
            _nodeCoordinator.RunNodeRole(stoppingToken);

            // Your scheduled job logic here
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // Run every 5 seconds.
        }
    }
}
