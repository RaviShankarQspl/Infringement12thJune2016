using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InfringementCustomerWeb.Startup))]
namespace InfringementCustomerWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
