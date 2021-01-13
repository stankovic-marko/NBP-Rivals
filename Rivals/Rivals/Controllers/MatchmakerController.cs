using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Rivals.DTOs;

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

        // name.on = true
        [Route("enable")]
        [HttpPost]
        public IActionResult EnableMatchmaker(string name)
        {
            return Ok();
        }

        // name.on = false/void mozda da se ociste svi prijavljeni iz baze
        [Route("disable")]
        [HttpPost]
        public IActionResult DisableMatchmaker(string name)
        {
            return Ok();
        }

        // enqueue mmname.rating -> RivalId
        [Route("enqueue")]
        [HttpPost]
        public IActionResult AddToMatchmaker(MatchmakerInput matchmakerInput)
        {
            return Ok();
        }

        // enqueue mmname.rating -> RivalId
        [Route("remove")]
        [HttpPost]
        public IActionResult RemoveFromMatchmaker(MatchmakerInput matchmakerInput)
        {
            return Ok();
        }

        // tbd
        [Route("GetMatches")]
        public IActionResult GetMatches(string name)
        {
            return Ok();
        }

        // tbd
        [Route("PeekMatches")]
        public IActionResult PeekMatches(string name)
        {
            return Ok();
        }


    }
}