using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Interfaces
{
    public interface INodeCommunication
    {
        public Task<bool> FloodIdToNode(string nodeUrl, int id);
        public Task<bool> PollNode(string nodeUrl);
        public Task<bool> SignalToFloodId(string nodeUrl);
    }
}
