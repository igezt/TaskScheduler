using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Strategy
{
    public interface IElectionStrategy
    {
        int ElectLeader(int currentLeader, int otherNodeId);
    }
}