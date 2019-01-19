using System;
using AttackPrevent.Business;
using AttackPreventAnalyzeEtwApi.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace AttackPreventAnalyzeEtwApi.Controllers
{
    public class AttackPreventController : ApiController
    {
        private readonly ILogService _logger = new LogService();

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

        #region IIS API
        [HttpPost]
        [Route("IISLogs/EtwResult")]
        //[ApiAuthorize]
        public async Task<IHttpActionResult> EtwResult()
        {
            try
            {
                _logger.Info($"Enter EtwResult method.");
                var ip = Utils.GetIPAddress();
                var buff = await Request.Content.ReadAsByteArrayAsync();
                var data = Utils.Deserialize(buff);
                var etwAnalyzeService = EtwAnalyzeService.GetInstance();

                await etwAnalyzeService.Add(ip, data);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.Error($"errorMessage = {e.Message}, \n StackTrace = {e.StackTrace}.");
                return null;
            }

        }
        #endregion
    }
}