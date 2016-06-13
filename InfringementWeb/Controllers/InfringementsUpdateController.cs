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
using System.Net.Mail;

namespace InfringementWeb.Controllers
{
    

    public class InfringementsUpdateController : Controller
    {
        private infringementEntities db = new infringementEntities();
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        

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

                        SendMail(payitems, transactionNumber, Email, totalamount);
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

        private void SendMail(List<infringement> items, string transactionNumber, string Email, decimal totalamount)
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
                    sbMail.Append("<p> This email is to confirm that your credit card was charged $" + totalamount + " and processed for :<br /><br />");
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
                    sbMail.Append("<p>");
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

                    message.Subject = "Credit Card Payment Receipt – " + transactionNumber;


                    //message.To.Add(new MailAddress(Email));
                    //if (Session["ReceiptEmail"] != null)
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
                    else if (Email.Trim().Length > 0)
                        message.To.Add(new MailAddress(Email));
                    
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
                    _logger.Info("Mail Sent successfully!...." + Email);
                }
            }
            catch(Exception exp)
            {
                _logger.Info("Error in mail sending" + exp.Message);
            }

        }

        // GET: InfringementsUpdate
        public ActionResult FinesAcknowledgement()
        {
            _logger.Info("Acknowledgement of payment page");

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

            //MNS implementation end

            

            if (Request.Form["custom_data"] != null)
            {
                List<infringement> payitems = new List<infringement>();
                string PostData = "";
                for (int i = 0; i < Request.Form.AllKeys.Length; i++)
                {
                    string key = Request.Form.AllKeys[i];
                    string value = Request.Form[i];
                    PostData += key + "=" + Server.UrlEncode(value) + "&";
                }

                _logger.Info("Response From Payment Gateway : " + PostData);

                int[] infringementIds = Array.ConvertAll(Request.Form["custom_data"].ToString().Split(','), int.Parse);


                payitems = db.infringements
                   .Where(x => infringementIds.Contains(x.Id) &&
                       x.StatusId == 1).ToList();
                return View("FinesAcknowledgement", model: payitems);
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
    }
}
