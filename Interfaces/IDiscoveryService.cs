using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Interfaces
{
    public interface IDiscoveryService
    {
        Task<List<int>> GetHealthyIds();
    
        Task<string> GetServiceAddress(int id);

        Task<bool> RegisterNode();

        Task DeregisterNode();

        Task<bool> IsNodeHealthy();
    }
}