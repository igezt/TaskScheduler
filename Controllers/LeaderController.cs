using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskScheduler.Services;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderController : ControllerBase
    {
        private readonly ElectionService _leaderService;
        private readonly ILogger<LeaderController> _logger;

        public LeaderController(ElectionService leaderService, ILogger<LeaderController> logger)
        {
            _leaderService = leaderService;
            _logger = logger;
        }

        // // GET: api/<LeaderController>
        [HttpGet]
        public async void Get()
        {
            _logger.LogWarning("Request to execute leader election received.");
            await _leaderService.FloodId();
        }

        // // GET api/<LeaderController>/5
        // [HttpGet("{id}")]
        // public IActionResult Get(int id)
        // {
        //     _logger.LogWarning("Request to check leader id received.");
        //     return Ok(_leaderService.IsLeader(id));
        // }

        // POST api/<LeaderController>/flood-id
        [HttpPost("flood-id")]
        public void UpdateFromFloodId([FromBody] int floodId)
        {
            _leaderService.UpdateLeaderId(floodId);
        }

        // // POST api/<LeaderController>/consensus
        // [HttpPost("consensus")]
        // public async void ConsensusId([FromBody] int consensusId) {
        //     await _leaderService.ReconcileConsensusId(consensusId);
        // }
    }
}
