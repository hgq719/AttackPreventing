using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class RateLimitRule
    {
        public string id { get; set; }
        public bool disabled { get; set; }
        public string description { get; set; }
        public Match match { get; set; }
        public Bypass[] bypass { get; set; }
        public bool login_protect { get; set; }
        public int threshold { get; set; }
        public int period { get; set; }
        public RateLimitAction action { get; set; }
    }
    public class RateLimitAction
    {
        public string mode { get; set; }
        public int timeout { get; set; }
        public RateLimitActionResponse response { get; set; }
    }
    public class RateLimitActionResponse
    {
        public string content_type { get; set; }
        public int body { get; set; }
    }
    public class Bypass
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    public class Match
    {
        public Request request { get; set; }
        public Response response { get; set; }
    }
    public class Request
    {
        public string[] methods { get; set; }
        public string[] schemes { get; set; }
        public string url { get; set; }
    }
    public class Response
    {
        public bool origin_traffic { get; set; }
        public int[] status { get; set; }
        public Header[] headers { get; set; }
    }
    public class Header
    {
        public string name { get; set; }
        public string op { get; set; }
        public string value { get; set; }
    }
    public class RateLimitResponse
    {
        public bool success { get; set; }
        public Error[] errors { get; set; }
        public string[] messages { get; set; }
        public RateLimitRule[] result { get; set; }
        public resultInfo result_info { get; set; }
    }
    public class CreateRateLimitResponse
    {
        public bool success { get; set; }
        public Error[] errors { get; set; }
        public string[] messages { get; set; }
        public RateLimitRule result { get; set; }
    }
    public class UpdateRateLimitResponse
    {
        public bool success { get; set; }
        public Error[] errors { get; set; }
        public string[] messages { get; set; }
        public RateLimitRule result { get; set; }
    }
    public class DeleteRateLimitResponse
    {
        public bool success { get; set; }
        public Error[] errors { get; set; }
        public string[] messages { get; set; }
        public RateLimitRule result { get; set; }
    }
}
