using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GuidanceTracker.Startup))]
namespace GuidanceTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
