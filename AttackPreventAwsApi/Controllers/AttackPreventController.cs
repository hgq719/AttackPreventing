using AttackPrevent.Business;
using AttackPrevent.Model;
using AttackPreventAwsApi.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AttackPreventAwsApi.Controllers
{
    public class AttackPreventController : ApiController
    {
        private ILogService _logger = new LogService();

        #region Test
        // GET api/<controller>
        [ApiAuthorize]
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
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
            return Ok(list);
            //Code review by michael. 如果方法内部报错怎么办? 1.没有log error. 2. 这个时候还应该返回Ok 吗？ 3.我觉得这里要做try catch 处理.
        }

        [HttpGet]
        [Route("GetWhiteList/{zoneId}/WhiteLists")]
        [ApiAuthorize]
        public IHttpActionResult GetWhiteList(string zoneId)
        {
            var authEmail = string.Empty;
            var authKey = string.Empty;
            //zoneID = "";

            var zoneList = ZoneBusiness.GetZoneList();
            var zone = zoneList.FirstOrDefault(a => a.ZoneId == zoneId);
            if (zone != null)
            {
                authEmail = zone.AuthEmail;
                authKey = zone.AuthKey;
            }

            ICloudFlareApiService cloudFlareApiService = new CloudFlareApiService();
            var list = cloudFlareApiService.GetAccessRuleList(zoneId, authEmail, authKey, EnumMode.whitelist);
            var whiteListModelList = new List<WhiteListModel>();
            if (list != null && list.Count > 0)
            {
                whiteListModelList = list.Select(a => new WhiteListModel
                {
                    IP = a.configurationValue,
                    Notes = a.notes,
                }).ToList();
            }
            return Ok(whiteListModelList);
        }

        [HttpGet]
        [Route("GetZoneList/Zones")]
        [ApiAuthorize]
        public IHttpActionResult GetZoneList()
        {
            var zoneList = ZoneBusiness.GetZoneList();         
            return Ok(zoneList);
        }

        [HttpPost]
        [Route("IISLogs/AnalyzeResult")]
        [ApiAuthorize]
        public IHttpActionResult AnalyzeResult(AnalyzeResult analyzeResult)
        {
            var attackPreventService = AttackPreventService.GetInstance();
            attackPreventService.Add(analyzeResult);
            return Ok();
        }


        #endregion
        
    }
}