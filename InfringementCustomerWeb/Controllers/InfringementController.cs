using InfringementCustomerWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;


namespace InfringementCustomerWeb.Controllers
{
    public class InfringementController : Controller
    {
        private infringementEntities db = new infringementEntities();
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Infringements
        public ActionResult Search()
        {
            _logger.Info("Render page to search for infrngements");
            return View(new SearchInfringementModel());
        }

        [HttpPost]
        public ActionResult SearchList(SearchInfringementModel model, string Submit)
        {
            ViewBag.NoOfRecords = 0;
            ViewData["NoOfRecords"] = 0;
            Session["SearchInfringementModel"] = model;

            _logger.Info("Search for infringement");
            var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).OrderBy(x => x.IncidentTime);
            List<infringement> infringementlist = new List<infringement>();

            if (Submit == "Search By Registration Number")
            {
                if (model.SearchOnRegoNumber == null)
                {
                    ModelState.AddModelError("", "Please enter Registration Number.");
                    return View("Search");
                }
                else
                {
                    _logger.Info("Search on rego number");
                    try {
                        infringementlist = infringements
                            .Where(x => x.Rego.Trim().ToUpper() == model.SearchOnRegoNumber.Trim().ToUpper() && x.StatusId == 1)
                            .ToList();
                    }
                    catch(Exception ex)
                    {
                        string temp = ex.Message;
                    }
                }
            }
            else if (Submit == "Search by Infringement Number")
            {

                if (model.SearchString == null)
                {
                    ModelState.AddModelError("", "Please enter Infringement Number.");
                    return View("Search");
                }
                else
                {
                    try
                    { 
                    _logger.Info("Search on infringement number");
                    infringementlist = infringements
                        .Where(x => x.Number.Trim().ToUpper() == model.SearchString.Trim().ToUpper() && x.StatusId == 1)
                        .ToList();
                    }
                    catch (Exception ex)
                    {
                        string temp = ex.Message;
                    }
                }

                //if (infringementlist != null && infringementlist.Count > 0)
                //{
                //    ViewData["NoOfRecords"] = infringementlist.Count;
                //    ViewBag.NoOfRecords = infringementlist.Count;
                //    Session["SearchList"] = infringementlist;
                //}

                //return View("Search");
            }

