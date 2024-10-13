using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using TaskScheduler.Interfaces;

namespace TaskScheduler.Services
{
    public class ConsulService : IDiscoveryService 
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<ConsulService> _logger;

        private readonly string? _id;
        private readonly int _port;

        private readonly string _host = "localhost";
        private readonly string _serviceName = "TaskSchedulerNode";

        public ConsulService(IConsulClient consulClient, ILogger<ConsulService> logger) {
            _consulClient = consulClient;
            _logger = logger;

            var id = Environment.GetEnvironmentVariable("NODE_ID");
            if (!string.IsNullOrEmpty(id)) _id = id;

            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port)) _port = int.Parse(port);
        }


        public async Task<List<int>> GetHealthyIds() {
            var services = await _consulClient.Agent.Services();
            var allServiceIds = services.Response.Values.Select(s => s.ID).ToList();
            _logger.LogInformation("All services: {string}", string.Join(" ", allServiceIds));

            var healthyServices = await _consulClient.Health.Service(_serviceName, null, passingOnly: true);
            // Extract the service IDs from the response
            var serviceIds = healthyServices.Response.Select(s => int.Parse(s.Service.ID)).ToList();

            return serviceIds;
        }

        public async Task<string> GetServiceAddress(int id) {
            // Fetch all registered services
            var services = await _consulClient.Agent.Services();

            // Find the service with the matching ID
            var service = services.Response.Values.FirstOrDefault(s => s.ID == id.ToString());

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

        public async Task<bool> RegisterNode() {
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
                        Timeout = TimeSpan.FromSeconds(5)
                    },
                Tags = [$"Node {_id}",]
            };
            try {
                await _consulClient.Agent.ServiceRegister(registration);
                _logger.LogInformation("{string} registered with Consul on {string}:{int}. Heartbeat url: {string}", _serviceName, _host, _port, heartbeatUrl);
                return true;
            } catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused")) {
                return false;
            }
        }
        public async Task DeregisterNode()
        {
            await _consulClient.Agent.ServiceDeregister(_id);
            _logger.LogInformation("Service {int} deregistered from Consul", _id);
        }

        public async Task<bool> IsNodeHealthy() {
            var healthyNodeIds = await GetHealthyIds();
            var isHealthy = healthyNodeIds.Any(nodeId => nodeId == int.Parse(_id));
            // Return the node name, or null if not found
            _logger.LogWarning("isHealthy: {bool}", isHealthy);

            return isHealthy;
        }

        

        
    }
}