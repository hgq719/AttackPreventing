using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model
{
    public class RateLimitRule
    {
        public string Id { get; set; }
        public bool Disabled { get; set; }
        public string Description { get; set; }
        public RateLimitMatch Match { get; set; }
        public bool Login_Protect { get; set; }
        public int Threshold { get; set; }
        public int Period { get; set; }
        public RateLimitAction Action { get; set; }
    }

    public class RateLimitRules
    {
        public List<RateLimitRule> Result { get; set; }
        public bool Success { get; set; }
        public QueryResult Result_Info { get; set; }
    }

    public class RateLimitMatch
    {
        public MatchRequest Request { get; set; }
        public MatchResponse Response { get; set; }
    }

    public class MatchRequest
    {
        public List<string> Methods { get; set; }
        public List<string> Schemes { get; set; }
        public string Url { get; set; }
    }

    public class MatchResponse
    {
        public bool Origin_Traffic { get; set; }
        public List<MatchResponseHeaders> Headers { get; set; }
    }

    public class MatchResponseHeaders
    {
        public string Name { get; set; }
        public string Op { get; set; }
        public string Value { get; set; }
    }

    public class RateLimitByPass
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class RateLimitAction
    {
        public string Mode { get; set; }
        public int Timeout { get; set; }
        public RateLimitResponse response { get; set; }
    }

    public class RateLimitResponse
    {
        public string Content_Type { get; set; }
        public string Body { get; set; }
    }
}
