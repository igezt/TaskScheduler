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

        public HttpNodeCommunication(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _fallbackPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<bool>(res => res) // Handle cases where the result is null (e.g., bad request)
                .FallbackAsync(
                    fallbackValue: false // Fallback value if exception occurs
                );
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
                var res = await _httpClient.GetAsync(nodeUrl + "/api/heartbeat");
                if (!res.IsSuccessStatusCode)
                {
                    return false;
                }
                else
                {
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
