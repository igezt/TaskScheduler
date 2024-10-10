using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;

namespace TaskScheduler.Services
{
    public class ConsulService
    {
        private readonly Dictionary<int, string> instances = [];
        private readonly IConsulClient _consulClient;
        private readonly ILogger<ConsulService> _logger;

        private readonly string? _id;
        private readonly int _port;

        private readonly string _host = "host.docker.internal";

        public ConsulService(IConsulClient consulClient, ILogger<ConsulService> logger) {
            instances = [];
            _consulClient = consulClient;
            _logger = logger;

            var id = Environment.GetEnvironmentVariable("LAUNCH_PROFILE");
            if (!string.IsNullOrEmpty(id)) _id = id;

            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port)) _port = int.Parse(port);
        }

        private readonly string serviceName = "TaskSchedulerNode";

        public async Task<List<string>> GetIds() {
            var services = await _consulClient.Agent.Services();
            var allServiceIds = services.Response.Values.Select(s => s.ID).ToList();
            _logger.LogInformation("All services: " + string.Join(" ", allServiceIds));

            var healthyServices = await _consulClient.Health.Service(serviceName, null, passingOnly: true);
            // Extract the service IDs from the response
            var serviceIds = healthyServices.Response.Select(s => s.Service.ID).ToList();

            return serviceIds;
        }

        public async Task RegisterService() {
            // var heartbeatUrl = $"http://localhost:5001/api/heartbeat";
            var heartbeatUrl = $"http://{_host}:{_port}/api/heartbeat";
            var registration = new AgentServiceRegistration
            {
                ID = _id,
                Name = serviceName,
                Address = _host,
                Port = _port,
                Check = new AgentServiceCheck()
                    {
                        HTTP = heartbeatUrl,
                        Interval = TimeSpan.FromSeconds(10),
                        Timeout = TimeSpan.FromSeconds(5)
                    },
                Tags = [$"Node {_id}",]

            };

            await _consulClient.Agent.ServiceRegister(registration);
            _logger.LogInformation("{string} registered with Consul on {string}:{int}. Heartbeat url: {string}", serviceName, _host, _port, heartbeatUrl);
        }
        public async Task DeregisterService()
        {
            await _consulClient.Agent.ServiceDeregister(_id);
            _logger.LogInformation("Service {int} deregistered from Consul", _id);
        }
    }
}