﻿using System;
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
using System.Net.Mail;
using Newtonsoft;
using System.Text.RegularExpressions;


namespace InfringementWeb.Controllers
{
    public class ImageUpload
    {
        public string FileName;
        public string ContentType;
        public byte[] Image;
        public string Latitude;
        public string Longitude;
        public string Comment;
    }

    public class InfringementsController : BaseController
    {
        private int PageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]);
        private infringementEntities db = new infringementEntities();
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index(int page = 1, string sortOrder = "IncidentTime", int lstpagesize = 0)
        {
            DateTime today = DateTime.Now;
            DateTime yesday = today.AddDays(-1);

            ViewBag.InitialSearch = true;
            ViewBag.StartDate = yesday;

            if (lstpagesize > 0)
            {
                Session["PageSize"] = lstpagesize;
                PageSize = lstpagesize;
            }
            else if (Session["PageSize"] != null)
            {
                PageSize = Convert.ToInt16(Session["PageSize"].ToString());
            }
            else if (Session["PageSize"] == null)
            {
                Session["PageSize"] = PageSize;
            }


            if (Session["SortOrder"] == null)
                Session["SortOrder"] = sortOrder;
            else if (sortOrder == "NA")
            {
                sortOrder = Session["SortOrder"].ToString();
            }
            else if (Session["SortOrder"] != null && Session["SortOrder"].ToString() != sortOrder)
                Session["SortOrder"] = sortOrder;
            else
                sortOrder = Session["SortOrder"].ToString();

            //var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).OrderBy(x => x.IncidentTime).Skip((page - 1) *  PageSize).Take(PageSize).ToList();
            List<infringement> infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).ToList();
            infringements = SortItems(infringements, sortOrder);
            infringements = infringements.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            ViewBag.PreviousPage = 0;
            ViewBag.NextPage = 0;

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalPages = 0;

            if (db.infringements.Count() > 0)
                ViewBag.TotalPages = Math.Ceiling((double)db.infringements.Count() / PageSize);

            if (ViewBag.TotalPages == page)
            {
                ViewBag.PreviousPage = page - 1;
                ViewBag.NextPage = 0;
            }
            else if (ViewBag.TotalPages <= 2)
            {
                ViewBag.PreviousPage = 0;
                ViewBag.NextPage = 0;
            }
            else if (page > 1)
            {
                ViewBag.PreviousPage = page - 1;
            }

            if (page == ViewBag.TotalPages)
                ViewBag.NextPage = 0;
            else
                if (page >= 1 && page < ViewBag.TotalPages)
                ViewBag.NextPage = page + 1;

            ViewBag.StartDate = yesday.ToString("dd-MM-yyyy HH:mm");
            ViewBag.ToDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm");


            IList<SelectListItem> lstPageSize = new List<SelectListItem>
            {
                new SelectListItem() {Text="10", Value="10"},
                new SelectListItem() { Text="20", Value="20"},
                new SelectListItem() { Text="50", Value="50"},
                new SelectListItem() { Text="75", Value="75"},
                new SelectListItem() { Text="100", Value="100"}
            };

            ViewBag.lstPageSize = new SelectList(lstPageSize, "Value", "Text", PageSize);
            Session["Sortinfringements"] = infringements;
            return View(infringements);
        }

        public List<infringement> SortItems(List<infringement> infringements, string sortOrder)
        {
            // List<infringement> infringements = new List<infringement>();
            //if (Session["Sortinfringements"] != null)
            //{
            //    infringements = Session["Sortinfringements"] as List<infringement>;

            List<infringement> ilist = new List<infringement>();
            switch (sortOrder)
            {
                case "Number":
                    ilist = infringements.OrderBy(item => item.Number).ToList();
                    break;
                case "Rego":
                    ilist = infringements.OrderBy(item => item.Rego).ToList();
                    break;
                case "location":
                    ilist = infringements.OrderBy(item => item.parking_location.Name).ToList();
                    break;
                case "cityname":
                    ilist = infringements.OrderBy(item => item.parking_location.city.name).ToList();
                    break;
                case "IncidentTime":
                    ilist = infringements.OrderBy(item => item.IncidentTime).ToList();
                    break;
                case "infringementtype":
                    ilist = infringements.OrderBy(item => item.infringementtype.Type).ToList();
                    break;
                case "Amount":
                    ilist = infringements.OrderBy(item => item.Amount).ToList();
                    break;
                case "Status":
                    ilist = infringements.OrderByDescending(item => item.StatusId).ToList();
                    break;

                default:
                    ilist = infringements.OrderBy(item => item.IncidentTime).ToList();
                    break;
            }
            //}
            return ilist;
        }
        public ActionResult DownloadInfList()
        {
            List<infringement> infringements = new List<infringement>();
            if (Session["DownloadList"] != null)
                infringements = Session["DownloadList"] as List<infringement>;

            var userslist = db.users.ToList();
            var makelist = db.makes.ToList();
            var carmodels = db.carmodels.ToList();
            var cities = db.cities.ToList();
            //var countries = db.countries.ToList();


            StringWriter sw = new StringWriter();

            sw.WriteLine("\"Infringement Number\",\"Rego\",\"City\",\"Car Park\",\"Make\",\"Model\",\"Incident DateTime\",\"Infringement Type\",\"Amount\",\"Comment\",\"Due Date\",\"After Due Date\",\"Latitude\",\"Longitude\",\"Created By\",\"Created Date\",\"Status\",\"Name\",\"Street 1\",\"Street 2\",\"Suburb\",\"City\",\"Post Code\",\"Country\",\"Payment Date\",\"Payment Method\",\"Transaction ID\",\"Images\"");

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment;filename=Infringements_List.csv");
            Response.ContentType = "text/csv";
            string pictures1 = "";
            string makename = "";
            string carmodel = "";
            string transactionno = "";
            string transactiondate = "";
            string paymentype = "Flo2Cash";
            foreach (infringement line in infringements)
            {
                pictures1 = "";
                paymentype = line.PaymentType;
                transactionno = "";
                transactiondate = "";

                var ipayment = db.infringement_payment.Where(x => x.InfringementId == line.Id).FirstOrDefault();
                if (ipayment != null)
                {
                    transactionno = "'" + ipayment.TransactionNumber;
                    if (ipayment.TransactionDate != null && ipayment.TransactionDate.Value != null)
                        transactiondate = ipayment.TransactionDate.Value.ToString("dd-MM-yyyy HH:mm");
                    //paymentype = "Flo2Cash";
                }
                var pictures = db.infringementpictures.Where(x => x.InfringementId == line.Id).Select(x => x.Location).ToList();

                var createdby1 = userslist.Where(x => x.Id == line.CreatedBy).FirstOrDefault();
                string createdby = "";
                if (createdby1 != null)
                {
                    if (createdby1.FirstName != null)
                        createdby = createdby1.FirstName;

                    if (createdby1.LastName != null)
                        createdby = createdby + " " + createdby1.LastName;

                }
                var makename1 = makelist.Where(x => x.id == line.MakeId).FirstOrDefault();
                if (makename1 != null)
                {
                    makename = makename1.Name;
                    if (makename.ToUpper().Contains("OTHER"))
                        makename = line.OtherMake;
                }
                var infringementCity = db.parking_location.FirstOrDefault(x => x.Id == line.ParkingLocationId).CityId;
                var Infrcityname = cities.Where(x => x.id == infringementCity).FirstOrDefault().name;

                //var countryname = "";
                //if (line.CountryId != null)
                // countryname = countries.Where(x => x.id == line.CountryId).FirstOrDefault().CountryName;

                var carmodel1 = carmodels.Where(x => x.Id == line.ModelId).FirstOrDefault();
                if (carmodel1 != null)
                {
                    carmodel = carmodel1.Name;
                    if (carmodel.ToUpper().Contains("OTHER"))
                        carmodel = line.OtherModel;
                }

                if (pictures != null)
                    foreach (string a in pictures)
                    {
                        if (a != null && a.Length > 0)
                            pictures1 = a + ", " + pictures1;
                    }

                sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\",\"{24}\",\"{25}\",\"{26}\",\"{27}\"",
                                           line.Number,
                                           line.Rego,
                                           Infrcityname,
                                           line.parking_location.Name,
                                           makename,
                                           carmodel,
                                           string.Format("{0:dd/MM/yyyy HH:mm}", line.IncidentTime),
                                           line.infringementtype.Type,
                                           line.Amount,
                                           line.Comment,
                                            string.Format("{0:dd/MM/yyyy}", line.DueDate),
                                             line.AfterDueDate,
                                              line.Latitude,
                                               line.Longitude,
                                                createdby,
                                                  string.Format("{0:dd/MM/yyyy}", line.CreatedDate),
                                                 ReturnStatus(line.StatusId),
                                                 line.OwnerName,
                                                 line.Street1,
                                                 line.Street2,
                                                 line.Suburb,
                                                 line.CityName,
                                                 line.PostCode,
                                                 line.CountryId,
                                                 transactiondate, //transaction date time
                                                 paymentype,
                                                 transactionno, // Transaction ID
                                                 pictures1
                                           ));
            }

            Response.Write(sw.ToString());

            Response.End();
            return View("Downloads", infringements);
        }

        private string ReturnStatus(int StatusId)
        {
            string status = "Pending";
            switch (StatusId)
            {

                case 1:
                    status = "Pending";
                    break;
                case 2:
                    status = "Paid";
                    break;
                case 3:
                    status = "Objected";
                    break;
                case 4:
                    status = "Cancelled";
                    break;
            }
            return status;
        }


        public ActionResult Downloads()
        {
            List<infringement> infringements = new List<infringement>();

            if (Request.Form["Submit"] != null)
            {
                if (Request.Form["Submit"].ToString() == "Search Infringement")
                {
                    if (Request.Form["SearchInfringeNumber"] != null && Request.Form["SearchInfringeNumber"].ToString().Trim().Length > 0)
                    {
                        string infringementno = Request.Form["SearchInfringeNumber"].ToString().Trim();
                        ViewData["SearchINO"] = infringementno;

                        //infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).Where(i => i.Number.ToUpper() == infringementno.Trim().ToUpper()).OrderByDescending(x => x.IncidentTime).ToList();

                        _logger.Info("Search on infringement number" + infringementno);
                        if (infringementno.Contains("*"))
                            infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                            .Where(x => x.Number.Trim().ToUpper().StartsWith(infringementno.Trim().ToUpper().Substring(0, infringementno.Trim().Length - 1)))
                            .OrderByDescending(x => x.IncidentTime)
                            .ToList();
                        else
                            infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                            .Where(x => x.Number.Trim().ToUpper() == infringementno.Trim().ToUpper())
                            .OrderByDescending(x => x.IncidentTime)
                            .ToList();
                    }
                    else
                        ModelState.AddModelError("", "Please enter Infringement Number.");
                }
                else if (Request.Form["Submit"].ToString() == "Search RegNo")
                {
                    if (Request.Form["SearchRegoNumber"] != null && Request.Form["SearchRegoNumber"].ToString().Trim().Length > 0)
                    {
                        string RegNo = Request.Form["SearchRegoNumber"].ToString().Trim();
                        ViewData["RegNo"] = RegNo;
                        //infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).Where(i => i.Rego.ToUpper() == RegNo.Trim().ToUpper()).OrderByDescending(x => x.IncidentTime).ToList();

                        _logger.Info("Search on rego number" + RegNo);
                        if (RegNo.Contains("*"))
                            infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                            .Where(x => x.Rego.Trim().ToUpper().StartsWith(RegNo.Trim().ToUpper().Substring(0, RegNo.Trim().Length - 1)))
                            .OrderByDescending(x => x.IncidentTime)
                            .ToList();
                        else
                            infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                            .Where(x => x.Rego.Trim().ToUpper() == RegNo.Trim().ToUpper())
                            .OrderByDescending(x => x.IncidentTime)
                            .ToList();
                    }
                    else
                        ModelState.AddModelError("", "Please enter Vehicle Registration Number.");
                }
                else if (Request.Form["Submit"].ToString() == "Search by Date")
                {
                    string SearchFrom = Request.Form["SearchFrom"].ToString().Trim();
                    string SearchTo = Request.Form["SearchTo"].ToString().Trim();

                    if (!string.IsNullOrEmpty(SearchFrom) && !string.IsNullOrEmpty(SearchTo))
                    {
                        //DateTime datefom = Convert.ToDateTime(SearchFrom);
                        //DateTime dateto = Convert.ToDateTime(SearchTo);

                        //DateTime startdate = new DateTime(datefom.Year, datefom.Month, datefom.Day);
                        //DateTime enddate = new DateTime(dateto.Year, dateto.Month, dateto.Day, 23, 59, 59);

                        string[] datefrom = SearchFrom.Split('-');

                        int fyear = Convert.ToInt16(datefrom[2]);
                        int fmonth = Convert.ToInt16(datefrom[1]);
                        int fday = Convert.ToInt16(datefrom[0]);

                        string[] dateto = SearchTo.Split('-');

                        int tyear = Convert.ToInt16(dateto[2]);
                        int tmonth = Convert.ToInt16(dateto[1]);
                        int tday = Convert.ToInt16(dateto[0]);

                        DateTime startdate = new DateTime(fyear, fmonth, fday);
                        DateTime enddate = new DateTime(tyear, tmonth, tday, 23, 59, 59);

                        _logger.Info("Search on infringement date range");

                        infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).Where(x => x.IncidentTime >= startdate && x.IncidentTime <= enddate).OrderByDescending(x => x.IncidentTime).ToList();
                    }
                    else
                        ModelState.AddModelError("", "Please select From and To dates.");
                }
            }
            if(Convert.ToString(Request.Form["SearchFrom"]) != null && Convert.ToString(Request.Form["SearchFrom"])!="" && Convert.ToString(Request.Form["SearchTo"]) != "" && Convert.ToString(Request.Form["SearchTo"]) != null)
            {
                TempData["DateFrom"] = Request.Form["SearchFrom"].ToString().Trim();
                TempData["DateTo"] = Request.Form["SearchTo"].ToString().Trim();
            }
           
            Session["DownloadList"] = infringements;

            return View(infringements);
        }

        // GET: Infringements
        public ActionResult IndexNew()
        {
            int page = 1;
            _logger.Info("List all infringements");
            //var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).OrderByDescending(x => x.IncidentTime).ToList();

            var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).OrderBy(x => x.IncidentTime).Skip((page - 1) * PageSize).Take(PageSize).ToList();


            ViewBag.CurrentPage = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalPages = Math.Ceiling((double)infringements.Count() / PageSize);

            if (infringements == null || infringements.Count == 0)
                ModelState.AddModelError("", "No data available.");

            Session["Sortinfringements"] = infringements;

            return View(infringements);
        }

        [HttpPost]
        public ActionResult Index(string SearchRegoNumber, string SearchInfringeNumber, string SearchFrom, string SearchTo, string Submit)
        {
            if (Session["PageSize"] != null)
            {
                PageSize = Convert.ToInt16(Session["PageSize"].ToString());
            }
            IList<SelectListItem> lstPageSize = new List<SelectListItem>
            {
                new SelectListItem() {Text="10", Value="10"},
                new SelectListItem() { Text="20", Value="20"},
                new SelectListItem() { Text="50", Value="50"},
                new SelectListItem() { Text="75", Value="75"},
                new SelectListItem() { Text="100", Value="100"}
            };

            ViewBag.lstPageSize = new SelectList(lstPageSize, "Value", "Text", PageSize);
            _logger.Info("Search for rego");
            var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).OrderBy(i => i.IncidentTime);

            if (Submit == "Search By Rego")
            {
                if (!string.IsNullOrEmpty(SearchRegoNumber))
                {
                    _logger.Info("Search on rego number");
                    if (SearchRegoNumber.Contains("*"))
                        return View(infringements
                            .Where(x => x.Rego.Trim().ToUpper().StartsWith(SearchRegoNumber.Trim().ToUpper().Substring(0, SearchRegoNumber.Trim().Length - 1))).OrderBy(i => i.IncidentTime)
                            .ToList());
                    else
                        return View(infringements
                        .Where(x => x.Rego.Trim().ToUpper() == SearchRegoNumber.Trim().ToUpper()).OrderBy(i => i.IncidentTime)
                        .ToList());
                }
                else
                {
                    _logger.Warn("Infringement number does not exist in the database");
                    ModelState.AddModelError("", "Please enter Registration Number.");
                }

            }
            else if (Submit == "Search By Number")
            {
                _logger.Info("Search on infringement number");
                if (!string.IsNullOrEmpty(SearchInfringeNumber))
                {
                    _logger.Info("Search on rego number");
                    if (SearchInfringeNumber.Contains("*"))
                        return View(infringements
                            .Where(x => x.Number.Trim().ToUpper().StartsWith(SearchInfringeNumber.Trim().ToUpper().Substring(0, SearchInfringeNumber.Trim().Length - 1))).OrderBy(i => i.IncidentTime)
                            .ToList());
                    else
                        return View(infringements
                        .Where(x => x.Number.Trim().ToUpper() == SearchInfringeNumber.Trim().ToUpper()).OrderBy(i => i.IncidentTime)
                        .ToList());
                }
                else
                {
                    _logger.Warn("Infringement number does not exist in the database");
                    ModelState.AddModelError("", "Please enter Infringement Number.");
                }
                return View(infringements
                    .Where(x => x.Number.Trim().ToUpper() == SearchInfringeNumber.Trim().ToUpper()).OrderBy(i => i.IncidentTime)
                    .ToList());
            }
            else if (!string.IsNullOrEmpty(SearchFrom) && !string.IsNullOrEmpty(SearchTo))
            {
                //No validation at present
                //DateTime searchFrom = DateTime.MinValue;
                //DateTime searchTo = DateTime.MaxValue;

                //DateTime.TryParseExact(SearchFrom, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out searchFrom);
                //searchFrom = searchFrom.Date;
                //DateTime.TryParseExact(SearchTo, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out searchTo);
                //searchTo = searchTo.Date.AddDays(1).AddSeconds(-1);

                //string date = "01/08/2008";
                //DateTime datefom = Convert.ToDateTime(SearchFrom);
                //DateTime dateto = Convert.ToDateTime(SearchTo);

                ViewBag.StartDate = SearchFrom;
                ViewBag.ToDate = SearchTo;

                string[] datefrom = SearchFrom.Split('-');

                int fyear = Convert.ToInt16(datefrom[2]);
                int fmonth = Convert.ToInt16(datefrom[1]);
                int fday = Convert.ToInt16(datefrom[0]);

                string[] dateto = SearchTo.Split('-');

                int tyear = Convert.ToInt16(dateto[2]);
                int tmonth = Convert.ToInt16(dateto[1]);
                int tday = Convert.ToInt16(dateto[0]);

                try
                {
                    DateTime startdate = new DateTime(fyear, fmonth, fday);
                    DateTime enddate = new DateTime(tyear, tmonth, tday, 23, 59, 59);

                    _logger.Info("Search on infringement date range");

                    ViewBag.TotalPages = 0;
                    return View(infringements
                        .Where(x => x.IncidentTime >= startdate && x.IncidentTime <= enddate)
                        .ToList());
                }
                catch
                {
                    _logger.Warn("Please select proper dates.");
                    ModelState.AddModelError("", "Please select Valid date with format(DD-MM-YYYY).");
                }
            }
            else
            {
                return View(infringements.ToList());
            }
            return View(infringements.ToList());
        }

        // GET: Infringements
        public ActionResult Search()
        {
            _logger.Info("Render page to search for infrngements");
            return View(new SearchInfringementModel());
        }

        [HttpPost]
        public ActionResult SearchList(SearchInfringementModel model)
        {
            ViewBag.NoOfRecords = 0;
            ViewData["NoOfRecords"] = 0;
            Session["SearchInfringementModel"] = model;

            _logger.Info("Search for infringement");
            var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype);
            List<infringement> infringementlist = new List<infringement>();
            if (model.SearchOnRegoNumber == null && model.SearchString == null)
            {
                ModelState.AddModelError("", "Reg No or Infringement value is required.");
                return View("Search");
            }
            else if (model.SearchOnRegoNumber != null && model.SearchString != null)
            {
                _logger.Info("Search on rego number");
                infringementlist = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                                .Where(x => x.Rego.Trim().ToUpper() == model.SearchOnRegoNumber.Trim().ToUpper() & x.Number.Trim().ToUpper() == model.SearchString.Trim().ToUpper() & x.StatusId == 1)
                                .ToList();
                // return View(infringementlist);
            }
            else if (model.SearchOnRegoNumber != null)
            {
                _logger.Info("Search on rego number");
                if (model.SearchOnRegoNumber.Contains("*"))
                    infringementlist = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                    .Where(x => x.Rego.Trim().ToUpper().StartsWith(model.SearchOnRegoNumber.Trim().ToUpper().Substring(0, model.SearchOnRegoNumber.Trim().Length - 1)) && x.StatusId == 1)
                    .ToList();
                else
                    infringementlist = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                    .Where(x => x.Rego.Trim().ToUpper() == model.SearchOnRegoNumber.Trim().ToUpper() && x.StatusId == 1)
                    .ToList();
                //return View(infringementlist);
            }
            else if (model.SearchString != null)
            {
                _logger.Info("Search on infringement number");

                if (model.SearchString.Contains("*"))
                    infringementlist = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                    .Where(x => x.Number.Trim().ToUpper().StartsWith(model.SearchString.Trim().ToUpper().Substring(0, model.SearchString.Trim().Length - 1)) && x.StatusId == 1)
                    .ToList();
                else
                    infringementlist = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                    .Where(x => x.Number.Trim().ToUpper() == model.SearchString.Trim().ToUpper() && x.StatusId == 1)
                    .ToList();

                //infringementlist = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype)
                //    .Where(x => x.Number.Trim().ToUpper() == model.SearchString.Trim().ToUpper() && x.StatusId == 1)
                //    .ToList();

                //return View(infringementlist);
            }

            if (infringementlist != null && infringementlist.Count > 0)
            {
                //checking the due date and amount
                foreach (infringement info in infringementlist)
                {
                    if (DateTime.Now > info.DueDate)
                    {
                        info.ActualAmountToPay = info.AfterDueDate.Value;
                        //info.DisplayDueAmount = "<b>" + info.AfterDueDate.Value + "</b>";
                        info.DisplayDueAmount = info.AfterDueDate.Value.ToString();
                        info.DisplayAmount = info.Amount.ToString();
                    }
                    else
                    {
                        info.ActualAmountToPay = info.Amount;
                        info.DisplayDueAmount = info.AfterDueDate.ToString();
                        //info.DisplayAmount = "<b>" + info.Amount.ToString() + "</b>"; 
                        info.DisplayAmount = info.Amount.ToString();
                    }
                }

                ViewData["NoOfRecords"] = infringementlist.Count;
                ViewBag.NoOfRecords = infringementlist.Count;
                Session["SearchList"] = infringementlist;
                return View(infringementlist);
            }
            else
            {
                ModelState.AddModelError("", "No pending payment for searched Car Reg. No. or Infringement No.");
                return View("Search");
            }

        }


        [HttpPost]
        public ActionResult PayNow(List<InfringementWeb.infringement> model, string Email, string InfringementIds, string Submit, string txtTotalAmount)
        {
            List<infringement> infringementlist = new List<infringement>();
            if (Session["SearchList"] != null)
                infringementlist = Session["SearchList"] as List<infringement>;

            if (InfringementIds.Trim() == Email.Trim())
                InfringementIds = "";

            //List<infringement> infringementlist = Session["SearchList"] as List<infringement>;
            SearchInfringementModel modeltemp = Session["SearchInfringementModel"] as SearchInfringementModel;

            ViewData["NoOfRecords"] = infringementlist.Count;
            ViewBag.NoOfRecords = infringementlist.Count;

            Session["PaymentOption"] = "F";

            ViewBag.Email = Email;
            ViewBag.TotalAmount = txtTotalAmount;

            if (Email == null && InfringementIds == null)
            {
                ModelState.AddModelError("", "Please enter the your email and select minimum one Infringement for payment.");
                return View("SearchList", infringementlist);
            }
            else if (Email == null || Email.Trim() == "")
            {
                ModelState.AddModelError("", "Please enter the your email.");
                return View("SearchList", infringementlist);
            }
            else if (InfringementIds == null || InfringementIds.Trim() == "")
            {
                ModelState.AddModelError("", "Please selecct at least one Infringement for payment.");
                return View("SearchList", infringementlist);
            }
            else if (!Helpers.HtmlHelpers.ValidateEmail(Email))
            {
                ModelState.AddModelError("", "Invalid Email Address. Please enter proper Email Id.");
                return View("SearchList", infringementlist);
            }
            List<int> IdsToPay = new List<int>();

            string selectedRecs = InfringementIds;

            if (InfringementIds != null && InfringementIds.Length > 0)
            {
                InfringementIds = InfringementIds.Substring(0, InfringementIds.Length - 1);

                string[] ids = InfringementIds.Split(',');
                foreach (string id in ids)
                {
                    IdsToPay.Add(Convert.ToInt16(id));
                }
            }

            if (IdsToPay.Count > 0)
            {
                int[] infringementIds = IdsToPay.ToArray();


                //var totalAmount = db.infringements
                //    .Where(x => infringementIds.Contains(x.Id) &&
                //        x.StatusId == 1).Sum(x => x.Amount);

                decimal totalAmount = Convert.ToDecimal(txtTotalAmount);
                Session["AmountToBePaid"] = totalAmount;
                Session["InfringementIds"] = InfringementIds;

                if (totalAmount > 0)
                {
                    Session["ReceiptEmail"] = Email;
                    if (Submit == "Pay via Credit Card")
                    {
                        return RedirectToPaymentPage(totalAmount, infringementIds, Email);
                    }
                    else
                    {
                        Session["PaymentOption"] = "P";
                        string merchanturl = ConfigurationManager.AppSettings["MerchantHomepageURL"];
                        string HomeUrl = ConfigurationManager.AppSettings["MerchantHomepageURL"];
                        string successUrl = ConfigurationManager.AppSettings["SuccessURL"];
                        string failureUrl = ConfigurationManager.AppSettings["FailureURL"];
                        string cancelurl = ConfigurationManager.AppSettings["CancellationURL"];
                        string notificationurl = ConfigurationManager.AppSettings["NotificationURL"];

                        string MerchantCode = ConfigurationManager.AppSettings["MerchantCode"];
                        string AuthenticationCode = ConfigurationManager.AppSettings["AuthenticationCode"];
                        string PoliURL = ConfigurationManager.AppSettings["PoliURL"];

                        //string POLiAuthenticationKey = ConfigurationManager.AppSettings["POLiAuthenticationKey"];
                        //string POLiAPIURL = ConfigurationManager.AppSettings["POLiAPIURL"];

                        List<infringement> infList = db.infringements.Include(i => i.parking_location)
                                .Where(x => infringementIds.Contains(x.Id)).ToList();
                        string particular = "";

                        if (infList != null && infList.Count > 0)
                        {
                            foreach (infringement item in infList)
                            {
                                particular = particular + item.Number + "_";
                            }
                        }
                        particular = particular.Substring(0, particular.Length - 1);

                        //reference = "Parking Infringement :"+ particular;

                        //string requiredParams = @"{'Amount':'"+ totalAmount + "', 'CurrencyCode':'NZD', 'MerchantReference':'" + reference + "','MerchantHomepageURL':'" + HomeUrl + "','SuccessURL':'" + successUrl + "','FailureURL':'" + failureUrl + "','CancellationURL':'" + cancelurl + "', 'NotificationURL':'" + notificationurl + "' }";

                        //var json = System.Text.Encoding.UTF8.GetBytes(requiredParams);

                        string requiredParams = @"{'Amount':'" + totalAmount + "', 'CurrencyCode':'NZD', 'MerchantReference':'" + particular + "',";

                        //requiredParams = requiredParams + @" 'MerchantHomepageURL':'http://indideveloper-001-site4.atempurl.com/',
                        //          'SuccessURL':'http://indideveloper-001-site4.atempurl.com/Infringements/TransSuccess',
                        //          'FailureURL':'http://indideveloper-001-site4.atempurl.com/Infringements/TransFailure',
                        //          'CancellationURL':'http://indideveloper-001-site4.atempurl.com/Infringements/TransCancelled',
                        //          'NotificationURL':'http://indideveloper-001-site4.atempurl.com/Infringements/nudge'

                        requiredParams = requiredParams + @" 'MerchantHomepageURL':'" + merchanturl + "','SuccessURL':'" + successUrl + "','FailureURL':'" + failureUrl + "', 'CancellationURL':'" + cancelurl + "', 'NotificationURL':'" + notificationurl + "' }";

                        var json = System.Text.Encoding.UTF8.GetBytes(requiredParams);

                        //var auth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("SS64000860:98!6tOii25GtT"));

                        var auth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(MerchantCode + ':' + AuthenticationCode));
                        //var myRequest = System.Net.WebRequest.Create("https://poliapi.apac.paywithpoli.com/api/Transaction/Initiate");
                        var myRequest = System.Net.WebRequest.Create(PoliURL);

                        myRequest.Method = "POST";
                        myRequest.ContentType = "application/json";
                        myRequest.Headers.Add("Authorization", "Basic " + auth);
                        myRequest.ContentLength = json.Length;

                        System.IO.Stream dataStream = myRequest.GetRequestStream();
                        dataStream.Write(json, 0, json.Length);
                        dataStream.Close();

                        var response = (System.Net.HttpWebResponse)myRequest.GetResponse();
                        var data = response.GetResponseStream();
                        var streamRead = new StreamReader(data);
                        Char[] readBuff = new Char[response.ContentLength];
                        int count = streamRead.Read(readBuff, 0, (int)response.ContentLength);
                        while (count > 0)
                        {
                            var outputData = new String(readBuff, 0, count);
                            Console.Write(outputData);
                            count = streamRead.Read(readBuff, 0, (int)response.ContentLength);
                            dynamic latest = Newtonsoft.Json.JsonConvert.DeserializeObject(outputData);
                            //Response.Redirect(latest["NavigateURL"].Value);
                            return Redirect(latest["NavigateURL"].Value);
                        }
                        response.Close();
                        data.Close();
                        streamRead.Close();

                    }
                }
                else
                {
                    ModelState.AddModelError("", "Already payment done for these Infringements.");

                    return View("SearchList", infringementlist);
                }
            }
            else
            {
                return View("SearchList", infringementlist);
            }

            return View("SearchList", infringementlist);
        }

        private ActionResult RedirectToPaymentPage(decimal amount, int[] infringementIds, string Email)
        {
            List<infringement> infList = db.infringements.Include(i => i.parking_location)
                .Where(x => infringementIds.Contains(x.Id)).ToList();
            string particular = "";
            string reference = "";
            if (infList != null && infList.Count > 0)
            {
                foreach (infringement item in infList)
                {
                    //if (!reference.Contains(item.parking_location.Name))
                    //    reference = reference + item.parking_location.Name + ",";

                    particular = particular + item.Number + ",";
                }
            }
            //if (reference.Trim().Length > 50)
            //    reference = reference.Substring(0, 49);

            reference = "Parking Infringement";

            //if (particular.Trim().Length > 50)
            //    particular = particular.Substring(0, 49);


            //var paymentUrl = ConfigurationManager.AppSettings["Flo2CashPaymentUrl"];
            //var returnUrl = ConfigurationManager.AppSettings["Flo2CashReturnUrl"];
            //var notificationUrl = ConfigurationManager.AppSettings["InfringementNotificationUrl"];
            //var clientAccountId = ConfigurationManager.AppSettings["ClientAccountId"];
            ////amount = 50.80M;
            //StringBuilder s = new StringBuilder();
            //s.Append("<html>");
            //s.AppendFormat("<body onload='document.forms[\"form\"].submit()'>");
            //s.AppendFormat("<form name='form' action='{0}' method='post'>", paymentUrl);
            //s.AppendFormat("<input type='hidden' name='cmd' value='_xclick' />");
            //s.AppendFormat("<input type='hidden' name='account_id' value='{0}' />", clientAccountId);
            //s.AppendFormat("<input type='hidden' name='amount' value='{0}' />", amount.ToString());
            //s.AppendFormat("<input type='hidden' name='notification_url' value='{0}' />", notificationUrl);
            //s.AppendFormat("<input type='hidden' name='reference' value='{0}' />", reference);
            //s.AppendFormat("<input type='hidden' name='particular' value='{0}' />", particular);

            //s.AppendFormat("<input type='hidden' name='return_url' value='{0}' />", returnUrl);
            //s.AppendFormat("<input type='hidden' name='display_customer_email' value='1' />");
            //s.AppendFormat("<input type='hidden' name='item_name' value='Infringement Payment' />");
            //s.AppendFormat("<input type='hidden' name='custom_data' value='{0}' />", String.Join(",", infringementIds) +"$"+ Email);
            //s.Append("</form></body></html>");

            if (particular.Trim().Length > 50)
                particular = particular.Substring(0, 49);

            var paymentUrl = ConfigurationManager.AppSettings["Flo2CashPaymentUrl"];
            var returnUrl = ConfigurationManager.AppSettings["Flo2CashReturnUrl"];
            var notificationUrl = ConfigurationManager.AppSettings["InfringementNotificationUrl"];
            var clientAccountId = ConfigurationManager.AppSettings["ClientAccountId"];

            StringBuilder s = new StringBuilder();
            s.Append("<html>");
            s.AppendFormat("<body onload='document.forms[\"form\"].submit()'>");
            s.AppendFormat("<form name='form' action='{0}' method='post'>", paymentUrl);
            s.AppendFormat("<input type='hidden' name='cmd' value='_xclick' />");
            s.AppendFormat("<input type='hidden' name='account_id' value='{0}' />", clientAccountId);
            s.AppendFormat("<input type='hidden' name='amount' value='{0}' />", amount.ToString());
            s.AppendFormat("<input type='hidden' name='notification_url' value='{0}' />", notificationUrl);
            s.AppendFormat("<input type='hidden' name='return_url' value='{0}' />", returnUrl);
            s.AppendFormat("<input type='hidden' name='reference' value='{0}' />", reference);
            s.AppendFormat("<input type='hidden' name='particular' value='{0}' />", particular);

            s.AppendFormat("<input type='hidden' name='display_customer_email' value='1' />");
            s.AppendFormat("<input type='hidden' name='item_name' value='Infringement Payment' />");
            s.AppendFormat("<input type='hidden' name='custom_data' value='{0}' />", String.Join(",", infringementIds) + "$" + Email);
            s.Append("</form></body></html>");

            return Content(s.ToString());
        }

        // GET: Infringements/Details/5
        public ActionResult Details(int? id)
        {
            using (log4net.NDC.Push("Infringement_Details"))
            {
                if (id == null)
                {
                    _logger.Warn("Bad request, infringemnet id is null");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                infringement infringement = db.infringements.Find(id);
                if (infringement == null)
                {
                    _logger.Warn("Infringement not found");
                    return HttpNotFound();
                }

                _logger.Info("Infringement found");
                return View(infringement);
            }
        }

        // GET: Infringements/Create
        public ActionResult Create()
        {
            DateTime today = new DateTime();
            DateTime tomorrow = DateTime.Now.AddDays(1);
            ViewBag.TomorrowDate = tomorrow.ToString("MM/dd/yyyy");

            ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "id", "name");
            ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name");
            ViewBag.PLocations = new SelectList(db.parking_location.Where(x => x.Id == 999), "Id", "Name");
            ViewBag.Models = new SelectList(db.carmodels.Where(x => x.Id == 999), "Id", "Name");
            // ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");

            Session["ImagesCount"] = 0;

            ViewBag.IncidentTime = DateTime.Now.ToString("MM-dd-yyyy HH:mm");

            TempData["Latitude"] = "";
            TempData["Longitude"] = "";
            TempData["Comment"] = "";

            ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type");

            Session["ImageList"] = new List<ImageUpload>();
            TempData["GalleryImageScroll"] = -1;
            Session["ImagesCount"] = 0;

            InfringementModel emptymodel = new InfringementModel();
            emptymodel.Latitude = "0";
            emptymodel.Longitude = "0";
            emptymodel.ImageLatitude = "0";
            emptymodel.ImageLongitude = "0";

            TempData["Latitude"] = "0";
            TempData["Longitude"] = "0";
            TempData["Comment"] = "";


            return View(emptymodel);
        }


        public ActionResult GetImage(string Id)
        {
            try
            {
                ViewBag.Latitude = "";
                ViewBag.Longitude = "";
                ViewBag.Comment = "";

                int id = -1;
                int.TryParse(Id, out id);
                if (Session["ImageList"] != null)
                {
                    List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                    if (id > -1)
                    {

                        byte[] imageData = null;

                        //byte[] imageData = imageUploads[id].Image;

                        if (imageUploads != null && imageUploads[id] != null && imageUploads[id].Image != null)
                        {
                            imageData = imageUploads[id].Image;

                            TempData["Latitude"] = imageUploads[id].Latitude;
                            TempData["Longitude"] = imageUploads[id].Longitude;
                            TempData["Comment"] = imageUploads[id].Comment;
                        }

                        return File(imageData, imageUploads[id].ContentType); // Might need to adjust the content type based on your actual image type
                    }
                    else
                    {
                        string path1 = Server.MapPath("~") + "Content/empty.png";

                        return File(ImageToBinary(path1), "png");
                    }
                }
                else
                {
                    string path1 = Server.MapPath("~") + "Content/empty.png";

                    return File(ImageToBinary(path1), "png");
                }
            }
            catch
            {

            }
            string path = Server.MapPath("~") + "Content/empty.png";

            return File(ImageToBinary(path), "png");
        }

        //public JsonResult GetImageObject(string Id)
        public ActionResult GetImageObject(string Id)
        {
            string path = Server.MapPath("~") + "Content/empty.png";
            var imagesource = File(ImageToBinary(path), "png");

            try
            {
                int id = -1;
                int.TryParse(Id, out id);
                if (Session["ImageList"] != null)
                {
                    List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                    if (id > -1)
                    {
                        byte[] imageData = null;

                        //byte[] imageData = imageUploads[id].Image;

                        if (imageUploads != null && imageUploads[id] != null && imageUploads[id].Image != null)
                        {
                            imageData = imageUploads[id].Image;

                            TempData["Latitude"] = imageUploads[id].Latitude;
                            TempData["Longitude"] = imageUploads[id].Longitude;
                            //TempData["Comment"] = imageUploads[id].Comment;
                            if (imageUploads[id].Comment == null)
                            {
                                TempData["Comment"] = "N/A";
                            }
                            else
                            {
                                TempData["Comment"] = imageUploads[id].Comment;
                            }


                        }
                        var result = (new { Latitude = TempData["Latitude"], Longitude = TempData["Longitude"], Comment = TempData["Comment"] });
                        //var result = new { CurrentAmount = CurrentAmount, NewAmount = NewAmount };
                        return Json(result, JsonRequestBehavior.AllowGet);

                        // return File(imageData, imageUploads[id].ContentType); // Might need to adjust the content type based on your actual image type
                    }

                }
                return Json(new { imagedata = imagesource, Latitude = TempData["Latitude"], Longitude = TempData["Longitude"], Comment = TempData["Comment"] }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

            }
            return Json(new { imagedata = imagesource, Latitude = TempData["Latitude"], Longitude = TempData["Longitude"], Comment = TempData["Comment"] }, JsonRequestBehavior.AllowGet);
        }

        public static byte[] ImageToBinary(string imagePath)
        {
            FileStream fS = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            byte[] b = new byte[fS.Length];
            fS.Read(b, 0, (int)fS.Length);
            fS.Close();
            return b;
        }

        public ActionResult RemoveImage(string Id)
        {
            int id = -1;
            int.TryParse(Id, out id);
            if (id > -1)
            {
                List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                byte[] imageData = null;

                //byte[] imageData = imageUploads[id].Image;

                imageUploads.RemoveAll(x => x.FileName == imageUploads[id].FileName);
                if (id > 0)
                    id = id - 1;
                else
                    id = 0;
                Session["ImageList"] = imageUploads;

                if (imageUploads != null && imageUploads.Count >= id && imageUploads[id] != null && imageUploads[id].Image != null)
                    imageData = imageUploads[id].Image;



                return File(imageData, imageUploads[id].ContentType); // Might need to adjust the content type based on your actual image type
            }
            else
            {
                return null;
            }
        }

        public bool IsAlphaNumeric(string Number)
        {
            if (Number == null || Number.Trim() == "")
                return true;

            Regex r = new Regex("^[a-zA-Z0-9]+$");
            if (r.IsMatch(Number))
                return true;
            else
                return false;
        }

        // POST: Infringements/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IncidentTime,Number,Rego,CityId,ParkingLocationId,MakeId,ModelId,OtherMake,OtherModel,InfringementTypeId,Amount,Comment,User,UploadTime,Latitude,Longitude,DueDate,AfterDueDate,Name,Street1,Street2,Suburb,PostCode,CountryId,CityName,OtherInfringementType,imageid,ImageLatitude,ImageLongitude,ImageComment")] InfringementModel infringement, string Submit, string GalleryImageScroll, HttpPostedFileBase upload)
        {
            DateTime today = new DateTime();
            DateTime tomorrow = DateTime.Now.AddDays(1);
            ViewBag.TomorrowDate = tomorrow.ToString("MM/dd/yyyy");

            using (log4net.NDC.Push("Create infringement post"))
            {
                var errors = ModelState
                               .Where(x => x.Value.Errors.Count > 0)
                               .Select(x => new { x.Key, x.Value.Errors })
                               .ToArray();
                if (Submit == "Remove Photo")
                {
                    if (Session["CurrentImageId"] != null)
                    {
                        List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                        int id = 0; // Convert.ToInt16(Session["CurrentImageId"]);

                        if (GalleryImageScroll != "-1")
                            id = Convert.ToInt16(GalleryImageScroll);

                        //if (imageUploads.Count >= id && id > 0)
                        //    id = id - 1;

                        if (id >= 0 && imageUploads.Count() > 0)
                        {
                            imageUploads.RemoveAll(x => x.FileName == imageUploads[id].FileName);
                            if (id > 0)
                                id = id - 1;
                            else
                                id = 0;
                            Session["ImageList"] = imageUploads;

                            int galleryImageScroll = -1;
                            int.TryParse(GalleryImageScroll, out galleryImageScroll);

                            TempData["GalleryImageScroll"] = id;
                            Session["CurrentImageId"] = id;

                            if (imageUploads != null)
                            {
                                Session["ImagesCount"] = imageUploads.Count();

                                if (imageUploads.Count() == 0)
                                {
                                    TempData["GalleryImageScroll"] = "-1";
                                    Session["CurrentImageId"] = "-1";
                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["GalleryImageScroll"] = "-1";
                        Session["CurrentImageId"] = null;
                    }

                    ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "id", "name", infringement.CityId);
                    ViewBag.PLocations = new SelectList(db.parking_location.Where(x => x.CityId == infringement.CityId).OrderBy(x => x.SortOrder), "Id", "Name", infringement.ParkingLocationId);
                    ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name");
                    ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type");
                    ViewBag.Models = new SelectList(db.carmodels.Where(x => x.MakeId == infringement.MakeId).OrderBy(x => x.SortOrder), "Id", "Name");
                    //ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");
                    ViewBag.IncidentTime = infringement.IncidentTime.ToString("MM-dd-yyyy HH:mm");

                    return View(infringement);
                }
                else if (Submit == "Add Photo")
                {
                    var validImageTypes = new string[]
                    {
                        "image/gif",
                        "image/jpg",
                        "image/jpeg",
                        "image/pjpeg",
                        "image/png"
                    };

                    if (upload != null && upload.ContentLength > 0)
                    {
                        if (!validImageTypes.Contains(upload.ContentType))
                        {
                            _logger.Warn("Please choose either a GIF, JPG or PNG image." + upload.ContentType);
                            ModelState.AddModelError("ImageUpload", "Please choose either a GIF, JPG or PNG image.");
                        }
                        else
                        {
                            ImageUpload imageUpload = new ImageUpload();

                            using (var binaryReader = new BinaryReader(upload.InputStream))
                            {
                                imageUpload.Image = binaryReader.ReadBytes(upload.ContentLength);
                            }

                            imageUpload.FileName = upload.FileName;
                            imageUpload.ContentType = upload.ContentType;
                            imageUpload.Latitude = infringement.ImageLatitude;
                            imageUpload.Longitude = infringement.ImageLongitude;
                            imageUpload.Comment = infringement.ImageComment;

                            TempData["Latitude"] = infringement.ImageLatitude;
                            TempData["Longitude"] = infringement.ImageLongitude;
                            TempData["Comment"] = infringement.ImageComment;

                            List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                            imageUploads.Add(imageUpload);
                            Session["ImageList"] = imageUploads;

                            int galleryImageScroll = -1;
                            int.TryParse(GalleryImageScroll, out galleryImageScroll);
                            TempData["GalleryImageScroll"] = galleryImageScroll + 1;
                            Session["CurrentImageId"] = galleryImageScroll + 1;

                            //int id = Convert.ToInt16(Session["CurrentImageId"]);

                            if (imageUploads != null)
                                Session["ImagesCount"] = imageUploads.Count();

                        }
                    }
                    else
                    {
                        _logger.Warn("Please choose either a GIF, JPG or PNG image.");
                        ModelState.AddModelError("ImageUpload", "Please choose photo either a GIF, JPG or PNG image.");
                        ImageUpload imageUpload = new ImageUpload();
                        if (TempData["GalleryImageScroll"] == null)
                            TempData["GalleryImageScroll"] = "-1";

                        TempData["Latitude"] = infringement.ImageLatitude;
                        TempData["Longitude"] = infringement.ImageLongitude;
                        TempData["Comment"] = infringement.ImageComment;

                        if (Session["CurrentImageId"] != null)
                        {
                            int galleryImageScroll = -1;
                            int.TryParse(GalleryImageScroll, out galleryImageScroll);
                            Session["CurrentImageId"] = galleryImageScroll + 0;
                            int id = Convert.ToInt16(Session["CurrentImageId"]);
                            Session["ImagesCount"] = id + 1;
                        }
                    }


                    ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "id", "name", infringement.CityId);
                    ViewBag.PLocations = new SelectList(db.parking_location.Where(x => x.CityId == infringement.CityId).OrderBy(x => x.SortOrder), "Id", "Name", infringement.ParkingLocationId);
                    ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name");
                    ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type");
                    ViewBag.Models = new SelectList(db.carmodels.Where(x => x.MakeId == infringement.MakeId).OrderBy(x => x.SortOrder), "Id", "Name");
                    //ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");
                    ViewBag.IncidentTime = infringement.IncidentTime.ToString("MM-dd-yyyy HH:mm");
                    //infringement.ImageComment = " ";
                    //infringement.ImageLatitude = " ";
                    //infringement.ImageLongitude = " ";

                    return View(infringement);
                }
                else if (Submit == "Save Infringement")
                {


                    if (ModelState.IsValid)
                    {
                        _logger.Info("model is valid, mapping to entity" + infringement);
                        try
                        {
                            infringement.User = "1";

                            var infring = db.infringements.FirstOrDefault(x => x.Number == infringement.Number);
                            if (infring != null)
                                infringement.Number = null;

                            var entityModel = MvcModelToDatabaseModelMapper.MapInfringementForCreate(infringement);

                            entityModel.CreatedBy = (int)Session["UserId"];
                            entityModel.CreatedDate = System.DateTime.Now;

                            entityModel.Pay = false;
                            entityModel.GeneratedFrom = 1;
                            int infringementCity = db.parking_location.FirstOrDefault(x => x.Id == infringement.ParkingLocationId).CityId;
                            entityModel.CityId = infringementCity;
                            db.infringements.Add(entityModel);
                            db.SaveChanges();

                            infringement.Number = entityModel.Number;

                            if (Session["ImageList"] != null)
                            {
                                if (db.infringements.FirstOrDefault(x => x.Number == infringement.Number) == null)
                                {
                                    _logger.Warn("Infringement number does not exist in the database");
                                    ModelState.AddModelError("infringementNumber", "Infringement Number does not exist");
                                }
                                else
                                {
                                    //IRestRequest request = new RestRequest(
                                    //    String.Format("infringement/{0}/images", infringement.Number)
                                    //    , Method.POST);

                                    string apiLink = System.Configuration.ConfigurationManager.AppSettings["APIServerPath"];

                                    var client = new RestClient(apiLink + "api"); //"http://localhost:50247/api"

                                    List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];

                                    try
                                    {
                                        foreach (ImageUpload imageUpload in imageUploads)
                                        {
                                            //byte[] fileData = null;
                                            //using (var binaryReader = new BinaryReader(upload.InputStream))
                                            //{
                                            //    fileData = binaryReader.ReadBytes(upload.ContentLength);
                                            //}
                                            IRestRequest request = new RestRequest(
                                                String.Format("infringement/{0}/images", infringement.Number)
                                                , Method.POST);


                                            request.AddFileBytes(imageUpload.FileName,
                                                imageUpload.Image, imageUpload.FileName, imageUpload.ContentType);

                                            request.AddParameter(new Parameter
                                            {
                                                Name = "Longitude",
                                                Type = ParameterType.GetOrPost,
                                                Value = imageUpload.Longitude   //infringement.Longitude
                                            });

                                            request.AddParameter(new Parameter
                                            {
                                                Name = "Latitude",
                                                Type = ParameterType.GetOrPost,
                                                Value = imageUpload.Latitude  //infringement.Latitude
                                            });

                                            request.AddParameter(new Parameter
                                            {
                                                Name = "Description",
                                                Type = ParameterType.GetOrPost,
                                                Value = imageUpload.Comment //imageUpload.FileName
                                            });

                                            _logger.Info("Everything is valid, calling the rest client to save image");
                                            IRestResponse response = client.Execute(request);
                                            var content = response.Content; // raw content as string

                                            // already image information we are stroing through API.
                                            //var imageViewModel = new infringementpicture
                                            //{
                                            //    //Location = String.Format("infringement/{0}/images/{1}", infringement.Number, imageUpload.FileName),
                                            //    Location = String.Format("{0}/{1}", infringement.Number, imageUpload.FileName),
                                            //    Latitude = infringement.Latitude,
                                            //    Longitude = infringement.Longitude,
                                            //    InfringementId = entityModel.Id      // int.Parse(infringement.Number)
                                            //};

                                            //db.infringementpictures.Add(imageViewModel);

                                            //db.SaveChanges();
                                        }

                                        TempData["InfringementCreated"] = "Created successfully.";
                                        return RedirectToAction("Index");
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Warn("Could not save image", ex);
                                        ModelState.AddModelError("ImageUpload", "Could not save image, try again");
                                        return View(infringement);
                                    }

                                }
                            }
                        }
                        catch (InvalidOfficerCodeException ex)
                        {
                            _logger.Warn("Officer code is invalid");
                            ModelState.AddModelError("User", "Invalid Officer Code");
                            _logger.Info("Infringement model is not valid, return back to create view");
                            ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "id", "name");
                            ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name");
                            ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type");
                            //ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");
                            return View(infringement);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn("Saving of infrin. entity failed", ex);
                            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                        }
                        TempData["InfringementCreated"] = "Created successfully.";
                        return RedirectToAction("Index");
                    }

                    _logger.Info("Infringement model is not valid, return back to create view");
                    ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "id", "name");
                    ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name");
                    ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type");
                    ViewBag.PLocations = new SelectList(db.parking_location.Where(x => x.CityId == infringement.CityId).OrderBy(x => x.SortOrder), "Id", "Name", infringement.ParkingLocationId);
                    //ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");
                    return View(infringement);
                }
                TempData["InfringementCreated"] = "Created successfully.";
                return RedirectToAction("Index");
            }
        }

        // GET: Infringements/Edit/5
        public ActionResult Edit(int? id)
        {
            DateTime today = new DateTime();
            DateTime tomorrow = DateTime.Now.AddDays(1);
            ViewBag.TomorrowDate = tomorrow.ToString("MM/dd/yyyy");

            using (log4net.NDC.Push("Edit Infringemnet - GET"))
            {
                Session["ImageList"] = new List<ImageUpload>();
                TempData["GalleryImageScroll"] = -1;
                Session["InfringementId"] = id.Value;
                Session["ImagesCount"] = 0;
                Session["DueDate"] = "";
                if (id == null)
                {
                    _logger.Warn("Cannot edit infringement as id was not sent across");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                infringement infringement = db.infringements.Find(id);
                if (infringement == null)
                {
                    _logger.Warn("Infringement was not found for id" + id);
                    return HttpNotFound();
                }
                _logger.Info("infringement found, setting up viewbag and returning view");

                List<ImageUpload> imageUploads = new List<ImageUpload>(); // Session["ImageList"];

                //var imageListDetails = db.infringementpictures
                //    .Where(x => x.InfringementId == id)
                //    //.Select(x => x.Location)
                //    .ToList();

                List<infringementpicture> imageListDetails = db.infringementpictures
                   .Where(x => x.InfringementId == id)
                   //.Select(x => x.Location)
                   .ToList();

                _logger.Info("infringement images count: " + imageListDetails.Count());
                //var imageList  = db.infringementpictures
                //    .Where(x => x.InfringementId == id)
                //    .Select(x => x.Location)
                //    .ToList();

                ViewBag.Images = imageListDetails;

                var infringementCity = db.parking_location
                    .FirstOrDefault(x => x.Id == infringement.ParkingLocationId).CityId;
                ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "Id", "Name", infringementCity);
                ViewBag.Buildings = new SelectList(db.parking_location.OrderBy(x => x.SortOrder), "Id", "Name", infringement.ParkingLocationId);
                ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name", infringement.MakeId);
                ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type", infringement.InfringementTypeId);
                ViewBag.Status = new SelectList(db.infringementstatus, "Id", "Name", infringement.StatusId);
                var model = MvcModelToDatabaseModelMapper.MapInfringementForDisplay(infringement);
                ViewBag.IncidentTime = model.IncidentTime.ToString("MM-dd-yyyy HH:mm");
                ViewBag.Models = new SelectList(db.carmodels.Where(x => x.MakeId == infringement.MakeId).OrderBy(x => x.SortOrder), "Id", "Name", infringement.ModelId);
                //ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");

                Session["DueDate"] = infringement.DueDate;
                //Session["DueDate"] = string.Format("{0:dd/MM/yyyy}", Session["DueDate"].ToString());
                TempData["DueDate"] = Session["DueDate"].ToString();

                TempData["Latitude"] = model.ImageLatitude;
                TempData["Longitude"] = model.ImageLongitude;
                TempData["Comment"] = model.ImageComment;

                ViewBag.ModelId = infringement.ModelId;
                model.CityId = Convert.ToInt16(infringementCity);
                ViewBag.APIServerPath = System.Configuration.ConfigurationManager.AppSettings["APIServerPath"].ToString();
                Session["EditObjectModel"] = infringement;

                Session["CurrentImageId"] = "-1";
                Session["ImagesCount"] = 0;
                // Session["ImageList"] = null;
                _logger.Info("returning model ");
                return View(model);
            }
        }

        // POST: Infringements/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IncidentTime,Number,Rego,CityId,ParkingLocationId,MakeId,ModelId,OtherMake,OtherModel,CarModel,InfringementTypeId,Amount,Comment,Latitude,Longitude,User,StatusId,,DueDate,AfterDueDate,Name,Street1,Street2,Suburb,PostCode,CountryId,CityName,OtherInfringementType,imageid,ImageLatitude,ImageLongitude,ImageComment")] InfringementModel model, string Submit, string GalleryImageScroll, HttpPostedFileBase upload)
        {
            DateTime tomorrow = DateTime.Now.AddDays(1);
            ViewBag.TomorrowDate = tomorrow.ToString("MM/dd/yyyy");

            //ViewBag.TomorrowDate = tomorrow.ToString("dd-MM-yyyy");

            int eid = (int)Session["InfringementId"];
            using (log4net.NDC.Push("Post for editing infrin."))
            {
                //int eid = (int)Session["InfringementId"];
                TempData["DueDate"] = Session["DueDate"].ToString();


                if (Submit == "Remove Photo")
                {
                    if (Session["CurrentImageId"] != null)
                    {
                        List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                        int id = 0;
                        if (GalleryImageScroll != "-1")
                            id = Convert.ToInt16(GalleryImageScroll);

                        //if (imageUploads.Count >= id && id > 0)
                        //    id = id - 1;

                        if (id >= 0)
                        {
                            imageUploads.RemoveAll(x => x.FileName == imageUploads[id].FileName);
                            if (id > 0)
                                id = id - 1;
                            else
                                id = 0;
                            Session["ImageList"] = imageUploads;

                            int galleryImageScroll = -1;
                            int.TryParse(GalleryImageScroll, out galleryImageScroll);

                            TempData["GalleryImageScroll"] = id;
                            Session["CurrentImageId"] = id;

                            if (imageUploads != null)
                            {
                                if (imageUploads.Count() == 0)
                                {
                                    TempData["GalleryImageScroll"] = "-1";
                                    Session["CurrentImageId"] = "-1";
                                }
                                Session["ImagesCount"] = imageUploads.Count();
                            }

                        }
                        if (imageUploads.Count() > 0)
                        {
                            TempData["Latitude"] = imageUploads[id].Latitude;
                            TempData["Longitude"] = imageUploads[id].Longitude;
                            if (imageUploads[id].Comment == null)
                            {
                                TempData["Comment"] = "N/A";
                            }
                            else
                            {
                                TempData["Comment"] = imageUploads[id].Comment;
                            }
                        }
                        if (imageUploads.Count() == 0)
                        {
                            TempData["Latitude"] = "";
                            TempData["Longitude"] = "";
                            TempData["Comment"] = "";
                        }

                    }
                    else
                    {
                        _logger.Warn("Please choose either a GIF, JPG or PNG image.");
                        ModelState.AddModelError("ImageUpload", "Please choose photo either a GIF, JPG or PNG image.");
                        ImageUpload imageUpload = new ImageUpload();
                        if (TempData["GalleryImageScroll"] == null)
                            TempData["GalleryImageScroll"] = "-1";

                        TempData["Latitude"] = model.ImageLatitude;
                        TempData["Longitude"] = model.ImageLongitude;
                        if (model.ImageComment == null)
                        {
                            TempData["Comment"] = "N/A";
                        }
                        else
                        {
                            TempData["Comment"] = model.ImageComment;
                        }

                        if (Session["CurrentImageId"] != null)
                        {
                            int galleryImageScroll = -1;
                            int.TryParse(GalleryImageScroll, out galleryImageScroll);
                            Session["CurrentImageId"] = galleryImageScroll + 0;
                            int id = Convert.ToInt16(Session["CurrentImageId"]);
                            Session["ImagesCount"] = id + 1;
                        }
                    }



                    ViewBag.Images = db.infringementpictures
                .Where(x => x.InfringementId == eid)
                // .Select(x => x.Location)
                .ToList();
                    var infringementCity = db.parking_location
                    .FirstOrDefault(x => x.Id == model.ParkingLocationId).CityId;
                    ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "id", "name", model.CityId);
                    ViewBag.Buildings = new SelectList(db.parking_location.Where(x => x.CityId == model.CityId).OrderBy(x => x.SortOrder), "Id", "Name", model.ParkingLocationId);
                    ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name");
                    ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type");
                    ViewBag.Models = new SelectList(db.carmodels.Where(x => x.MakeId == model.MakeId).OrderBy(x => x.SortOrder), "Id", "Name", model.ModelId);
                    //ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");
                    ViewBag.IncidentTime = model.IncidentTime.ToString("dd-MM-yyyy HH:mm");
                    ViewBag.Status = new SelectList(db.infringementstatus, "Id", "Name", model.StatusId);
                    model.DueDate = Convert.ToDateTime(Session["DueDate"]);
                    ViewData["Images"] = ViewBag.Images;

                    return View(model);
                }
                else if (Submit == "Add Photo")
                {
                    var validImageTypes = new string[]
                    {
                        "image/gif",
                        "image/jpg",
                        "image/jpeg",
                        "image/pjpeg",
                        "image/png"
                    };

                    if (upload != null && upload.ContentLength > 0)
                    {
                        if (!validImageTypes.Contains(upload.ContentType))
                        {
                            _logger.Warn("Please choose either a GIF, JPG or PNG image." + upload.ContentType);
                            ModelState.AddModelError("ImageUpload", "Please choose either a GIF, JPG or PNG image.");
                        }
                        else
                        {
                            ImageUpload imageUpload = new ImageUpload();

                            using (var binaryReader = new BinaryReader(upload.InputStream))
                            {
                                imageUpload.Image = binaryReader.ReadBytes(upload.ContentLength);
                            }

                            imageUpload.FileName = upload.FileName;
                            imageUpload.ContentType = upload.ContentType;

                            imageUpload.FileName = upload.FileName;
                            imageUpload.ContentType = upload.ContentType;
                            imageUpload.Latitude = model.ImageLatitude;
                            imageUpload.Longitude = model.ImageLongitude;
                            imageUpload.Comment = model.ImageComment;

                            TempData["Latitude"] = model.ImageLatitude;
                            TempData["Longitude"] = model.ImageLongitude;
                            if (model.ImageComment == null)
                            {
                                TempData["Comment"] = "N/A";
                            }
                            else
                            {
                                TempData["Comment"] = model.ImageComment;
                            }

                            List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                            imageUploads.Add(imageUpload);
                            Session["ImageList"] = imageUploads;

                            int galleryImageScroll = -1;
                            int.TryParse(GalleryImageScroll, out galleryImageScroll);
                            TempData["GalleryImageScroll"] = galleryImageScroll + 1;
                            Session["CurrentImageId"] = galleryImageScroll + 1;

                            int icount = (int)Session["CurrentImageId"];

                            Session["CurrentImageId"] = icount + 1;

                            int id = Convert.ToInt16(Session["ImagesCount"]);

                            if (imageUploads != null)
                                Session["ImagesCount"] = imageUploads.Count();

                            model.ImageLatitude = "0";
                            model.ImageLongitude = "0";
                            model.ImageComment = "0";
                        }
                    }
                    else
                    {
                        _logger.Warn("Please choose either a GIF, JPG or PNG image.");
                        ModelState.AddModelError("ImageUpload", "Please choose photo either a GIF, JPG or PNG image.");
                        ImageUpload imageUpload = new ImageUpload();
                        if (TempData["GalleryImageScroll"] == null)
                            TempData["GalleryImageScroll"] = "-1";

                        TempData["Latitude"] = model.ImageLatitude;
                        TempData["Longitude"] = model.ImageLongitude;
                        TempData["Comment"] = model.ImageComment;

                        if (Session["CurrentImageId"] != null)
                        {
                            int galleryImageScroll = -1;
                            int.TryParse(GalleryImageScroll, out galleryImageScroll);
                            Session["CurrentImageId"] = galleryImageScroll + 0;
                            int id = Convert.ToInt16(Session["CurrentImageId"]);
                            Session["ImagesCount"] = id + 1;
                        }
                    }
                    ViewBag.Images = db.infringementpictures
                .Where(x => x.InfringementId == eid)
                //.Select(x => x.Location)
                .ToList();
                    var infringementCity = db.parking_location
                    .FirstOrDefault(x => x.Id == model.ParkingLocationId).CityId;
                    ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "id", "name", model.CityId);
                    ViewBag.Buildings = new SelectList(db.parking_location.Where(x => x.CityId == model.CityId).OrderBy(x => x.SortOrder), "Id", "Name", model.ParkingLocationId);
                    ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name");
                    ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type");
                    ViewBag.Models = new SelectList(db.carmodels.Where(x => x.MakeId == model.MakeId).OrderBy(x => x.SortOrder), "Id", "Name", model.ModelId);
                    //ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");
                    ViewBag.IncidentTime = model.IncidentTime.ToString("dd-MM-yyyy HH:mm");
                    ViewBag.Status = new SelectList(db.infringementstatus, "Id", "Name", model.StatusId);
                    model.DueDate = Convert.ToDateTime(Session["DueDate"]);
                    return View(model);
                }
                else if (Submit == "Save Infringement")
                {

                    model.User = "1";
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { x.Key, x.Value.Errors })
                        .ToArray();

                    if (ModelState.IsValid)
                    {
                        _logger.Info("Model is valid, search for infringment in the database" + model.Number);
                        _logger.Info(model);
                        var entity = db.infringements.FirstOrDefault(x => x.Number == model.Number);
                        if (entity == null)
                        {
                            _logger.Warn("Infrin. not found");
                            return new HttpNotFoundResult();
                        }

                        _logger.Info("Infrin. found, updating the database entity");
                        MvcModelToDatabaseModelMapper.MapInfringementForEdit(model, entity);
                        try
                        {

                            infringement infringe = Session["EditObjectModel"] as infringement;
                            entity.StatusId = entity.StatusId;
                            entity.User = entity.User;
                            entity.GeneratedFrom = 1;
                            int infCity = db.parking_location.FirstOrDefault(x => x.Id == entity.ParkingLocationId).CityId;
                            entity.CityId = infCity;

                            db.SaveChanges();

                            entity.Number = model.Number;

                            if (Session["ImageList"] != null)
                            {
                                if (db.infringements.FirstOrDefault(x => x.Number == model.Number) == null)
                                {
                                    _logger.Warn("Infringement number does not exist in the database");
                                    ModelState.AddModelError("infringementNumber", "Infringement Number does not exist");
                                }
                                else
                                {
                                    //IRestRequest request = new RestRequest(
                                    //    String.Format("infringement/{0}/images", infringement.Number)
                                    //    , Method.POST);

                                    string apiLink = System.Configuration.ConfigurationManager.AppSettings["APIServerPath"];

                                    var client = new RestClient(apiLink + "api"); //"http://localhost:50247/api"

                                    List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];

                                    try
                                    {
                                        foreach (ImageUpload imageUpload in imageUploads)
                                        {
                                            //byte[] fileData = null;
                                            //using (var binaryReader = new BinaryReader(upload.InputStream))
                                            //{
                                            //    fileData = binaryReader.ReadBytes(upload.ContentLength);
                                            //}
                                            IRestRequest request = new RestRequest(
                                                String.Format("infringement/{0}/images", model.Number)
                                                , Method.POST);


                                            request.AddFileBytes(imageUpload.FileName,
                                                imageUpload.Image, imageUpload.FileName, imageUpload.ContentType);

                                            request.AddParameter(new Parameter
                                            {
                                                Name = "Longitude",
                                                Type = ParameterType.GetOrPost,
                                                Value = imageUpload.Longitude   //infringement.Longitude
                                            });

                                            request.AddParameter(new Parameter
                                            {
                                                Name = "Latitude",
                                                Type = ParameterType.GetOrPost,
                                                Value = imageUpload.Latitude  //infringement.Latitude
                                            });

                                            request.AddParameter(new Parameter
                                            {
                                                Name = "Description",
                                                Type = ParameterType.GetOrPost,
                                                Value = imageUpload.Comment //imageUpload.FileName
                                            });

                                            _logger.Info("Everything is valid, calling the rest client to save image");
                                            IRestResponse response = client.Execute(request);
                                            var content = response.Content; // raw content as string                                            
                                        }

                                        TempData["InfringementUpdated"] = "Updated successfully.";
                                        return RedirectToAction("Index");
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Warn("Could not save image", ex);
                                        ModelState.AddModelError("ImageUpload", "Could not save image, try again");
                                        return View(model);
                                    }

                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Warn("Infrin. could not be updated", ex);
                            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);

                        }
                        TempData["InfringementUpdated"] = "Updated successfully.";
                        return RedirectToAction("Index");
                    }

                    infringement infringement = db.infringements.Find(eid);
                    if (infringement == null)
                    {
                        _logger.Warn("Infringement was not found for id" + eid);
                        return HttpNotFound();
                    }
                    _logger.Info("infringement found, setting up viewbag and returning view");

                    ViewBag.Images = db.infringementpictures
                        .Where(x => x.InfringementId == infringement.Id)
                        //.Select(x => x.Location)
                        .ToList();
                    ViewBag.Images = null;
                    var infringementCity = db.parking_location
                       .FirstOrDefault(x => x.Id == infringement.ParkingLocationId).CityId;
                    ViewBag.Cities = new SelectList(db.cities.OrderBy(x => x.SortOrder), "Id", "Name", infringementCity);
                    ViewBag.Buildings = new SelectList(db.parking_location.OrderBy(x => x.SortOrder), "Id", "Name", infringement.ParkingLocationId);
                    ViewBag.Makes = new SelectList(db.makes.OrderBy(x => x.SortOrder), "id", "Name", infringement.MakeId);
                    ViewBag.InfringementTypes = new SelectList(db.infringementtypes.OrderBy(x => x.SortOrder), "Id", "Type", infringement.InfringementTypeId);
                    ViewBag.Status = new SelectList(db.infringementstatus, "Id", "Name", infringement.StatusId);
                    model = MvcModelToDatabaseModelMapper.MapInfringementForDisplay(infringement);
                    ViewBag.IncidentTime = model.IncidentTime.ToString("MM-dd-yyyy HH:mm");
                    ViewBag.Models = new SelectList(db.carmodels.Where(x => x.MakeId == infringement.MakeId).OrderBy(x => x.SortOrder), "Id", "Name", infringement.ModelId);
                    //ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");

                    model.CityId = Convert.ToInt16(infringementCity);
                    ViewBag.APIServerPath = System.Configuration.ConfigurationManager.AppSettings["APIServerPath"].ToString();
                    Session["EditObjectModel"] = infringement;

                    return View(model);
                }
            }

            return View(model);
        }

        // GET: Infringements/Delete/5
        public ActionResult Delete(int? id)
        {
            using (log4net.NDC.Push("Delete_Infrin GET"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = db.infringements.FirstOrDefault(x => x.Id == id);

                ViewBag.Images = db.infringementpictures
                    .Where(x => x.InfringementId == id)
                    .Select(x => x.Location)
                    .ToList();

                if (entity == null)
                {
                    _logger.Warn("infrin. could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("Infrin. found, mapping to view model to editing purposes");
                return View(MvcModelToDatabaseModelMapper.MapInfringementForDisplayDelete(entity));
            }
        }

        // POST: Infringements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (log4net.NDC.Push("Post_For_Delete"))
            {
                try
                {
                    var images = db.infringementpictures.Where(x => x.InfringementId == id);
                    db.infringementpictures.RemoveRange(images);

                    var entity = db.infringements.Find(id);
                    db.infringements.Remove(entity);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Warn("Error deleting infrin.", ex);
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
        }

        //[HttpPost]
        public void MNS()
        {
            _logger.Info("MNS called by Flo2Cash");
            var notificationUrl = ConfigurationManager.AppSettings["Flo2CashNotificationUrl"];
            WebClient wClient = new WebClient();
            wClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            string postData = "";
            string transactionNumber = String.Empty;
            string transactionResult = String.Empty;
            IList<String> customData = new List<String>();
            string amount = String.Empty;
            for (int i = 0; i < Request.Form.AllKeys.Length; i++)
            {
                string key = Request.Form.AllKeys[i];
                string value = Request.Form[i];

                if (key == "transaction_id")
                    transactionNumber = value;
                else if (key == "transaction_status")
                    transactionResult = value;
                else if (key == "custom_data")
                    customData = value.Split(new char[] { ',' });
                postData += key + "=" + Server.UrlEncode(value) + "&";
            }
            postData += "cmd" + "=" + "_xverify-transaction";
            _logger.Info("Posting the following data back  to Flo2Cash:" + postData);
            byte[] postBytes = Encoding.ASCII.GetBytes(postData);
            byte[] responseBytes = wClient.UploadData(notificationUrl, "POST", postBytes);
            string actionResponse = Encoding.ASCII.GetString(responseBytes);
            _logger.Info("Action response: " + actionResponse);
            if (actionResponse.Trim().ToUpper() == "VERIFIED")
            {
                foreach (var infr in customData)
                {
                    int infringementNumber = int.Parse(infr);
                    var infringement = db.infringements.FirstOrDefault(x => x.Id == infringementNumber
                    && x.StatusId == 1);
                    if (infringement == null)
                        _logger.Warn("Payment made BUT infringement not found. Infrin num:" +
                            infr + "txn:" + transactionNumber);
                    else
                        _logger.Warn("Payment made AND infringement found. Infrin num:" +
                            infr + "txn:" + transactionNumber);
                    if (transactionResult == "2")
                    {
                        _logger.Info("Payment successful, change infringement status");
                        infringement.StatusId = 2;
                    }

                    var infringementPayment = new infringement_payment
                    {
                        InfringementId = infringementNumber,
                        PaymentResult = transactionResult,
                        TransactionNumber = transactionNumber
                    };

                    _logger.Info("Infringement payment record created" + infringementPayment);

                    db.infringement_payment.Add(infringementPayment);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Payment record not saved in the database", ex);
                        throw;
                    }

                    _logger.Info("MNS completed");
                }
            }
        }

        // GET: Infringements
        public ActionResult FinesAcknowledgement()
        {
            _logger.Info("Acknowledgement of payment page");
            string transactionResult = String.Empty;
            string response_code = string.Empty;

            if (Request.Form["custom_data"] != null)
            {
                List<infringement> payitems = new List<infringement>();
                string PostData = "";
                string transactionNumber = "";
                for (int i = 0; i < Request.Form.AllKeys.Length; i++)
                {
                    string key = Request.Form.AllKeys[i];
                    string value = Request.Form[i];
                    PostData += key + "=" + Server.UrlEncode(value) + "&";

                    if (key == "txn_id")
                        transactionNumber = value;

                    else if (key == "txn_status")
                        transactionResult = value;
                    else if (key == "response_code")
                        response_code = value;

                }

                _logger.Info("Response From Payment Gateway : " + PostData);

                //int[] infringementIds = Array.ConvertAll(Request.Form["custom_data"].ToString().Split(','), int.Parse);

                if (response_code == "0" && transactionResult == "2")
                {
                    string[] custom_data = Request.Form["custom_data"].ToString().Split('$');

                    int[] infringementIds = Array.ConvertAll(custom_data[0].ToString().Split(','), int.Parse);

                    payitems = db.infringements
                       .Where(x => infringementIds.Contains(x.Id)
                           ).ToList();

                    //decimal totalamount = payitems.Sum(x => x.Amount);
                    decimal totalamount = 0;
                    foreach (infringement info in payitems)
                    {
                        if (DateTime.Now > info.DueDate)
                        {
                            info.ActualAmountToPay = info.AfterDueDate.Value;
                            //info.DisplayDueAmount = "<b>" + info.AfterDueDate.Value + "</b>";
                            info.DisplayDueAmount = info.AfterDueDate.Value.ToString();
                            info.DisplayAmount = info.Amount.ToString();
                        }
                        else
                        {
                            info.ActualAmountToPay = info.Amount;
                            info.DisplayDueAmount = info.AfterDueDate.ToString();
                            //info.DisplayAmount = "<b>" + info.Amount.ToString() + "</b>"; 
                            info.DisplayAmount = info.Amount.ToString();
                        }

                        totalamount += info.ActualAmountToPay;
                    }
                    ViewData["totalamount"] = totalamount;

                    Session["payitems"] = payitems;
                    Session["TransactionNO"] = transactionNumber;

                    //SendMail(payitems, transactionNumber, custom_data[1].ToString());
                }
                else
                {
                    List<infringement> infringementlist = Session["SearchList"] as List<infringement>;
                    ViewData["NoOfRecords"] = infringementlist.Count;
                    ViewBag.NoOfRecords = infringementlist.Count;
                    _logger.Info("Payment declined.");
                    ModelState.AddModelError("", "Payment Declined.");
                    return View("SearchList", model: infringementlist);
                }


                return View("FinesAcknowledgement", model: payitems);
            }
            else
            {
                List<infringement> infringementlist = Session["SearchList"] as List<infringement>;
                ViewData["NoOfRecords"] = infringementlist.Count;
                ViewBag.NoOfRecords = infringementlist.Count;
                _logger.Info("Payment Cancelled.");
                ModelState.AddModelError("", "Payment Cancelled.");
                return View("SearchList", model: infringementlist);
            }
            //return View("FinesAcknowledgement", model:ctlAllPostbackData);
            return View("FinesAcknowledgement", null);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SendMail(List<infringement> items, string transactionNumber, string Email, decimal totalamount, string PaymentType = "CCPayment")
        {
            string fromaddr = System.Configuration.ConfigurationManager.AppSettings["FromUserEmail"];
            string password = System.Configuration.ConfigurationManager.AppSettings["FromUserEmailPwd"];
            int smtpport = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
            string smtpserver = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
            //string toaddr = "anilkumarkaranam@gmail.com";//TO ADDRESS HERE

            try
            {
                if (items != null && items.Count > 0)
                {
                    if (transactionNumber == null)
                        transactionNumber = db.infringement_payment.Where(x => x.InfringementId == items[0].Id).Select(x => x.TransactionNumber).ToString();

                    // decimal totalamount = items.Sum(x => x.Amount);
                    decimal gst = 0;
                    decimal amount = 0;
                    if (totalamount > 0)
                    {
                        gst = Math.Round((totalamount * 3) / 23, 2);
                        amount = Math.Round(totalamount - gst, 2);
                    }

                    StringBuilder sbMail = new StringBuilder();
                    sbMail.Append("<!doctype html >");
                    sbMail.Append("<html lang = 'en' >");
                    sbMail.Append("<head >");
                    sbMail.Append("</head >");
                    sbMail.Append("<body >");
                    sbMail.Append("<p> Municipal Enforcement Limited <br />");
                    sbMail.Append("PO Box 11785, Wellington </p>");
                    sbMail.Append("<p> GST Number : 85 150 596 <br />");
                    sbMail.Append("Payment Reference Number: " + transactionNumber + " </p >");
                    sbMail.Append("<p> &nbsp;  </p >");
                    sbMail.Append("<p> Dear Customer,</p >");
                    if (PaymentType == "CCPayment")
                        sbMail.Append("<p> This email is to confirm that your Bank card was charged $" + totalamount + " and processed for<br /><br />");
                    else
                        sbMail.Append("<p> This email is to confirm that you have successfully transferred from your Bank account $" + totalamount + " for<br /><br />");


                    //sbMail.Append("<p>");
                    foreach (infringement item in items)
                    {
                        if (DateTime.Now > item.DueDate)
                        {
                            sbMail.Append("Infringement number " + item.Number + ", Amount $" + item.AfterDueDate.Value + " <br />");
                        }
                        else
                            sbMail.Append("Infringement number " + item.Number + ", Amount $" + item.Amount + " <br />");
                    }


                    sbMail.Append("</p >");
                    sbMail.Append("<p>");
                    sbMail.Append("Amount	: $" + amount + " <br/>");
                    sbMail.Append("GST		: $" + gst + " <br />");
                    sbMail.Append("Total	: $" + totalamount + " <br/>");
                    sbMail.Append("</p ><br/><br/><br/>");
                    sbMail.Append("<p> Thank you,</p>");
                    sbMail.Append("<p>");
                    sbMail.Append("Municipal Enforcement Limited <br/>");
                    sbMail.Append("Web: www.municipalenforcements.co.nz <br/>");
                    sbMail.Append("Email: info@municipalenforcements.co.nz <br/>");
                    sbMail.Append("</p >");
                    sbMail.Append("</body >");
                    sbMail.Append("</html >");

                    //sending mail

                    //string fromaddr = "getanilat@gmail.com";
                    //string toaddr = "anilkumarkaranam@gmail.com";//TO ADDRESS HERE
                    //string password = "kumar1@3";
                    //string transactionNumber = "TR0000001";

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(fromaddr);
                    message.IsBodyHtml = true;
                    if (PaymentType == "CCPayment")
                        message.Subject = "Credit Card Payment Receipt : " + transactionNumber;
                    else
                        message.Subject = "Bank Payment Receipt : " + transactionNumber;

                    if (Email.Contains(","))
                    {
                        string[] emails = Email.Split(',');
                        foreach (string id in emails)
                        {
                            if (id != null && id.Trim().Length > 0)
                            {
                                message.To.Add(new MailAddress(id.Trim()));
                            }
                        }
                    }
                    else
                        message.To.Add(new MailAddress(Email));
                    //message.To.Add(new MailAddress("teh mus@ahuraconsulting.com"));

                    if (Session["ReceiptEmail"] != null && Email == null)
                        Email = Session["ReceiptEmail"].ToString();
                    else
                        Email = "";

                    if (Email.Trim().Length > 0 && Email.Contains(","))
                    {
                        string[] emails = Email.Split(',');
                        foreach (string id in emails)
                        {
                            if (id != null && id.Trim().Length > 0)
                            {
                                message.To.Add(new MailAddress(id.Trim()));
                            }
                        }
                    }
                    else if (Email.Trim().Length > 0)
                        message.To.Add(new MailAddress(Email));


                    //message.CC.Add("");

                    message.Body = sbMail.ToString();

                    SmtpClient client = new SmtpClient();
                    client.UseDefaultCredentials = false;
                    //client.Credentials = new System.Net.NetworkCredential(fromaddr, password);
                    //client.Host = "smtp.gmail.com";
                    //client.Port = 587;
                    //client.EnableSsl = true;

                    client.Credentials = new System.Net.NetworkCredential(fromaddr, password);
                    client.Host = smtpserver;
                    client.Port = smtpport;
                    client.EnableSsl = false;

                    client.Send(message);
                    _logger.Info("transactionNumber : " + transactionNumber);
                    _logger.Info("Mail Sent successfully!." + Email);
                }
            }
            catch (Exception exp)
            {
                _logger.Info("Error in mail sending." + exp.Message);
            }

        }


        public JsonResult ResendMail(string mailid)
        {
            _logger.Info("Resend mail");

            List<infringement> payitems = Session["payitems"] as List<infringement>;
            string transactionNumber = Session["TransactionNO"].ToString();
            string email = Session["ReceiptEmail"].ToString();
            string paymentoption = Session["PaymentOption"].ToString();

            if (mailid != null && mailid.Trim().Length > 0)
                email = mailid;
            decimal totalamount = Convert.ToDecimal(Session["AmountToBePaid"]);

            if (paymentoption == "F")
                paymentoption = "CCPayment";
            else
                paymentoption = "POLI";

            SendMail(payitems, transactionNumber, email, totalamount, paymentoption);
            return Json("");
        }

        public JsonResult GetImageDetails(string id)
        {
            int imageid = Convert.ToInt16(id);
            _logger.Info("GetImageDetails");
            infringementpicture imgpic = db.infringementpictures
                    .Where(x => x.Id == imageid)
                    //.Select(x => x.Location)
                    .FirstOrDefault();

            return Json(new { Id = imgpic.Id, InfId = imgpic.InfringementId, Latitude = imgpic.Latitude, Longitude = imgpic.Longitude, Location = imgpic.Location, Comments = imgpic.Description }, JsonRequestBehavior.AllowGet);
            //return Json(imgpic, JsonRequestBehavior.AllowGet);
        }


        public JsonResult UpdateImageDetails(string id, string Latitude, string Longitude, string Comments)
        {
            int imageid = Convert.ToInt16(id);
            _logger.Info("GetImageDetails");
            infringementpicture imgpic = db.infringementpictures
                    .Where(x => x.Id == imageid)
                    //.Select(x => x.Location)
                    .FirstOrDefault();

            imgpic.Longitude = Longitude;
            imgpic.Latitude = Latitude;
            imgpic.Description = Comments;
            db.SaveChanges();

            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
            //return Json(imgpic, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveImageDetails(string id, string infid)
        {
            int pid = Convert.ToInt16(id);
            infringementpicture imgpic = db.infringementpictures
                   .Where(x => x.Id == pid)
                   .FirstOrDefault();
            db.infringementpictures.Remove(imgpic);
            db.SaveChanges();
            // return RedirectToAction("/Edit/"+ infid);

            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
            //return Json(imgpic, JsonRequestBehavior.AllowGet);
        }

        #region POLI methods
        // GET: Infringements
        public ActionResult TransSuccess()
        {
            _logger.Info("Acknowledgement of payment page");

            string TransactionRefNo = "";
            string TransactionStatusCode = "";
            string MerchantReference = "";
            string BankReceipt = "";
            string Email = "";

            string MerchantCode = ConfigurationManager.AppSettings["MerchantCode"];
            string AuthenticationCode = ConfigurationManager.AppSettings["AuthenticationCode"];
            string PoliURL = ConfigurationManager.AppSettings["PoliURL"];

            List<int> IdsToPay = new List<int>();

            var token = Request.Form["Token"];
            if (String.IsNullOrEmpty(token)) { token = Request.QueryString["token"]; }

            //var auth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("SS64000860:98!6tOii25GtT"));

            var auth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(MerchantCode + ':' + AuthenticationCode));

            var myRequest =
                System.Net.WebRequest.Create
                ("https://poliapi.apac.paywithpoli.com/api/Transaction/GetTransaction?token=" + HttpUtility.UrlEncode(token));

            // var myRequest = System.Net.WebRequest.Create(PoliURL+"?token="+ HttpUtility.UrlEncode(token));
            myRequest.Method = "GET";
            myRequest.Headers.Add("Authorization", "Basic " + auth);

            var response = (System.Net.HttpWebResponse)myRequest.GetResponse();
            var data = response.GetResponseStream();
            var streamRead = new StreamReader(data);
            Char[] readBuff = new Char[response.ContentLength];
            int count = streamRead.Read(readBuff, 0, (int)response.ContentLength);
            while (count > 0)
            {
                var outputData = new String(readBuff, 0, count);
                Console.Write(outputData);
                count = streamRead.Read(readBuff, 0, (int)response.ContentLength);
                dynamic latest = Newtonsoft.Json.JsonConvert.DeserializeObject(outputData);
                ViewBag.Data = latest["TransactionRefNo"];

                TransactionStatusCode = latest["TransactionStatusCode"];
                TransactionRefNo = latest["TransactionRefNo"];
                MerchantReference = latest["MerchantReference"];
                BankReceipt = latest["BankReceipt"];


                _logger.Info("TransactionRefNo: " + latest["TransactionRefNo"]);
                _logger.Info("TransactionStatusCode: " + latest["TransactionStatusCode"]);

                _logger.Info("outputData: " + outputData);
                _logger.Info("latest: " + latest);
            }
            response.Close();
            data.Close();
            streamRead.Close();
            List<infringement> payitems = new List<infringement>();
            if (TransactionStatusCode.Trim().ToLower() == "completed")
            {
                string InfringementIds = Session["InfringementIds"].ToString();
                _logger.Info("InfringementIds: " + InfringementIds);
                if (InfringementIds != null && InfringementIds.Length > 0)
                {
                    //InfringementIds = InfringementIds.Substring(0, InfringementIds.Length - 1);
                    //_logger.Info("InfringementIds: " + InfringementIds);
                    string[] ids = InfringementIds.Split(',');
                    foreach (string id in ids)
                    {
                        if (id != null && id.Trim().Length > 0)
                        {
                            IdsToPay.Add(Convert.ToInt16(id));

                            int infringementNumber = int.Parse(id);
                            var infringement = db.infringements.FirstOrDefault(x => x.Id == infringementNumber && x.StatusId == 1);

                            if (infringement != null)
                            {
                                _logger.Info("Bank Payment successful, change infringement status");
                                infringement.StatusId = 2;
                                infringement.PaymentType = "POLI";
                                var infringementPayment = new infringement_payment
                                {
                                    InfringementId = infringementNumber,
                                    PaymentResult = "2",
                                    TransactionNumber = TransactionRefNo,
                                    TransactionDate = DateTime.Now
                                };

                                _logger.Info("Bank Infringement payment record created" + infringementPayment);

                                db.infringement_payment.Add(infringementPayment);
                            }
                        }
                    }

                    try
                    {
                        db.SaveChanges();
                        _logger.Info("Bank Infringement payment record saved");
                    }
                    catch (Exception ex)
                    {
                        _logger.Info("Error Occured: While updating the status." + TransactionRefNo);
                    }
                }


                if (IdsToPay.Count > 0)
                {
                    int[] infringementIds = IdsToPay.ToArray();
                    payitems = db.infringements
                     .Where(x => infringementIds.Contains(x.Id)
                         ).ToList();

                    //decimal totalamount = payitems.Sum(x => x.Amount);
                    decimal totalamount = 0;
                    foreach (infringement info in payitems)
                    {
                        if (DateTime.Now > info.DueDate)
                        {
                            info.ActualAmountToPay = info.AfterDueDate.Value;
                            //info.DisplayDueAmount = "<b>" + info.AfterDueDate.Value + "</b>";
                            info.DisplayDueAmount = info.AfterDueDate.Value.ToString();
                            info.DisplayAmount = info.Amount.ToString();
                        }
                        else
                        {
                            info.ActualAmountToPay = info.Amount;
                            info.DisplayDueAmount = info.AfterDueDate.ToString();
                            //info.DisplayAmount = "<b>" + info.Amount.ToString() + "</b>"; 
                            info.DisplayAmount = info.Amount.ToString();
                        }

                        totalamount += info.ActualAmountToPay;
                    }
                    ViewData["totalamount"] = totalamount;

                    Session["payitems"] = payitems;
                    Session["TransactionNO"] = TransactionRefNo;
                    Email = Session["ReceiptEmail"].ToString();
                    SendMail(payitems, TransactionRefNo, Email, totalamount, "BankPayment");
                }
            }


            return View("FinesAcknowledgement", model: payitems);
        }

        // GET: Infringements
        public ActionResult TransFailure()
        {
            _logger.Info("Acknowledgement of payment page for POLI ");

            return View("Poliresponse");
        }

        // GET: Infringements
        public ActionResult TransCancelled()
        {
            _logger.Info("Acknowledgement of payment page for POLI Cancelled");
            List<infringement> infringementlist = Session["SearchList"] as List<infringement>;
            ViewData["NoOfRecords"] = infringementlist.Count;
            ViewBag.NoOfRecords = infringementlist.Count;
            _logger.Info("Payment Cancelled.");
            ModelState.AddModelError("", "Payment Cancelled.");
            return View("SearchList", model: infringementlist);

            //return View("TransCancelled");
        }

        // GET: Infringements
        public ActionResult nudge()
        {
            _logger.Info("Acknowledgement of payment page for POLI nudge");

            return View("Poliresponse");
        }
        #endregion

    }
}
