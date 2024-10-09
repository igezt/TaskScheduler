using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeartBeatController : ControllerBase
    {
        // GET: api/<HeartBeatController>
        [HttpGet]
        public Microsoft.AspNetCore.Http.IResult Get()
        {
            return Results.Ok(1);
        }

        // GET api/<HeartBeatController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<HeartBeatController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<HeartBeatController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HeartBeatController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
