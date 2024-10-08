using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Services
{
    public class ElectionService
    {
        
        private int leaderId;
        private readonly int _id;
        private readonly Dictionary<int, string> instances;
        private readonly ILogger<ElectionService> _logger;

        private readonly HttpClient _httpClient;

        public ElectionService(HttpClient httpClient, ILogger<ElectionService> logger) {
            instances = new Dictionary<int, string>
                {
                    [1] = "5001",
                    [2] = "5002",
                    [3] = "5003"
                };
            _logger = logger;
            _httpClient = httpClient;
            _id = int.Parse(Environment.GetEnvironmentVariable("LAUNCH_PROFILE"));
            leaderId = -1;       
        }

        
        
        public bool IsSelfLeader() {
            return leaderId == _id;
        }

        public async Task<bool> PollLeaderAsync() {
            return await PollNodeAsync(leaderId);
        }

        public async Task<bool> RequestIdFromNodes() {
            foreach (var kvPair in instances) {
                await SignalToFloodId(kvPair.Key);
            }
            return true;
        }

        public async Task<bool> FloodId() {
            foreach(var kvPair in instances) {
                await FloodIdToNode(kvPair.Key);
            }
            return true;
        }

        public async void ElectLeader() {
            leaderId = -1;
            await FloodId();
        }

        public void UpdateLeaderId(int id) {
            _logger.LogInformation("New id found: {int}", id);
            leaderId = int.Max(id, leaderId);
        }

        private async Task<bool> PollNodeAsync(int id) {
            var port = instances[id];
            var nodeUrl = $"http://localhost:{port}";
            try {
                var res = await _httpClient.GetAsync(nodeUrl + "/api/heartbeat");
                if (!res.IsSuccessStatusCode)
                {
                    return false;
                } else {
                    return true;
                }
            } catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused")) {
                return false;
            }
        }

        private async Task<bool> SignalToFloodId(int id) {
            var port = instances[id];
            var nodeUrl = $"http://localhost:{port}";
            try {
                var res = await _httpClient.GetAsync(nodeUrl + $"/api/leader");
                if (!res.IsSuccessStatusCode)
                {
                    return false;
                } else {
                    // _logger.LogInformation("newLeaderId {int} is not the same leader id in node {id}", newLeaderId, id);
                    return true;
                }
            } catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused")) {
                return false;
            }
        }

        private async Task<bool> FloodIdToNode(int id) {
            var port = instances[id];
            var nodeUrl = $"http://localhost:{port}";
            try {
                var res = await _httpClient.PostAsJsonAsync(nodeUrl + $"/api/leader/flood-id", _id);
                if (!res.IsSuccessStatusCode)
                {
                    return false;
                } else {
                    // _logger.LogInformation("newLeaderId {int} is not the same leader id in node {id}", newLeaderId, id);
                    return true;
                }
            } catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused")) {
                return false;
            }
        }
    }
}