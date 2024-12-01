using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TaskScheduler.src.Services.Tasks.TaskQueue;

namespace TaskScheduler.src.Services.Tasks.Roles
{
    public class LeaderRole : IRole
    {
        private readonly ITaskQueue _taskQueue;
        private readonly ILogger _logger;

        public LeaderRole(ITaskQueue taskQueue, ILoggerFactory logger)
        {
            _taskQueue = taskQueue;
            _logger = logger.CreateLogger("Leader");
        }

        public async Task<bool> Perform()
        {
            int taskId = RandomNumberGenerator.GetInt32(1000);

            _logger.LogInformation($"Pushing task {taskId}");
            bool res = await _taskQueue.PushTask(taskId);
            string success = res ? "successfully" : "unsuccessfully";

            _logger.LogInformation($"Task is pushed {success}");

            return true;
        }
    }
}
