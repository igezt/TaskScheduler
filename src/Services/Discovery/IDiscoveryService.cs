using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Discovery
{
    public interface IDiscoveryService
    {
        Task<List<int>> GetHealthyIds();

        Task<string> GetNodeAddress(int nodeId);

        Task<bool> RegisterNode();

        Task DeregisterNode();

        Task<bool> IsNodeHealthy();
    }
}
