using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Fallback;
using TaskScheduler.Discovery;

namespace TaskScheduler.Communication
{
    public class HttpNodeCommunication : INodeCommunication
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncFallbackPolicy<bool> _fallbackPolicy;
        private readonly ILogger<HttpNodeCommunication> _logger;
        private readonly IDiscoveryService _discoveryService;

        public HttpNodeCommunication(
            HttpClient httpClient,
            ILogger<HttpNodeCommunication> logger,
            IDiscoveryService discoveryService
        )
        {
            _httpClient = httpClient;
            _fallbackPolicy = Policy<bool>
                .Handle<HttpRequestException>()
                .FallbackAsync(
                    fallbackValue: false // Fallback value if exception occurs
                );
            _discoveryService = discoveryService;
            _logger = logger;
        }

        public async Task<bool> BroadcastElectionMessage(object message)
        {
            return await _fallbackPolicy.ExecuteAsync(async () =>
            {
                var ids = await _discoveryService.GetHealthyIds();
                Task<bool>[] tasks = ids.Select(async id =>
                    {
                        var nodeUrl = _discoveryService.GetNodeAddress(id);

                        var res = await _httpClient.PostAsJsonAsync(
                            nodeUrl + $"/api/election",
                            message
                        );
                        return res.IsSuccessStatusCode;
                    })
                    .ToArray();
                var results = await Task.WhenAll(tasks);
                bool allReceived = results.All(res => res);

                return allReceived;
            });
        }

        public async Task<bool> PollNode(int nodeId)
        {
            return await _fallbackPolicy.ExecuteAsync(async () =>
            {
                var nodeUrl = await _discoveryService.GetNodeAddress(nodeId);
                var res = await _httpClient.GetAsync(nodeUrl + "/api/heartbeat");
                return res.IsSuccessStatusCode;
            });
        }

        public async Task<dynamic> SendElectionMessageToNode(int nodeId, object message)
        {
            // return await _fallbackPolicy.ExecuteAsync(async () =>
            // {

            // });
            var nodeUrl = await _discoveryService.GetNodeAddress(nodeId);

            var res = await _httpClient.PostAsJsonAsync(nodeUrl + $"/api/election", message);
            return res;
        }
    }
}
