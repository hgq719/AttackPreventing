using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Model.Cloudflare
{
    public class CloudflareLogErrorResponse
    {
        //{"success":false,"errors":[{"code":10000,"message":"Authentication error"}]}
        public bool Success { get; set; }

        public List<CloudflareLogError> Errors { get; set; }


    }

    public class CloudflareLogError
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
