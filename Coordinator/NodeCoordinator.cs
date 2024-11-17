using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mono.TextTemplating;
using TaskScheduler.Election;

namespace TaskScheduler.Coordinator
{
    public class NodeCoordinator : ICoordinator
    {
        public readonly int _id;

        private readonly ILogger<NodeCoordinator> _logger;

        private readonly IElectionManager _electionManager;

        public NodeCoordinator(ILogger<NodeCoordinator> logger, IElectionManager electionManager)
        {
            _logger = logger;
            _electionManager = electionManager;
            _id = int.Parse(Environment.GetEnvironmentVariable("NODE_ID"));
        }

        public async void RunNodeRole(CancellationToken token)
        {
            var isLeader = await PollOrReElectLeader(token);

            if (isLeader)
            {
                _logger.LogInformation("I am the leader");
                // TODO: Produce tasks to Kafka
                // _kafkaProducer.ProduceMessage();
            }
            else
            {
                _logger.LogInformation("Leader is online.");
                // TODO: Receive tasks from Kafka
                // _kafkaConsumer.ConsumeMessage();
            }
        }

        // Election-related methods

        private async Task<bool> PollOrReElectLeader(CancellationToken token)
        {
            return await _electionManager.TryAcquireLeadership(token);
        }
    }
}
