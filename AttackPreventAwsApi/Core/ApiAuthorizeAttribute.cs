using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;

namespace AttackPreventAwsApi.Core
{
    public class ApiAuthorizeAttribute : AuthorizationFilterAttribute
    {
        private readonly string key = "EEF1BFC8-177C-424E-8F05-AFC08DEFBAC3";
        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            bool ifAuth = false;
            if(actionContext.Request.Headers.Authorization != null)
            {
                if (actionContext.Request.Headers.Authorization.Scheme.Equals("Basic", StringComparison.InvariantCultureIgnoreCase))
                {
                    string credential = actionContext.Request.Headers.Authorization.Parameter;
                    if (!string.IsNullOrEmpty(credential) && credential == key)
                    {
                        ifAuth = true;
                    }
                }
            }

            if (!ifAuth)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            return Task.FromResult(0);
        }
    }
}