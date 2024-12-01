using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskScheduler.Discovery;

namespace TaskScheduler.src.Services.Tasks.TaskQueue
{
    public class HttpTaskQueue : ITaskQueue
    {
        private readonly ConcurrentQueue<int> tasks;

        private readonly HttpClient _http;

        private readonly IDiscoveryService _discovery;

        private readonly Random _rnd = new();

        private readonly ILogger<HttpTaskQueue> _logger;

        public HttpTaskQueue(
            HttpClient httpClient,
            IDiscoveryService discoveryService,
            ILogger<HttpTaskQueue> logger
        )
        {
            tasks = new ConcurrentQueue<int>();
            _http = httpClient;
            _discovery = discoveryService;
            _logger = logger;
        }

        public async Task<int> ConsumeTask()
        {
            tasks.TryDequeue(out int newTask);

            return newTask == 0 ? -1 : newTask;
        }

        public async Task<bool> PushTask(int taskId)
        {
            List<int> healthyIds = await _discovery.GetHealthyIds();

            if (healthyIds.Count == 0)
            {
                return false;
            }

            int randomIndex = _rnd.Next(healthyIds.Count);

            int firstHealthyId = healthyIds[randomIndex];

            string nodeAddress = await _discovery.GetNodeAddress(firstHealthyId);

            var res = await _http.PostAsJsonAsync(nodeAddress + "/api/task", taskId);

            return res.IsSuccessStatusCode;
        }

        public async Task<bool> EnqueueTask(int taskId)
        {
            tasks.Enqueue(taskId);
            return true;
        }
    }
}