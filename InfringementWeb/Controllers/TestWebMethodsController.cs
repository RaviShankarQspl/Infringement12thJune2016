using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using InfringementWeb.Models;
using InfringementWeb.Helpers;
using System.IO;
using RestSharp;

namespace InfringementWeb.Controllers
{
    public class TestWebMethodsController : Controller
    {
        // GET: TestWebMethods
        public ActionResult Index()
        {
            TestIngringementCreation();
            return View();
        }

        public void TestIngringementCreation()
        {

            string apiLink = System.Configuration.ConfigurationManager.AppSettings["APIServerPath"];

            var client = new RestClient(apiLink + "api"); //"http://localhost:50247/api/"

            List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];

            try
            {
                string cdate = System.DateTime.Now.Day.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString() + " " + System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString();

                    IRestRequest request = new RestRequest(
                        String.Format("infringement/create")
                        , Method.POST);

                    request.AddParameter(new Parameter
                    {
                        Name = "InfringementTime",
                        Type = ParameterType.GetOrPost,
                        Value = cdate  // DateTime.ParseExact("04/30/2013 23:00", "MM/dd/yyyy HH:mm", null)
                        //Value = Convert.ToDateTime(System.DateTime.Now.Day.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString() + " " + System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString())
                    });

                    request.AddParameter(new Parameter
                    {
                        Name = "InfringementNumber",
                        Type = ParameterType.GetOrPost,
                        Value = "INFG0002"
                    });

                    request.AddParameter(new Parameter
                    {
                        Name = "Rego",
                        Type = ParameterType.GetOrPost,
                        Value = "REG0002"
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "BuildingId",
                        Type = ParameterType.GetOrPost,
                        Value = 6
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "CarMakeId",
                        Type = ParameterType.GetOrPost,
                        Value = 1
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "CarModel",
                        Type = ParameterType.GetOrPost,
                        Value = 1
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "InfringementTypeId",
                        Type = ParameterType.GetOrPost,
                        Value = 1
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "Amount",
                        Type = ParameterType.GetOrPost,
                        Value = 10
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "Comment",
                        Type = ParameterType.GetOrPost,
                        Value = "Test API from web app"
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "UserName",
                        Type = ParameterType.GetOrPost,
                        Value = "10"
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "Latitude",
                        Type = ParameterType.GetOrPost,
                        Value = "111"
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "Longitude",
                        Type = ParameterType.GetOrPost,
                        Value = "222"
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "LoginId",
                        Type = ParameterType.GetOrPost,
                        Value = "anilkumarkaranam@gmail.com"
                    });
                    request.AddParameter(new Parameter
                    {
                        Name = "Password",
                        Type = ParameterType.GetOrPost,
                        Value = "anil"
                    });


                IRestResponse response = client.Execute(request);
                    var content = response.Content; // raw content as string

                //return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}