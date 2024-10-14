using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Fallback;
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

        private readonly IElectionStrategy _electionStrategy;

        private readonly INodeCommunication _nodeCommunication;
        private readonly string _fromSammy = "iloveuuuuu";

        public ElectionService(
            HttpClient httpClient,
            ILogger<ElectionService> logger,
            IDiscoveryService discoveryService,
            IElectionStrategy electionStrategy,
            INodeCommunication nodeCommunication
        )
        {
            _logger = logger;

            _id = int.Parse(Environment.GetEnvironmentVariable("NODE_ID"));
            leaderId = -1;
            _discoveryService = discoveryService;
            _electionStrategy = electionStrategy;
            _nodeCommunication = nodeCommunication;
        }

        public async Task<bool> IsSelfLeader()
        {
            if (leaderId == -1)
            {
                await BlockingRegistration();
                await InitLeader();
            }
            _logger.LogInformation(
                "Healthy node ids: {string}",
                string.Join(" ", await _discoveryService.GetHealthyIds())
            );
            return leaderId == _id;
        }

        public async Task<bool> PollOrReElectLeaderAsync()
        {
            if (leaderId == -1)
            {
                await BlockingRegistration();
                await InitLeader();
            }
            var isLeaderOnline = await PollNodeAsync(leaderId);
            if (!isLeaderOnline)
            {
                ReElectLeader();
            }
            return isLeaderOnline;
        }

        /**
        *   Called to poll all nodes to flood their ids to one another to re-elect a leader.
        */
        private async Task<bool> InitLeader()
        {
            leaderId = _id;
            var healthyServicesIds = await _discoveryService.GetHealthyIds();
            _logger.LogWarning(
                "Initializing leader. Sending flood signal to the following nodes: {string}",
                string.Join(", ", healthyServicesIds)
            );
            foreach (var id in healthyServicesIds)
            {
                await SignalToFloodId(id);
            }
            return true;
        }

        private async Task<bool> BlockingRegistration()
        {
            await _discoveryService.RegisterNode();
            // TODO: Remove busy waiting.
            while (!await _discoveryService.IsNodeHealthy())
            {
                _logger.LogWarning("Node is currently not registered as healthy in Consul");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            return true;
        }

        /**
        *   Floods the current node's id to all other nodes, including itself.
        */
        public async Task<bool> FloodId()
        {
            var ids = await _discoveryService.GetHealthyIds();
            foreach (var id in ids)
            {
                await FloodIdToNode(id);
            }
            return true;
        }

        public void UpdateLeaderId(int id)
        {
            _logger.LogInformation("New id found: {int}", id);
            leaderId = _electionStrategy.ElectLeader(leaderId, id);
        }

        private async void ReElectLeader()
        {
            leaderId = -1;
            await FloodId();
        }

        private async Task<bool> PollNodeAsync(int id)
        {
            var nodeUrl = await _discoveryService.GetNodeAddress(id);

            return await _nodeCommunication.PollNode(nodeUrl);
        }

        private async Task<bool> SignalToFloodId(int id)
        {
            var nodeUrl = await _discoveryService.GetNodeAddress(id);
            return await _nodeCommunication.SignalToFloodId(nodeUrl);
        }

        private async Task<bool> FloodIdToNode(int id)
        {
            var nodeUrl = await _discoveryService.GetNodeAddress(id);
            return await _nodeCommunication.FloodIdToNode(nodeUrl, _id);
        }
    }
}
