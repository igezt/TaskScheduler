using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskScheduler.Coordinator;
using TaskScheduler.Election;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectionController : ControllerBase
    {
        private readonly ICoordinator _coordinator;
        private readonly ILogger<ElectionController> _logger;

        public ElectionController(ICoordinator coordinator, ILogger<ElectionController> logger)
        {
            _coordinator = coordinator;
            _logger = logger;
        }

        // // GET: api/<LeaderController>
        [HttpGet]
        public async void Get()
        {
            _logger.LogWarning("Request to execute leader election received.");
        }

        // // GET api/<LeaderController>/5
        // [HttpGet("{id}")]
        // public IActionResult Get(int id)
        // {
        //     _logger.LogWarning("Request to check leader id received.");
        //     return Ok(_leaderService.IsLeader(id));
        // }

        // // POST api/<LeaderController>/flood-id
        // [HttpPost]
        // public IActionResult ElectionMessage([FromBody] ElectionMessage electionMessage)
        // {
        //     return Ok(
        //         _coordinator.HandleElectionMessage(
        //             electionMessage.SenderId,
        //             electionMessage.Payload
        //         )
        //     );
        // }

        // // POST api/<LeaderController>/consensus
        // [HttpPost("consensus")]
        // public async void ConsensusId([FromBody] int consensusId) {
        //     await _leaderService.ReconcileConsensusId(consensusId);
        // }
    }
}
