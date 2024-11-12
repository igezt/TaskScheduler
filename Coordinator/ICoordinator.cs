using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskScheduler.Election;

namespace TaskScheduler.Coordinator
{
    public interface ICoordinator
    {
        // Event handlers
        void HandleLeaderDeclared(object sender, LeaderDeclaredEventArgs e);
        void HandleStateTransition(object sender, StateTransitionEventArgs e);

        // Election-related methods
        int GetLeaderId();
        int GetId();

        // void StartElection();

        Task HandleElectionMessage(int senderId, object payload);

        Task<bool> PollOrReElectLeader(CancellationToken token);

        Task<bool> AmILeader(CancellationToken token);

        /** Task processing methods */
        void ProcessTaskFromQueue();
        void CompleteTask();
    }
}
