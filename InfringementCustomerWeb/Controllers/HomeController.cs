using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InfringementCustomerWeb.Models;
using System.Text;
using System.Net.Mail;
using Newtonsoft.Json;

namespace InfringementCustomerWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "";
            ViewBag.reCAPTCHAClient = System.Configuration.ConfigurationManager.AppSettings["reCAPTCHAClient"];
            //ViewBag.RandomString = RandomString(6).ToUpper();
            return View(new ContactUsModel());
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost]
        public ActionResult ContactMail(ContactUsModel model)
        {
            ViewBag.reCAPTCHAClient = System.Configuration.ConfigurationManager.AppSettings["reCAPTCHAClient"];

            _logger.Info("contact us : " + model.CustomerName +" ::  "+ model.Subject + " ::  " + model.Email + " ::  " + model.Message);
            ViewBag.Message = model.Message;
            ViewBag.Subject = model.Subject;

            string EncodedResponse = Request.Form["g-Recaptcha-Response"];
            bool IsCaptchaValid = (ReCaptchaClass.Validate(EncodedResponse) == "True" ? true : false);
            if (IsCaptchaValid)
            {
                infringementEntities db = new infringementEntities();
                contactu contactus = new contactu();

                contactus.SubjectText = model.Subject;
                contactus.CustomerName = model.CustomerName;
                contactus.Email = model.Email;
                contactus.SubjectText = model.Subject;
                contactus.BodyContent = model.Message;
                contactus.CreatedDate = DateTime.Now;

                string VisitorsIPAddr = string.Empty;
                if (HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    VisitorsIPAddr = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
                else if (HttpContext.Request.UserHostAddress.Length != 0)
                {
                    VisitorsIPAddr = HttpContext.Request.UserHostAddress;
                }
                contactus.ClientIp = VisitorsIPAddr;
                contactus.BrowserName = Request.Browser.Browser;
                contactus.Status = 0;

                db.contactus.Add(contactus);
                db.SaveChangesAsync();

                //sending mail
                StringBuilder sb = new StringBuilder();
                sb.Append("<p>The following has been sent from contact us page.</p>");
                sb.Append("<p>Submit Date & Time: " + GetCurrentTime().ToString("dd-MM-yyyy HH:mm") + " </p>");

                sb.Append("<p>Name : " + model.CustomerName + " ");
                sb.Append("<p>Email : " + model.Email + " ");
                sb.Append("<p>Message Type : " + model.Subject + " </p>");
                sb.Append("<p> Infringement Number :" + model.InfringementNumber + " </p>");
                sb.Append("<p> Message :" + model.Message + " </p>");
                //sb.Append("<p>&nbsp;</p>");
                //sb.Append("<p>Thanks</p>");   
                //sb.Append("<p>Online Contact us web form -Customer Portal.</p>");

                string emailid = db.emailids.FirstOrDefault(x => x.EmailTitle == "contactus").Email;

                SendMail(emailid, model.Subject, sb.ToString());

                return View("MailResponse");
            }
            else
            {
                ModelState.AddModelError("", "Please select valid recaptcha.");
                return View("Contact", model);
            }
        }

        protected DateTime GetCurrentTime()
        {
            DateTime serverTime = DateTime.Now;
            DateTime _localTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(serverTime, TimeZoneInfo.Local.Id, "New Zealand Standard Time");
            return _localTime;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(GetCurrentTime());
        }

        private void SendMail(string Email, string Subject, string BodyText)
        {
            string fromaddr = System.Configuration.ConfigurationManager.AppSettings["FromUserEmail"];
            string password = System.Configuration.ConfigurationManager.AppSettings["FromUserEmailPwd"];
            int smtpport = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["SMTPPort"]);
            string smtpserver = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
            //string toaddr = "anilkumarkaranam@gmail.com";//TO ADDRESS HERE

            try
            {

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(fromaddr);
                    message.IsBodyHtml = true; 
                    message.Subject = Subject;

                    //Email = "anilkumarkaranam@gmail.com";

                    message.To.Add(new MailAddress(Email));
                //message.To.Add(new MailAddress("tehmus@ahuraconsulting.com"));

                    message.Body = BodyText;

                    SmtpClient client = new SmtpClient();
                    client.UseDefaultCredentials = false;
                    

                    client.Credentials = new System.Net.NetworkCredential(fromaddr, password);
                    client.Host = smtpserver;
                    client.Port = smtpport;
                    client.EnableSsl = false;

                    client.Send(message);
                    _logger.Info("BodyText : " + BodyText);
                    _logger.Info("Mail Sent successfully from contact us!...." + Email);
                
            }
            catch (Exception exp)
            {
                _logger.Info("Error in mail sending - Contact us" + exp.Message);
            }

        }


        public ActionResult ErrorPage()
        {

            return View();
        }
    }

    public class ReCaptchaClass
    {
        public static string Validate(string EncodedResponse)
        {
            var client = new System.Net.WebClient();

            //string PrivateKey = "6LdgRh0TAAAAAKPB2UR-2hQ3_h9SNJnPLSWabat8";
            string PrivateKey = System.Configuration.ConfigurationManager.AppSettings["reCAPTCHAServer"];

            var GoogleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", PrivateKey, EncodedResponse));

            var captchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ReCaptchaClass>(GoogleReply);

            return captchaResponse.Success;
        }

        [JsonProperty("success")]
        public string Success
        {
            get { return m_Success; }
            set { m_Success = value; }
        }

        private string m_Success;
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes
        {
            get { return m_ErrorCodes; }
            set { m_ErrorCodes = value; }
        }


        private List<string> m_ErrorCodes;
    }
}