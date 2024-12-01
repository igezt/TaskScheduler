using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Mono.TextTemplating;
using TaskScheduler.Election;
using TaskScheduler.src.Services.Tasks.Roles;

namespace TaskScheduler.Coordinator
{
    public class NodeCoordinator : ICoordinator
    {
        public readonly int _id;

        private readonly ILogger _logger;

        private readonly IElectionManager _electionManager;

        private readonly LeaderRole _leader;
        private readonly WorkerRole _worker;

        public NodeCoordinator(
            ILoggerFactory logger,
            IElectionManager electionManager,
            LeaderRole leader,
            WorkerRole worker
        )
        {
            _logger = logger.CreateLogger("NodeCoordinator");
            _electionManager = electionManager;
            _id = int.Parse(Environment.GetEnvironmentVariable("NODE_ID"));
            _leader = leader;
            _worker = worker;
        }

        public async void RunNodeRole(CancellationToken token)
        {
            var isLeader = await PollOrReElectLeader(token);

            if (isLeader)
            {
                _logger.LogInformation("I am the leader");
                // TODO: Produce tasks to Kafka
                // _kafkaProducer.ProduceMessage();
                await _leader.Perform();
            }
            else
            {
                _logger.LogInformation("Leader is online.");
                // TODO: Receive tasks from Kafka
                // _kafkaConsumer.ConsumeMessage();
                await _worker.Perform();
            }
        }

        // Election-related methods

        private async Task<bool> PollOrReElectLeader(CancellationToken token)
        {
            return await _electionManager.TryAcquireLeadership(token);
        }
    }
}
