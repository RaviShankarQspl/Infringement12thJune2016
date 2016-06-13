using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InfringementWeb.Controllers
{
    public class BaseController : Controller
    {
        //// GET: Base
        //public ActionResult Index()
        //{
        //    return View();
        //}

        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
       System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //string a = Session["ESport@ClubID"].ToString();
            //Session["ESport@ClubID"] = null;
            if (Session["Authenticated"] == null)
                filterContext.Result = this.Redirect("/Account/Login"); //Url.Content("~/")+

            //return Redirect(Url.Content("~/"));
        }

        /// <summary>
        /// On Exception, Exception will be handled.
        /// </summary>
        /// <param name="filterContext">Exception Context object.</param>
        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    filterContext.ExceptionHandled = true;
        //    ////Session["ErrorMessage"] = filterContext.Exception;
        //    this.TempData["ErrorMessage"] = filterContext.Exception;
        //    _logger.Warn("Error :" + filterContext.Exception.Message);
        //    try
        //    {
        //        string action = filterContext.RouteData.Values["action"].ToString();
        //        string returnType = filterContext.Controller.GetType().GetMethod(action).ReturnType.Name;

        //        if (returnType == "ActionResult")
        //            this.TempData["ViewType"] = true;
        //    }
        //    catch (Exception ex) { }


        //    filterContext.Result = this.Redirect("/Home/ErrorPage");
        //}
    }
}