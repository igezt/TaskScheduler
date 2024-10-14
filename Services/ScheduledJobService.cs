using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskScheduler.Services;

public class ScheduledJobService : BackgroundService
{
    private readonly ILogger<ScheduledJobService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ElectionService _electionService;

    private readonly int id;

    public ScheduledJobService(
        ILogger<ScheduledJobService> logger,
        HttpClient httpClient,
        ElectionService electionService
    )
    {
        _logger = logger;
        _httpClient = httpClient;
        _electionService = electionService;
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

            var isLeader = await _electionService.IsSelfLeader();

            if (isLeader)
            {
                _logger.LogInformation("I am the leader");
                // TODO: Produce tasks to Kafka
            }
            else
            {
                var isLeaderOnline = await _electionService.PollOrReElectLeaderAsync();
                if (isLeaderOnline)
                {
                    _logger.LogInformation("Leader is online.");
                    // TODO: Receive tasks from Kafka
                }
            }

            // Your scheduled job logic here
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // Run every 5 seconds.
        }
    }
}
