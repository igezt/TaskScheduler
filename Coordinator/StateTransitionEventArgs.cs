using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Coordinator
{
    public class StateTransitionEventArgs(NodeState newState)
    {
        public NodeState NewState = newState;
    }
}
