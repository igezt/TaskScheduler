using Microsoft.AspNetCore.Mvc;

namespace TaskScheduler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskSchedulerController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public int Get()
        {
            return 1;
        }
    }
}
