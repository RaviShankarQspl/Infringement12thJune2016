using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InfringementWeb.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace InfringementWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
