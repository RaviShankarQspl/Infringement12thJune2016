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
   
    public class DownloadInfringementController : BaseController
    {
        private int PageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]);
        private infringementEntities db = new infringementEntities();
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index(int page = 1)
        {
            var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).OrderByDescending(x => x.IncidentTime).Skip((page - 1) * PageSize).Take(PageSize).ToList();


            ViewBag.CurrentPage = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalPages = Math.Ceiling((double)db.infringements.Count() / PageSize);

            return View(infringements);
        }

        // GET: Infringements
        public ActionResult IndexNew()
        {
            int page = 1;
            _logger.Info("List all infringements");
            //var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).OrderByDescending(x => x.IncidentTime).ToList();

            var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype).OrderByDescending(x => x.IncidentTime).Skip((page - 1) * PageSize).Take(PageSize).ToList();


            ViewBag.CurrentPage = page;
            ViewBag.PageSize = PageSize;
            ViewBag.TotalPages = Math.Ceiling((double)infringements.Count() / PageSize);

            if (infringements == null || infringements.Count == 0)
                ModelState.AddModelError("", "No data available.");

            return View(infringements);

            //_logger.Info("List all infringements");
            //var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype);
            //List<infringement> infringementlist = infringements.ToList();
            //if (infringementlist != null && infringementlist.Count > 0)
            //{
            //    using (infringementEntities entity = new infringementEntities())
            //    {
            //        foreach (infringement infr in infringementlist)
            //        {
            //            infr.ImagePath = (from d in entity.infringementstatus where d.Id == infr.StatusId select d.Name).FirstOrDefault();
            //        }
            //    }

            //}
            //return View(infringementlist);

        }

        [HttpPost]
        public ActionResult Index(string SearchRegoNumber, string SearchInfringeNumber, string SearchFrom, string SearchTo)
        {
            _logger.Info("Search for infringement");
            var infringements = db.infringements.Include(i => i.parking_location).Include(i => i.make).Include(i => i.infringementtype);

            if (!string.IsNullOrEmpty(SearchRegoNumber))
            {
                _logger.Info("Search on rego number");
                return View(infringements
                    .Where(x => x.Rego.Trim().ToUpper().Contains(SearchRegoNumber.Trim().ToUpper()))
                    .ToList());
            }
            else if (!string.IsNullOrEmpty(SearchInfringeNumber))
            {
                _logger.Info("Search on infringement number");
                return View(infringements
                    .Where(x => x.Number.Trim().ToUpper().Contains(SearchInfringeNumber.Trim().ToUpper()))
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
                DateTime datefom = Convert.ToDateTime(SearchFrom);
                DateTime dateto = Convert.ToDateTime(SearchTo);

                DateTime startdate = new DateTime(datefom.Year, datefom.Month, datefom.Day);
                DateTime enddate = new DateTime(dateto.Year, dateto.Month, dateto.Day, 23, 59, 59);


                //var abc = infringements
                //    .Where(x => x.IncidentTime >= startdate && x.IncidentTime <= enddate)
                //    .ToList();

                _logger.Info("Search on infringement date range");
                //return View(infringements
                //    .Where(x => x.IncidentTime >= datefom && x.IncidentTime <= dateto)
                //    .ToList());

                return View(infringements
                    .Where(x => x.IncidentTime >= startdate && x.IncidentTime <= enddate)
                    .ToList());
            }
            else
            {
                return View(infringements.ToList());
            }
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
                infringementlist = infringements
                                .Where(x => x.Rego.Trim().ToUpper() == model.SearchOnRegoNumber.Trim().ToUpper() & x.Number.Trim().ToUpper() == model.SearchString.Trim().ToUpper() & x.StatusId == 1)
                                .ToList();
                // return View(infringementlist);
            }
            else if (model.SearchOnRegoNumber != null)
            {
                _logger.Info("Search on rego number");
                infringementlist = infringements
                    .Where(x => x.Rego.Trim().ToUpper() == model.SearchOnRegoNumber.Trim().ToUpper() && x.StatusId == 1)
                    .ToList();
                //return View(infringementlist);
            }
            else if (model.SearchString != null)
            {
                _logger.Info("Search on infringement number");
                infringementlist = infringements
                    .Where(x => x.Number.Trim().ToUpper() == model.SearchString.Trim().ToUpper() && x.StatusId == 1)
                    .ToList();

                //return View(infringementlist);
            }

            if (infringementlist != null && infringementlist.Count > 0)
            {
                ViewData["NoOfRecords"] = infringementlist.Count;
                ViewBag.NoOfRecords = infringementlist.Count;
                Session["SearchList"] = infringementlist;
                return View(infringementlist);
            }
            else
            {
                ModelState.AddModelError("", "No Records Found with given craiteria. or Payment already done for this Rego Number / Infringement Number.");
                return View("Search");
            }

        }


        [HttpPost]
        public ActionResult PayNow(List<InfringementWeb.infringement> model, string Email, string InfringementIds)
        {
            List<infringement> infringementlist = Session["SearchList"] as List<infringement>;
            SearchInfringementModel modeltemp = Session["SearchInfringementModel"] as SearchInfringementModel;
            if (Email == null || Email.Trim() == "")
            {
                ModelState.AddModelError("", "Please enter the your email and selecct at least one Infringement for payment.");
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


                var totalAmount = db.infringements
                    .Where(x => infringementIds.Contains(x.Id) &&
                        x.StatusId == 1).Sum(x => x.Amount);
                if (totalAmount > 0)
                {
                    Session["ReceiptEmail"] = Email;

                    return RedirectToPaymentPage(totalAmount, infringementIds, Email);
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

        }

        private ActionResult RedirectToPaymentPage(decimal amount, int[] infringementIds, string Email)
        {
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
            ViewBag.Cities = new SelectList(db.cities, "id", "name");
            ViewBag.Makes = new SelectList(db.makes, "id", "Name");
            ViewBag.PLocations = new SelectList(db.parking_location.Where(x => x.Id == 999), "Id", "Name");
            ViewBag.Models = new SelectList(db.carmodels.Where(x => x.Id == 999), "Id", "Name");
            ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");


            ViewBag.InfringementTypes = new SelectList(db.infringementtypes, "Id", "Type");

            Session["ImageList"] = new List<ImageUpload>();
            TempData["GalleryImageScroll"] = -1;

            return View(new InfringementModel());
        }


        public ActionResult GetImage(string Id)
        {
            int id = -1;
            int.TryParse(Id, out id);
            if (id > -1)
            {
                List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                byte[] imageData = null;

                //byte[] imageData = imageUploads[id].Image;

                if (imageUploads != null && imageUploads.Count >= id && imageUploads[id] != null && imageUploads[id].Image != null)
                    imageData = imageUploads[id].Image;

                return File(imageData, imageUploads[id].ContentType); // Might need to adjust the content type based on your actual image type
            }
            else
            {
                return null;
            }
        }


        // POST: Infringements/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IncidentTime,Number,Rego,CityId,ParkingLocationId,MakeId,ModelId,OtherMake,OtherModel,InfringementTypeId,Amount,Comment,User,UploadTime,Latitude,Longitude,DueDate,AfterDueDate,OwnerName,Street1,Street2,Suburb,PostCode,CountryId,CityName")] InfringementModel infringement, string Submit, string GalleryImageScroll, HttpPostedFileBase upload)
        {
            using (log4net.NDC.Push("Create infringement post"))
            {
                if (Submit == "Add Photo")
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

                            List<ImageUpload> imageUploads = (List<ImageUpload>)Session["ImageList"];
                            imageUploads.Add(imageUpload);
                            Session["ImageList"] = imageUploads;

                            int galleryImageScroll = -1;
                            int.TryParse(GalleryImageScroll, out galleryImageScroll);
                            TempData["GalleryImageScroll"] = galleryImageScroll + 1;
                        }
                    }

                    ViewBag.Cities = new SelectList(db.cities, "id", "name", infringement.CityId);
                    ViewBag.PLocations = new SelectList(db.parking_location.Where(x => x.CityId == infringement.CityId).OrderBy(x => x.SortOrder), "Id", "Name", infringement.ParkingLocationId);
                    ViewBag.Makes = new SelectList(db.makes, "id", "Name");
                    ViewBag.InfringementTypes = new SelectList(db.infringementtypes, "Id", "Type");
                    ViewBag.Models = new SelectList(db.carmodels.Where(x => x.Id == 0), "Id", "Name");
                    ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");

                    return View(infringement);
                }
                else if (Submit == "Save Infringement")
                {
                    var errors = ModelState
                                .Where(x => x.Value.Errors.Count > 0)
                                .Select(x => new { x.Key, x.Value.Errors })
                                .ToArray();

                    if (ModelState.IsValid)
                    {
                        _logger.Info("model is valid, mapping to entity" + infringement);
                        try
                        {
                            infringement.User = "10";

                            var infring = db.infringements.FirstOrDefault(x => x.Number == infringement.Number);
                            if (infring != null)
                                infringement.Number = null;

                            var entityModel = MvcModelToDatabaseModelMapper.MapInfringementForCreate(infringement);

                            entityModel.CreatedBy = (int)Session["UserId"];
                            entityModel.CreatedDate = System.DateTime.Now;

                            entityModel.Pay = false;
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
                                                Value = infringement.Longitude
                                            });

                                            request.AddParameter(new Parameter
                                            {
                                                Name = "Latitude",
                                                Type = ParameterType.GetOrPost,
                                                Value = infringement.Latitude
                                            });

                                            request.AddParameter(new Parameter
                                            {
                                                Name = "Description",
                                                Type = ParameterType.GetOrPost,
                                                Value = imageUpload.FileName
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
                            ViewBag.Cities = new SelectList(db.cities, "id", "name");
                            ViewBag.Makes = new SelectList(db.makes, "id", "Name");
                            ViewBag.InfringementTypes = new SelectList(db.infringementtypes, "Id", "Type");
                            ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");
                            return View(infringement);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn("Saving of infrin. entity failed", ex);
                            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                        }

                        return RedirectToAction("Index");
                    }

                    _logger.Info("Infringement model is not valid, return back to create view");
                    ViewBag.Cities = new SelectList(db.cities, "id", "name");
                    ViewBag.Makes = new SelectList(db.makes, "id", "Name");
                    ViewBag.InfringementTypes = new SelectList(db.infringementtypes, "Id", "Type");
                    ViewBag.PLocations = new SelectList(db.parking_location.Where(x => x.CityId == infringement.CityId).OrderBy(x => x.SortOrder), "Id", "Name", infringement.ParkingLocationId);
                    ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");
                    return View(infringement);
                }

                return RedirectToAction("Index");
            }
        }

        // GET: Infringements/Edit/5
        public ActionResult Edit(int? id)
        {
            using (log4net.NDC.Push("Edit Infringemnet - GET"))
            {
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

                ViewBag.Images = db.infringementpictures
                    .Where(x => x.InfringementId == id)
                    .Select(x => x.Location)
                    .ToList();
                var infringementCity = db.parking_location
                    .FirstOrDefault(x => x.Id == infringement.ParkingLocationId).CityId;
                ViewBag.Cities = new SelectList(db.cities, "Id", "Name", infringementCity);
                ViewBag.Buildings = new SelectList(db.parking_location, "Id", "Name", infringement.ParkingLocationId);
                ViewBag.Makes = new SelectList(db.makes, "id", "Name", infringement.MakeId);
                ViewBag.InfringementTypes = new SelectList(db.infringementtypes, "Id", "Type", infringement.InfringementTypeId);
                ViewBag.Status = new SelectList(db.infringementstatus, "Id", "Name", infringement.StatusId);
                var model = MvcModelToDatabaseModelMapper.MapInfringementForDisplay(infringement);
                ViewBag.IncidentTime = model.IncidentTime.ToString("dd/MM/yyyy HH:mm");
                ViewBag.Models = new SelectList(db.carmodels.Where(x => x.Id == infringement.MakeId), "Id", "Name");
                ViewBag.Countries = new SelectList(db.countries.OrderBy(x => x.CountryName), "Id", "CountryName");

                ViewBag.APIServerPath = System.Configuration.ConfigurationManager.AppSettings["APIServerPath"].ToString();
                return View(model);
            }
        }

        // POST: Infringements/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IncidentTime,Number,Rego,ParkingLocationId,MakeId,ModelId,OtherMake,OtherModel,CarModel,InfringementTypeId,Amount,Comment,Latitude,Longitude,User,StatusId,,DueDate,AfterDueDate,Name,Street1,Street2,Suburb,PostCode,CountryId,CityName")] InfringementModel model)
        {
            using (log4net.NDC.Push("Post for editing infrin."))
            {
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
                        entity.User = "10";
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Infrin. could not be updated", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);

                    }
                    return RedirectToAction("Index");
                }
                return View(model);
            }
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
                string transactionNumber = "";
                for (int i = 0; i < Request.Form.AllKeys.Length; i++)
                {
                    string key = Request.Form.AllKeys[i];
                    string value = Request.Form[i];
                    PostData += key + "=" + Server.UrlEncode(value) + "&";

                    if (key == "txn_id")
                        transactionNumber = value;

                }

                _logger.Info("Response From Payment Gateway : " + PostData);

                //int[] infringementIds = Array.ConvertAll(Request.Form["custom_data"].ToString().Split(','), int.Parse);

                string[] custom_data = Request.Form["custom_data"].ToString().Split('$');

                int[] infringementIds = Array.ConvertAll(custom_data[0].ToString().Split(','), int.Parse);


                payitems = db.infringements
                   .Where(x => infringementIds.Contains(x.Id)
                   // && x.StatusId == 1
                   ).ToList();

                SendMail(payitems, transactionNumber, custom_data[1].ToString());

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

        private void SendMail(List<infringement> items, string transactionNumber, string Email)
        {
            try
            {
                if (items != null && items.Count > 0)
                {
                    if (transactionNumber == null)
                        transactionNumber = db.infringement_payment.Where(x => x.InfringementId == items[0].Id).Select(x => x.TransactionNumber).ToString();

                    decimal totalamount = items.Sum(x => x.Amount);
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
                    sbMail.Append("<p> Municipal Enforcements <br />");
                    sbMail.Append("PO Box 11785, Wellington </p>");
                    sbMail.Append("<p> GST Number:123456789 <br />");
                    sbMail.Append("Payment Reference Number: " + transactionNumber + " </p >");
                    sbMail.Append("<p> &nbsp;  </p >");
                    sbMail.Append("<p> Dear Customer,</p >");
                    sbMail.Append("<p> This email is to confirm that your credit card was charged $" + totalamount + " and processed for</p>");
                    sbMail.Append("<p>");
                    foreach (infringement item in items)
                    {
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
                    sbMail.Append("Municipal enforcements <br/>");
                    sbMail.Append("Web: www.municipalenforcements.co.nz <br/>");
                    sbMail.Append("Email: info@municipalenforcements.co.nz <br/>");
                    sbMail.Append("</p >");
                    sbMail.Append("</body >");
                    sbMail.Append("</html >");

                    //sending mail

                    string fromaddr = "getanilat@gmail.com";
                    string toaddr = "anilkumarkaranam@gmail.com";//TO ADDRESS HERE
                    string password = "kumar1@3";
                    //string transactionNumber = "TR0000001";

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("admin@infringement.com");
                    message.IsBodyHtml = true;

                    message.Subject = "Credit Card Payment Receipt :: " + transactionNumber;

                    message.To.Add(new MailAddress(Email));
                    //message.To.Add(new MailAddress("tehmus@ahuraconsulting.com"));
                    if (Session["ReceiptEmail"] != null)
                        message.To.Add(new MailAddress(Session["ReceiptEmail"].ToString()));


                    //message.CC.Add("");

                    message.Body = sbMail.ToString();

                    SmtpClient client = new SmtpClient();
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(fromaddr, password);
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.EnableSsl = true;
                    client.Send(message);
                    _logger.Info("Mail Sent successfully!." + Email);
                }
            }
            catch (Exception exp)
            {
                _logger.Info("Error in mail sending." + exp.Message);
            }

        }
    }
}
