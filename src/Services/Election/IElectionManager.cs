using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Election
{
    public interface IElectionManager
    {
        Task<bool> TryAcquireLeadership(CancellationToken token);

        Task ReleaseLeadership(CancellationToken token);

        Task<bool> IsLeader(CancellationToken token);
    }
}
