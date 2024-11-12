using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Communication
{
    public interface INodeCommunication
    {
        public Task<bool> BroadcastElectionMessage(object message);
        public Task<bool> PollNode(int nodeId);
        public Task<dynamic> SendElectionMessageToNode(int nodeId, object message);
    }
}
