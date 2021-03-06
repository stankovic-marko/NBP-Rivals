﻿using System;
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
            var matchmakerOn = redisConn.StringGet("mm." + name + ".on").ToString();
            if (matchmakerOn != "true")
                return BadRequest("MM is not started");

            var maxRt = redisConn.StringGet("mm." + name + ".maxrt").ToString();
            if (!(maxRt == null || maxRt == ""))
            {
                int range = Int32.Parse(maxRt);
                for (int i = 0; i < range; i++)
                    redisConn.KeyDelete("mm." + name + ".rt." + i);
                redisConn.KeyDelete("mm." + name + ".maxrt");
                redisConn.KeyDelete("mm." + name + ".matches");
            }
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
                bool matchFound = false;
                int i = 0;
                while (!matchFound && i <= 25)
                {
                    var list = redisConn.ListRange("mm." + matchmakerName + ".rt." + (input.Rating + i)).ToList();
                    if (list.Count > 0)
                    {
                        matchFound = true;
                        break;
                    }

                    if(i > 0)
                        i = -i;
                    else
                        i = -(i - 1);
                }

                if (matchFound)
                {
                    string rivalId = redisConn.ListLeftPop("mm." + matchmakerName + ".rt." + (input.Rating + i));
                    redisConn.ListRightPush("mm." + matchmakerName + ".matches", input.RivalId + "-" + rivalId);
                }
                else
                {
                    var maxRt = redisConn.StringGet("mm." + matchmakerName + ".maxrt").ToString();
                    if (maxRt == null || maxRt == "")
                    {
                        redisConn.StringSet("mm." + matchmakerName + ".maxrt", input.Rating);
                    }
                    else
                    {
                        if (Int32.Parse(maxRt) < input.Rating)
                            redisConn.StringSet("mm." + matchmakerName + ".maxrt", input.Rating);
                    }
                    redisConn.ListRightPush("mm." + matchmakerName + ".rt." + input.Rating, input.RivalId);
                }
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
        public JsonResult GetMatches(string matchmakerName)
        {
            var toRet = redisConn.ListRange("mm." + matchmakerName + ".matches").Select(x => x.ToString());
            redisConn.ListTrim("mm." + matchmakerName + ".matches", toRet.Count(), -1);
            return new JsonResult(toRet);
        }

        // tbd
        [Route("PeekMatches/{matchmakerName}")]
        public JsonResult PeekMatches(string matchmakerName)
        {
            return new JsonResult(redisConn.ListRange("mm." + matchmakerName + ".matches").Select(x => x.ToString()));
        }


    }
}