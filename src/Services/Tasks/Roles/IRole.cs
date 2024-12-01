using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.src.Services.Tasks.Roles
{
    public interface IRole
    {
        Task<bool> Perform();
    }
}
