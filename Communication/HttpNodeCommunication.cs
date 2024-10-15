using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Fallback;
using TaskScheduler.Interfaces;

namespace TaskScheduler.Communication
{
    public class HttpNodeCommunication : INodeCommunication
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncFallbackPolicy<bool> _fallbackPolicy;
        private readonly ILogger<HttpNodeCommunication> _logger;

        public HttpNodeCommunication(HttpClient httpClient, ILogger<HttpNodeCommunication> logger)
        {
            _httpClient = httpClient;
            _fallbackPolicy = Policy<bool>
                .Handle<HttpRequestException>()
                .FallbackAsync(
                    fallbackValue: false // Fallback value if exception occurs
                );
            _logger = logger;
        }

        public async Task<bool> FloodIdToNode(string nodeUrl, int id)
        {
            return await _fallbackPolicy.ExecuteAsync(async () =>
            {
                var res = await _httpClient.PostAsJsonAsync(nodeUrl + $"/api/leader/flood-id", id);
                if (!res.IsSuccessStatusCode)
                {
                    return false;
                }
                else
                {
                    // _logger.LogInformation("newLeaderId {int} is not the same leader id in node {id}", newLeaderId, id);
                    return true;
                }
            });
        }

        public async Task<bool> PollNode(string nodeUrl)
        {
            return await _fallbackPolicy.ExecuteAsync(async () =>
            {
                // _logger.LogInformation("Polling node at {string}", nodeUrl);
                var res = await _httpClient.GetAsync(nodeUrl + "/api/heartbeat");
                if (!res.IsSuccessStatusCode)
                {
                    // _logger.LogInformation("Polling node at {string}: Not successful", nodeUrl);
                    return false;
                }
                else
                {
                    // _logger.LogInformation("Polling node at {string}: Successful", nodeUrl);
                    return true;
                }
            });
        }

        public async Task<bool> SignalToFloodId(string nodeUrl)
        {
            return await _fallbackPolicy.ExecuteAsync(async () =>
            {
                var res = await _httpClient.GetAsync(nodeUrl + $"/api/leader");
                if (!res.IsSuccessStatusCode)
                {
                    return false;
                }
                else
                {
                    // _logger.LogInformation("newLeaderId {int} is not the same leader id in node {id}", newLeaderId, id);
                    return true;
                }
            });
        }
    }
}
