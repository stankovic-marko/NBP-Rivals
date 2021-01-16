using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rivals.DTOs;
using StackExchange.Redis;

namespace Rivals.Controllers
{
    [Route("api/matchmaker")]
    [ApiController]
    public class MatchmakerController : ControllerBase
    {

        private readonly IDatabase redisConn;


        public MatchmakerController(IDatabase redisConn)
        {
            this.redisConn = redisConn;
        }

        // mm.name.on = true enque to active matchmakers
        [Route("enable/{name}")]
        [HttpPost]
        public IActionResult EnableMatchmaker(string name)
        {
            redisConn.StringSet("mm."+name+".on", "true");
            return Ok();
        }

        // mm.name.on = false/void treba da se ociste svi prijavljeni iz baze
        [Route("disable/{name}")]
        [HttpPost]
        public IActionResult DisableMatchmaker(string name)
        {
            redisConn.StringSet("mm."+name + ".on", "false");
            return Ok();
        }

        // enqueue mm.mmname.rating -> RivalId
        [Route("enqueue/{matchmakerName}")]
        [HttpPost]
        public IActionResult AddToMatchmaker(MatchmakerInput[] matchmakerInput, string matchmakerName)
        {
            if (redisConn.StringGet("mm."+ matchmakerName + ".on") != "true")
                return BadRequest("Matchmaker is not started");
            foreach (var input in matchmakerInput)
            {
                redisConn.ListRightPush("mm." + matchmakerName + ".rt." + input.Rating, input.RivalId);
            }
            return Ok();
        }

        // enqueue mm.mmname.rating -> RivalId
        [Route("remove/{matchmakerName}")]
        [HttpDelete]
        public IActionResult RemoveFromMatchmaker(MatchmakerInput[] matchmakerInput, string matchmakerName)
        {

            if (redisConn.StringGet("mm." + matchmakerName + ".on") != "true")
                return BadRequest("Matchmaker is not started");
            foreach (var input in matchmakerInput)
            {
                redisConn.ListRemove("mm." + matchmakerName + ".rt." + input.Rating, input.RivalId, 0);
            }
            return Ok();
        }

        // tbd
        [Route("GetMatches/{matchmakerName}")]
        public IActionResult GetMatches(string matchmakerName)
        {
            var toRet = redisConn.ListRange("mm." + matchmakerName + ".matches").ToList();
            redisConn.ListTrim("mm." + matchmakerName + ".matches", toRet.Count, -1);
            return Ok(toRet.ToList());
        }

        // tbd
        [Route("PeekMatches/{matchmakerName}")]
        public IActionResult PeekMatches(string matchmakerName)
        {
            return Ok(redisConn.ListRange("mm."+matchmakerName+".matches").ToList());
        }


    }
}