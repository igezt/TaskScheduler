using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Strategy
{
    public class MaxIdElectionStrategy: IElectionStrategy
    {
        public int ElectLeader(int currentLeader, int otherNodeId) {
            return Math.Max(currentLeader, otherNodeId);
        }
    }
}