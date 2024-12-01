using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskScheduler.src.Services.Tasks.TaskQueue
{
    public interface ITaskQueue
    {
        Task<bool> PushTask(int taskId);

        Task<int> ConsumeTask();

        Task<bool> EnqueueTask(int taskId);
    }
}
