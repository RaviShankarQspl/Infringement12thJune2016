using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InfringementWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            ModelBinders.Binders.Add(typeof(Nullable<System.DateTime>), new CustomBinding.Models.DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(System.DateTime), new CustomBinding.Models.DateTimeModelBinder());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));
        }

        protected void Application_BeginRequest()
        {
            //CultureInfo info = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.ToString());
            //info.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
            //System.Threading.Thread.CurrentThread.CurrentCulture = info;
        }

        //protected void Application_Error(Object sender, EventArgs e)
        //{
        //    var exception = Server.GetLastError();
        //    this.Application["Exception"] = exception;
        //    //Response.Clear();

        //    Server.ClearError();

        //    Response.Redirect("/Home/ErrorPage");
        //}
    }
}
