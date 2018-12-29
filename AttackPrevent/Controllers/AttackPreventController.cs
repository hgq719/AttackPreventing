using AttackPrevent.Business;
using AttackPrevent.Core;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AttackPrevent.Controllers
{
    public class AttackPreventController : ApiController
    {
        #region Test
        // GET api/<controller>
        [ApiAuthorize]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public Task Post([FromBody] List<byte[]> value)
        {

            return Task.FromResult(0);
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
        #endregion

        #region AWS API
        [HttpGet]
        [Route("GetZones/{zoneId}/Ratelimits")]
        [ApiAuthorize]
        public IHttpActionResult GetRateLimits(string zoneId)
        {
            var list = RateLimitBusiness.GetList(zoneId);
            return Ok(new
            {
                result = list
            });
        }
        [HttpPost]
        [Route("IISLogs/AnalyzeResult")]
        [ApiAuthorize]
        public IHttpActionResult AnalyzeResult(AnalyzeResult analyzeResult)
        {
            IAttackPreventService attackPreventService = AttackPreventService.GetInstance();
            attackPreventService.Add(analyzeResult);
            return Ok();
        }
        #endregion

        #region IIS API

        #endregion
    }
}