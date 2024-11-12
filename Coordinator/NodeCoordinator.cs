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
        private readonly SemaphoreSlim stateSem;

        public int leaderId;
        public readonly int _id;

        private NodeState _nodeState;
        private readonly ILogger<NodeCoordinator> _logger;

        private readonly IElectionManager _electionManager;

        public NodeCoordinator(ILogger<NodeCoordinator> logger, IElectionManager electionManager)
        {
            stateSem = new SemaphoreSlim(1);
            _logger = logger;
            _electionManager = electionManager;
            _nodeState = NodeState.IDLE;
            _id = int.Parse(Environment.GetEnvironmentVariable("NODE_ID"));
            leaderId = 2;
        }

        // Event-handler

        // Handles leader declared events.
        public void HandleLeaderDeclared(object sender, LeaderDeclaredEventArgs e)
        {
            leaderId = e.LeaderID;
        }

        // Handles state transition events
        public void HandleStateTransition(object sender, StateTransitionEventArgs e)
        {
            stateSem.Wait();
            _nodeState = e.NewState;
            stateSem.Release();
        }

        // Election-related methods

        // public void StartElection()
        // {
        //     _electionManager.StartElection();
        // }

        public int GetLeaderId()
        {
            return leaderId;
        }

        public int GetId()
        {
            return _id;
        }

        public async Task<bool> PollOrReElectLeader(CancellationToken token)
        {
            return await _electionManager.TryAcquireLeadership(token);
        }

        public async Task<bool> AmILeader(CancellationToken token)
        {
            return await _electionManager.IsLeader(token);
        }

        public Task HandleElectionMessage(int senderId, object payload)
        {
            // stateSem.Wait();
            // var res = _electionManager.HandleElectionMessage(senderId, payload, _nodeState);
            // stateSem.Release();
            // return res;
            throw new NotImplementedException();
        }

        // Task-processing methods
        public void CompleteTask()
        {
            throw new NotImplementedException();
        }

        public void ProcessTaskFromQueue()
        {
            throw new NotImplementedException();
        }
    }
}
