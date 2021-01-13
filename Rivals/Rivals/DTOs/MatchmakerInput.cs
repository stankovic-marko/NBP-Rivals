using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rivals.DTOs
{
    public class MatchmakerInput
    {
        public string RivalId { get; set; }

        public int Rating { get; set; }

        public string MatchmakerName { get; set; }
    }
}
