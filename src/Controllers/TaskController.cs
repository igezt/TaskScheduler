using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskScheduler.src.Services.Tasks.TaskQueue;

namespace TaskScheduler.src.Controllers
{
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly ILogger<TaskController> _logger;
        private readonly ITaskQueue _taskQueue;

        public TaskController(ILogger<TaskController> logger, ITaskQueue taskQueue)
        {
            _logger = logger;
            _taskQueue = taskQueue;
        }

        // POST api/<TaskController>
        [HttpPost]
        public async Task<bool> Post([FromBody] int taskId)
        {
            _logger.LogInformation($"Task {taskId} has been received");
            await _taskQueue.EnqueueTask(taskId);
            return true;
        }
    }
}
