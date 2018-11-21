using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model.Cloudflare
{
    public class CloudflareRateLimitRule
    {
        public string Id { get; set; }
        public bool Disabled { get; set; }
        public string Description { get; set; }
        public RateLimitMatch Match { get; set; }
        public bool Login_Protect { get; set; }
        public int Threshold { get; set; }
        public int Period { get; set; }
        public RateLimitAction Action { get; set; }

        public CloudflareRateLimitRule()
        {
        }

        public CloudflareRateLimitRule(string _url, int _threshold, int _period)
        {
            Match = new RateLimitMatch()
            {
                Request = new MatchRequest()
                {
                    Methods = new List<string>() { "_ALL_" },
                    Schemes = new List<string>() { "_ALL_" },
                    Url = _url
                },
                Response = new MatchResponse()
                {
                    Origin_Traffic = true,
                    Headers = new List<MatchResponseHeaders> { new MatchResponseHeaders() {
                        Name = "Cf-Cache-Status",
                        Op = "ne",
                        Value = "HIT"
                    } }
                }
            };
            Disabled = false;
            Login_Protect = false;
            Threshold = _threshold;
            Period = _period;
            Action = new RateLimitAction()
            {
                Mode = "challenge",
                Timeout = 0
            };
        }
    }

    public class RateLimitRuleResponse
    {
        public List<CloudflareRateLimitRule> Result { get; set; }
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
