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

        private readonly ConsulService _consulService;

        private readonly HttpClient _httpClient;

        public ElectionService(HttpClient httpClient, ILogger<ElectionService> logger, ConsulService consulService) {
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
            _consulService = consulService;  
        }

        
        
        public async Task<bool> IsSelfLeader() {
            if (leaderId == -1) {
                await InitLeader();
            }
            _logger.LogInformation("Ids: " + string.Join(" ", await _consulService.GetIds()));
            return leaderId == _id;
        }

        public async Task<bool> PollLeaderAsync() {
            if (leaderId == -1) {
                await InitLeader();
            }
            var isLeaderOnline = await PollNodeAsync(leaderId);
            if (!isLeaderOnline) {
                ReElectLeader();
            }
            return isLeaderOnline;
        }

        /**
        *   Called to poll all nodes to flood their ids to one another to re-elect a leader.
        */
        private async Task<bool> InitLeader() {
            leaderId = _id;
            foreach (var kvPair in instances) {
                await SignalToFloodId(kvPair.Key);
            }
            return true;
        }

        /**
        *   Floods the current node's id to all other nodes.
        */
        public async Task<bool> FloodId() {
            foreach(var kvPair in instances) {
                await FloodIdToNode(kvPair.Key);
            }
            return true;
        }

        /**
        *   Re-elects a leader by flooding the current node's id to all other nodes. To be called when the leader node is offline.
        */
        

        public void UpdateLeaderId(int id) {
            _logger.LogInformation("New id found: {int}", id);
            leaderId = int.Max(id, leaderId);
        }

        private async void ReElectLeader() {
            leaderId = -1;
            await FloodId();
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