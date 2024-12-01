using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TaskScheduler.src.Services.Tasks.TaskQueue;

namespace TaskScheduler.src.Services.Tasks.Roles
{
    public class WorkerRole : IRole
    {
        private readonly ITaskQueue _taskQueue;
        private readonly ILogger<WorkerRole> _logger;

        public WorkerRole(ITaskQueue taskQueue, ILogger<WorkerRole> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        public async Task<bool> Perform()
        {
            int taskId = await _taskQueue.ConsumeTask();

            _logger.LogInformation($"Consuming task {taskId}");

            return true;
        }
    }
}
