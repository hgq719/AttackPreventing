using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AttackPrevent.Startup))]
namespace AttackPrevent
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
