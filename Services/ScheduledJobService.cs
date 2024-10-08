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
    private readonly ElectionService _leaderService;

    private readonly int id;


    public ScheduledJobService(ILogger<ScheduledJobService> logger, HttpClient httpClient, ElectionService leaderService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _leaderService = leaderService;        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // _logger.LogInformation("Starting scheduled job.");
        await _leaderService.RequestIdFromNodes();
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("{string} Scheduled job running at: {time}", 
                Environment.GetEnvironmentVariable("LAUNCH_PROFILE"), DateTimeOffset.Now
            );


            var isLeader = _leaderService.IsSelfLeader();

            if (isLeader) {
                _logger.LogInformation("I am the leader");
            } else {
                var isLeaderOnline = await _leaderService.PollLeaderAsync();
                if (isLeaderOnline) {
                    _logger.LogInformation("Leader is online.");
                } else {
                    _leaderService.ElectLeader();
                }
            }
            
            // Your scheduled job logic here
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // Run every 5 seconds.
        }
    }
}
