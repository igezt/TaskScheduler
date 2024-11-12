using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Consul;
using Polly;
using Polly.Retry;

namespace TaskScheduler.Discovery
{
    public class ConsulDiscoveryService : IDiscoveryService
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<ConsulDiscoveryService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        private readonly string? _id;
        private readonly int _port;

        private readonly string _host = "localhost";
        private readonly string _serviceName = "TaskSchedulerNode";

        public ConsulDiscoveryService(
            IConsulClient consulClient,
            ILogger<ConsulDiscoveryService> logger
        )
        {
            _consulClient = consulClient;
            _logger = logger;

            var retryTime = TimeSpan.FromSeconds(5);

            _retryPolicy = Polly
                .Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    100,
                    retryAttempt => retryTime,
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var errorMsg = exception.InnerException.ToString();
                        _logger.LogError(
                            "\n{string} Retry {int}: Node is not able to connect to Consul. Re-attempting in {string} seconds.",
                            errorMsg,
                            retryCount,
                            retryTime.ToString()
                        );
                    }
                );

            var id = Environment.GetEnvironmentVariable("NODE_ID");
            if (!string.IsNullOrEmpty(id))
                _id = id;

            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port))
                _port = int.Parse(port);
        }

        public async Task<List<int>> GetHealthyIds()
        {
            var healthyServices = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _consulClient.Health.Service(_serviceName, null, passingOnly: true);
            });
            // Extract the service IDs from the response
            var serviceIds = healthyServices.Response.Select(s => int.Parse(s.Service.ID)).ToList();

            return serviceIds;
        }

        public async Task<string> GetNodeAddress(int nodeId)
        {
            _logger.LogInformation("Getting address for node " + nodeId.ToString());
            // Fetch all registered services
            var services = await _consulClient.Agent.Services();

            // Find the service with the matching ID
            var service = services.Response.Values.FirstOrDefault(s => s.ID == nodeId.ToString());

            // If service is found, return its address and port
            if (service != null)
            {
                return $"http://{_host}:{service.Port}";
            }
            else
            {
                throw new Exception("No service with this id was found.");
            }
        }

        public async Task<bool> RegisterNode()
        {
            var heartbeatUrl = $"http://host.docker.internal:{_port}/api/heartbeat";
            var registration = new AgentServiceRegistration
            {
                ID = _id,
                Name = _serviceName,
                Address = _host,
                Port = _port,
                Check = new AgentServiceCheck()
                {
                    HTTP = heartbeatUrl,
                    Interval = TimeSpan.FromSeconds(0.5),
                    Timeout = TimeSpan.FromSeconds(5),
                },
                Tags = [$"Node {_id}"],
            };
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                await _consulClient.Agent.ServiceRegister(registration);
                _logger.LogInformation(
                    "{string} registered with Consul on {string}:{int}. Heartbeat url: {string}",
                    _serviceName,
                    _host,
                    _port,
                    heartbeatUrl
                );
                return true;
            });
        }

        public async Task DeregisterNode()
        {
            await _consulClient.Agent.ServiceDeregister(_id);
            _logger.LogInformation("Service {int} deregistered from Consul", _id);
        }

        public async Task<bool> IsNodeHealthy()
        {
            var healthyNodeIds = await GetHealthyIds();
            var isHealthy = healthyNodeIds.Any(nodeId => nodeId == int.Parse(_id));
            // Return the node name, or null if not found
            _logger.LogWarning("isHealthy: {bool}", isHealthy);

            return isHealthy;
        }
    }
}