            if (infringementlist != null && infringementlist.Count > 0)
            {
                //checking the due date and amount
                foreach(infringement info in infringementlist)
                {
                    if (DateTime.Now > info.DueDate)
                    {
                        info.ActualAmountToPay = info.AfterDueDate.Value;
                        //info.DisplayDueAmount = "<b>" + info.AfterDueDate.Value + "</b>";
                        info.DisplayDueAmount = info.AfterDueDate.Value.ToString() ;
                        info.DisplayAmount = info.Amount.ToString() ;
                    }
                    else
                    {
                        info.ActualAmountToPay = info.Amount;
                        info.DisplayDueAmount = info.AfterDueDate.ToString();
                        //info.DisplayAmount = "<b>" + info.Amount.ToString() + "</b>"; 
                        info.DisplayAmount =  info.Amount.ToString() ;
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
        public ActionResult PayNow(List<InfringementCustomerWeb.infringement> model, string Email, string InfringementIds, string hdnNoOfRecords, string txtTotalAmount, string Submit)
        {
            List<infringement> infringementlist = new List<infringement>();
            if (Session["SearchList"] != null)
                 infringementlist = Session["SearchList"] as List<infringement>;

            if (InfringementIds.Trim() == Email.Trim())
                InfringementIds = "";

            ViewData["NoOfRecords"] = infringementlist.Count;
            ViewBag.NoOfRecords = infringementlist.Count;
            Session["AmountToBePaid"] = txtTotalAmount;
            ViewBag.Email = Email;
            ViewBag.TotalAmount = txtTotalAmount;
            Session["PaymentOption"] = "F";

            SearchInfringementModel modeltemp = Session["SearchInfringementModel"] as SearchInfringementModel;
            if (Email == null || Email.Trim() == "")
            {
                ModelState.AddModelError("", "Email is missing . Please enter a valid email address for us to send you the transaction receipt.");
                return View("SearchList", infringementlist);
            }

            

           if (Email == null || Email.Trim() == "")
            {
                ModelState.AddModelError("", "Please enter the your email and select minimum one Infringement for payment..");
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

            string selectedRecs = InfringementIds;

            List<int> IdsToPay = new List<int>();
            if (InfringementIds != null && InfringementIds.Length > 0)
            {
                InfringementIds = InfringementIds.Substring(0, InfringementIds.Length - 1);
                Session["InfringementIds"] = InfringementIds;

                string[] ids = InfringementIds.Split(',');
                foreach (string id in ids)
                {
                    IdsToPay.Add(Convert.ToInt16(id));
                }
            }

            //foreach (infringement infringe in model)
            //{
            //    if (infringe.Pay == true)
            //    {
            //        IdsToPay.Add(infringe.Id);
            //    }
            //}

            if (IdsToPay.Count > 0)
            {
                int[] infringementIds = IdsToPay.ToArray();


                //var totalAmount = db.infringements
                //    .Where(x => infringementIds.Contains(x.Id) &&
                //        x.StatusId == 1).Sum(x => x.Amount);
                decimal totalAmount = Convert.ToDecimal(txtTotalAmount);
                Session["AmountToBePaid"] = totalAmount;

                if (totalAmount > 0)
                {
                    Session["ReceiptEmail"] = Email;

                    List<infringement> infringementlistpaid = db.infringements
                    .Where(x => infringementIds.Contains(x.Id)).ToList();
                    Session["infringementlistpaid"] = infringementlistpaid;

                    //return RedirectToPaymentPage(totalAmount, infringementIds, Email);
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

                        List<infringement> infList = db.infringements.Include(i => i.parking_location)
                                .Where(x => infringementIds.Contains(x.Id)).ToList();
                        string particular = "";
                        string reference = "";
                        if (infList != null && infList.Count > 0)
                        {
                            foreach (infringement item in infList)
                            {
                                particular = particular + item.Number + "_";
                            }
                        }
                        particular = particular.Substring(0, particular.Length - 1);

                        if (particular.Trim().Length > 50)
                            particular = particular.Substring(0, 49);

                        reference = "Parking Infringement :" + particular;



                        string requiredParams = @"{'Amount':'" + totalAmount + "', 'CurrencyCode':'NZD', 'MerchantReference':'" + reference + "','MerchantHomepageURL':'" + HomeUrl + "','SuccessURL':'" + successUrl + "','FailureURL':'" + failureUrl + "','CancellationURL':'" + cancelurl + "', 'NotificationURL':'" + notificationurl + "' }";

                        //var json = System.Text.Encoding.UTF8.GetBytes(requiredParams);

                        requiredParams = @"{'Amount':'" + totalAmount + "', 'CurrencyCode':'NZD', 'MerchantReference':'" + particular + "',";

                        //requiredParams = requiredParams + @" 'MerchantHomepageURL':'http://indideveloper-001-site5.atempurl.com/',
                        //          'SuccessURL':'http://indideveloper-001-site5.atempurl.com/Infringement/TransSuccess',
                        //          'FailureURL':'http://indideveloper-001-site5.atempurl.com/Infringement/TransFailure',
                        //          'CancellationURL':'http://indideveloper-001-site5.atempurl.com/Infringement/TransCancelled',
                        //          'NotificationURL':'http://indideveloper-001-site5.atempurl.com/Infringement/nudge'
                        //        }";

                        requiredParams = requiredParams + @" 'MerchantHomepageURL':'" + merchanturl + "','SuccessURL':'" + successUrl + "','FailureURL':'" + failureUrl + "', 'CancellationURL':'" + cancelurl + "', 'NotificationURL':'" + notificationurl + "' }";

                        var json = System.Text.Encoding.UTF8.GetBytes(requiredParams);

                        //var auth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("SS64000860:98!6tOii25GtT"));

                        var auth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(MerchantCode+':'+AuthenticationCode));

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
                ModelState.AddModelError("", "No infringement is selected for payment. Please select the infringements that are to be paid.");
                return View("SearchList", infringementlist);
            }

            return View("SearchList", infringementlist);
        }

        //[HttpPost]
        public void MNS_old()
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
            if(actionResponse.Trim().ToUpper() == "VERIFIED")
            {
               foreach(var infr in customData)
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
                    if(transactionResult == "2")
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
                    try {
                        db.SaveChanges();
                    }catch(Exception ex)
                    {
                        _logger.Error("Payment record not saved in the database", ex);
                        throw;
                    }

                    _logger.Info("MNS completed");
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
            string Email = "";
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
                {
                    IList<String> customDataTemp = value.Split(new char[] { '$' });
                    customData = customDataTemp[0].Split(new char[] { ',' });
                    Email = customDataTemp[1].ToString();
                }
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
                        infringement.PaymentType = "Flow2Cash";
                    }

                    var infringementPayment = new infringement_payment
                    {
                        InfringementId = infringementNumber,
                        PaymentResult = transactionResult,
                        TransactionNumber = transactionNumber,
                        TransactionDate = DateTime.Now
                    };

                    _logger.Info("Infringement payment record created" + infringementPayment);

                    db.infringement_payment.Add(infringementPayment);
                    try
                    {
                        db.SaveChanges();
                        //int[] infringementIds = Array.ConvertAll(Request.Form["custom_data"].ToString().Split(','), int.Parse);
                        string[] custom_data = Request.Form["custom_data"].ToString().Split('$');

                        int[] infringementIds = Array.ConvertAll(custom_data[0].ToString().Split(','), int.Parse);

                        List<infringement> payitems = db.infringements
                           .Where(x => infringementIds.Contains(x.Id)
                           // && x.StatusId == 1
                           ).ToList();

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
                        SendMail(payitems, transactionNumber, Email, totalamount, "CCPayment");
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

        public JsonResult ResendMail(string mailid)
        {
            _logger.Info("Resend mail");
            string paymentoption = "";
            try
            {
                List<infringement> payitems = Session["payitems"] as List<infringement>;
                string transactionNumber = Session["TransactionNO"].ToString();
                string email = Session["ReceiptEmail"].ToString();

                if (mailid != null && mailid.Trim().Length > 0)
                    email = mailid;

                if (paymentoption == "F")
                    paymentoption = "CCPayment";
                else
                    paymentoption = "POLI";

                decimal totalamount = Convert.ToDecimal(Session["AmountToBePaid"]);
                SendMail(payitems, transactionNumber, email, totalamount, paymentoption);
            }
            catch { }
            return Json("");
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

                    //decimal totalamount = items.Sum(x => x.Amount);
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
                    sbMail.Append("Payment Reference Number : " + transactionNumber + " </p >");
                    sbMail.Append("<p> &nbsp;  </p >");
                    sbMail.Append("<p> Dear Customer,</p ><br/>");

                    //sbMail.Append("<p> This email is to confirm that your credit card was charged $" + totalamount + " and processed for :<br/><br/>");

                    if (PaymentType == "CCPayment")
                        sbMail.Append("<p> This email is to confirm that your Bank card was charged $" + totalamount + " and processed for<br /><br />");
                    else
                        sbMail.Append("<p> This email is to confirm that you have paid via your bank account $" + totalamount + " for<br /><br />");

                    //sbMail.Append("<p>");
                    //foreach (infringement item in items)
                    //{
                    //    sbMail.Append("Infringement number " + item.Number + ", Amount $" + item.Amount + " <br />");
                    //}

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
                    sbMail.Append("<p><br/>");
                    sbMail.Append("Amount	: $" + amount + " <br/>");
                    sbMail.Append("GST		: $" + gst + " <br />");
                    sbMail.Append("Total	: $" + totalamount + " <br/>");
                    sbMail.Append("</p ><br/><br/>");
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

                    //message.Subject = "Credit Card Payment Receipt – " + transactionNumber;

                    if (PaymentType == "CCPayment")
                        message.Subject = "Credit Card Payment Receipt : " + transactionNumber;
                    else
                        message.Subject = "Bank Payment Receipt : " + transactionNumber;

                    //message.To.Add(new MailAddress(toaddr));
                    //message.To.Add(new MailAddress("tehmus@ahuraconsulting.com"));
                    //if (Email != null && Email.Trim().Length > 0)
                    //    message.To.Add(new MailAddress(Email));
                    //else if (Session["ReceiptEmail"] != null)
                    //    message.To.Add(new MailAddress(Session["ReceiptEmail"].ToString()));

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
                    //message.To.Add(new MailAddress("tehmus@ahuraconsulting.com"));
                    if (Session["ReceiptEmail"] != null && Session["ReceiptEmail"].ToString() != Email)
                        Email = Session["ReceiptEmail"].ToString();
                    else
                        Email = "";

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
                    else if (Email != "")
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
                    _logger.Info("Mail Sent successfully!...." + Email);
                }
            }
            catch (Exception exp)
            {
                _logger.Info("Error in mail sending" + exp.Message);
            }

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
                    //        reference = reference + item.parking_location.Name + ",";

                    particular = particular + item.Number + ",";
                }
            }
            //if (reference.Trim().Length > 50)
            //    reference = reference.Substring(0, 49);

            reference = "Parking Infringement";

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

        
        // GET: Infringements
        public ActionResult FinesAcknowledgement()
        {
            _logger.Info("Acknowledgement of payment page");
            string transactionResult = String.Empty;
            string response_code = string.Empty;
            #region comment
            //string[] infringementIds = Request.Form["custom_data"].ToString().Split(',');

            //string FLO2CASH_MNS_URL = "https://secure.flo2cash.co.nz/web2pay/MNSHandler.aspx";
            //string[] keys = Request.Form.AllKeys;
            //string ctlAllPostbackData = " ";

            //for (int i = 0; i < keys.Length; i++)
            //{
            //    ctlAllPostbackData += "<b>" + keys[i] + "</b>: " + Request[keys[i]] + "<br />";
            //}

            //MNS implementation

            //WebClient WClient = new WebClient();
            //WClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            //string PostData = "";
            //for (int i = 0; i < Request.Form.AllKeys.Length; i++)
            //{
            //    string key = Request.Form.AllKeys[i];
            //    string value = Request.Form[i];
            //    PostData += key + "=" + Server.UrlEncode(value) + "&";
            //}
            //PostData += "cmd" + "=" + "_xverify-transaction";
            //byte[] PostBytes = Encoding.ASCII.GetBytes(PostData);
            //byte[] ResponseBytes = WClient.UploadData(FLO2CASH_MNS_URL, "POST", PostBytes);
            //string ActionResponse = Encoding.ASCII.GetString(ResponseBytes);
            #endregion
            //MNS implementation end
            ViewData["totalamount"] = 0;
            string transactionNumber = "";
            if (Request.Form["custom_data"] != null)
            {
                List<infringement> payitems = new List<infringement>();
                string PostData = "";
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
                if (response_code == "0" && transactionResult == "2")
                {
                    string[] custom_data = Request.Form["custom_data"].ToString().Split('$');

                    int[] infringementIds = Array.ConvertAll(custom_data[0].ToString().Split(','), int.Parse);


                    payitems = db.infringements
                       .Where(x => infringementIds.Contains(x.Id)
                           ).ToList();
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

                    //decimal totalamount = payitems.Sum(x => x.Amount);
                    ViewData["totalamount"] = totalamount;

                    Session["payitems"] = payitems;
                    Session["TransactionNO"] = transactionNumber;

                    return View("FinesAcknowledgement", model: payitems);
                }
                else
                {
                    List<infringement> infringementlist = Session["SearchList"] as List<infringement>;
                    ViewData["NoOfRecords"] = infringementlist.Count;
                    ViewBag.NoOfRecords = infringementlist.Count;

                    if (Session["ReceiptEmail"] != null)
                        ViewBag.Email = Session["ReceiptEmail"].ToString();

                    _logger.Info("Payment declined.");
                    ModelState.AddModelError("", "Payment Declined.");
                    return View("SearchList", model: infringementlist);
                }
            }
            else
            {
                List<infringement> infringementlist = Session["SearchList"] as List<infringement>;
                ViewData["NoOfRecords"] = infringementlist.Count;
                ViewBag.NoOfRecords = infringementlist.Count;
                if (Session["ReceiptEmail"] != null)
                 ViewBag.Email = Session["ReceiptEmail"].ToString();
                _logger.Info("Payment Cancelled.");
                ModelState.AddModelError("", "Payment Cancelled.");
                return View("SearchList", model: infringementlist);
            }
            //return View("FinesAcknowledgement", model:ctlAllPostbackData);
            //return View("FinesAcknowledgement", null);
        }
                
        public ActionResult TestSendMail()
        {
            string transactionNumber = "ABC";
            string Email = "";
            decimal totalamount = 0;
            try
            {
                _logger.Info("Mail Sent Started!...." );
                string fromaddr = System.Configuration.ConfigurationManager.AppSettings["FromUserEmail"];
                string password = System.Configuration.ConfigurationManager.AppSettings["FromUserEmailPwd"];
                int smtpport = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
                string smtpserver = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                string toaddr = "anilkumarkaranam@gmail.com";//TO ADDRESS HERE


                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromaddr);
                message.IsBodyHtml = true;

                message.Subject = "Credit Card Payment Receipt :: " + transactionNumber;

                message.To.Add(new MailAddress(toaddr));
                //message.To.Add(new MailAddress("tehmus@ahuraconsulting.com"));
                if (Session["ReceiptEmail"] != null)
                    message.To.Add(new MailAddress(Session["ReceiptEmail"].ToString()));


                //message.CC.Add("");

                message.Body = "test mail";

                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = false;
                //client.Credentials = new System.Net.NetworkCredential(fromaddr, password);
                //client.Host = "smtp.gmail.com";
                //client.Port = 587;

                client.Credentials = new System.Net.NetworkCredential(fromaddr, password);
                client.Host = smtpserver;
                client.Port = smtpport;

                client.EnableSsl = false;
                client.Send(message);
                _logger.Info("Mail Sent successfully!...." + Email);


            }
            catch (Exception exp)
            {
                _logger.Info("Mail Sent ERROR :" + exp.Message);
            }
            return View("Search");
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

            //var myRequest = System.Net.WebRequest.Create(PoliURL + "?token=" + HttpUtility.UrlEncode(token));
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

            //return View("TransCancelled");

            List<infringement> infringementlist = Session["SearchList"] as List<infringement>;
            ViewData["NoOfRecords"] = infringementlist.Count;
            ViewBag.NoOfRecords = infringementlist.Count;
            if (Session["ReceiptEmail"] != null)
                ViewBag.Email = Session["ReceiptEmail"].ToString();

            _logger.Info("POLI Payment Cancelled.");
            ModelState.AddModelError("", "POLI Payment Cancelled.");
            return View("SearchList", model: infringementlist);
        }

        // GET: Infringements
        public ActionResult nudge()
        {
            _logger.Info("Acknowledgement of payment page for POLI nudge");

            return View("Poliresponse");
        }
        #endregion

        /// <summary>
        /// On Exception, Exception will be handled.
        /// </summary>
        /// <param name="filterContext">Exception Context object.</param>
        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            ////Session["ErrorMessage"] = filterContext.Exception;
            this.TempData["ErrorMessage"] = filterContext.Exception;
            _logger.Warn("Error :" + filterContext.Exception.Message);
            try
            {
                string action = filterContext.RouteData.Values["action"].ToString();
                string returnType = filterContext.Controller.GetType().GetMethod(action).ReturnType.Name;

                if (returnType == "ActionResult")
                    this.TempData["ViewType"] = true;
            }
            catch (Exception ex) { }


            filterContext.Result = this.Redirect("/Home/ErrorPage");
        }
    }
}
