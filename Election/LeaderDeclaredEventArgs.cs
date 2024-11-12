using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.Election
{
    public class LeaderDeclaredEventArgs : EventArgs
    {
        public int LeaderID { get; set; }

        public LeaderDeclaredEventArgs(int leaderId)
        {
            LeaderID = leaderId;
        }
    }
}
