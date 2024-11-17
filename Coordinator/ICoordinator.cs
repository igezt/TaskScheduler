using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskScheduler.Election;

namespace TaskScheduler.Coordinator
{
    public interface ICoordinator
    {
        void RunNodeRole(CancellationToken token);
    }
}
