using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskScheduler.Interfaces;
using TaskScheduler.Strategy;

namespace TaskScheduler.Services
{
    public class ElectionService
    {
        
        private int leaderId;
        private readonly int _id;
        private readonly ILogger<ElectionService> _logger;

        private readonly IDiscoveryService _discoveryService;

        private readonly HttpClient _httpClient;

        private readonly IElectionStrategy _electionStrategy;

        private readonly string _fromSammy = "iloveuuuuu";

        public ElectionService(HttpClient httpClient, ILogger<ElectionService> logger, IDiscoveryService discoveryService, IElectionStrategy electionStrategy) {
            _logger = logger;
            _httpClient = httpClient;
            
            _id = int.Parse(Environment.GetEnvironmentVariable("NODE_ID"));
            leaderId = -1;     
            _discoveryService = discoveryService;  
            _electionStrategy = electionStrategy;

        }
        
        
        public async Task<bool> IsSelfLeader() {
            if (leaderId == -1) {
                await BlockingRegistration();
                await InitLeader();
            }
            _logger.LogInformation("Healthy node ids: {string}", string.Join(" ", await _discoveryService.GetHealthyIds()));
            return leaderId == _id;
        }

        public async Task<bool> PollOrReElectLeaderAsync() {
            if (leaderId == -1) {
                await BlockingRegistration();
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
            var healthyServicesIds = await _discoveryService.GetHealthyIds();
            _logger.LogWarning("Initializing leader. Sending flood signal to the following nodes: {string}", string.Join(", ", healthyServicesIds));
            foreach (var id in healthyServicesIds) {
                await SignalToFloodId(id);
            }
            return true;
        }

        private async Task<bool> BlockingRegistration() {
            while (!await _discoveryService.RegisterNode()) {
                _logger.LogError("Node is not able to connect to Consul. Re-attempting in 5 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            // TODO: Remove busy waiting.
            while (! await _discoveryService.IsNodeHealthy()) {
                _logger.LogWarning("Node is currently not registered as healthy in Consul");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            return true;
        }

        /**
        *   Floods the current node's id to all other nodes.
        */
        public async Task<bool> FloodId() {
            var ids = await _discoveryService.GetHealthyIds();
            foreach(var id in ids)
            {
                await FloodIdToNode(id);
            }
            return true;
        }

        /**
        *   Re-elects a leader by flooding the current node's id to all other nodes. To be called when the leader node is offline.
        */
        public void UpdateLeaderId(int id) {
            _logger.LogInformation("New id found: {int}", id);
            leaderId = _electionStrategy.ElectLeader(leaderId, id);
        }

        private async void ReElectLeader() {
            leaderId = -1;
            await FloodId();
        }

        private async Task<bool> PollNodeAsync(int id) {
            var nodeUrl = await _discoveryService.GetServiceAddress(id);
            try 
            {
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
            var nodeUrl = await _discoveryService.GetServiceAddress(id);
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
            var nodeUrl = await _discoveryService.GetServiceAddress(id);
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