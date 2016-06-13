using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InfringementWeb;
using System.IO;
using RestSharp;

namespace InfringementWeb.Controllers
{
    public class InfringementPicturesController : BaseController
    {
        private infringementEntities db = new infringementEntities();
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // GET: Infringements
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            infringementpicture infringementpicture = db.infringementpictures.Find(id);
            if (infringementpicture == null)
            {
                return HttpNotFound();
            }
            return View(infringementpicture);
        }

        public ActionResult Create()
        {
            return View(new ImageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ImageViewModel model)
        {
            using (log4net.NDC.Push("Create Infringement Image - POST"))
            {
                var validImageTypes = new string[]
                {
                "image/gif",
                "image/jpg",
                "image/jpeg",
                "image/pjpeg",
                "image/png"
                };

                if (!ModelState.IsValid)
                {
                    _logger.Warn("Infringement Number is reuqired");
                    ModelState.AddModelError("InfringementNumber", "Infringement Number is required");
                }
                else if (model.ImageUpload == null || model.ImageUpload.ContentLength == 0)
                {
                    _logger.Warn("Image upload path and content length is required");
                    ModelState.AddModelError("ImageUpload", "This field is required");
                }
                else if (!validImageTypes.Contains(model.ImageUpload.ContentType))
                {
                    _logger.Warn("Please choose either a GIF, JPG or PNG image." + model.ImageUpload.ContentType);
                    ModelState.AddModelError("ImageUpload", "Please choose either a GIF, JPG or PNG image.");
                }
                else if (db.infringements.FirstOrDefault(x => x.Number == model.InfringementNumber) == null)
                {
                    _logger.Warn("Infringement number does not exist in the database");
                    ModelState.AddModelError("infringementNumber", "Infringement Number does not exist");
                }
                else
                {

                    var client = new RestClient("http://localhost:50247/api");

                    //var client = new RestClient("http://localhost:49914/api");

                    IRestRequest request = new RestRequest(
                        String.Format("infringement/{0}/images", model.InfringementNumber)
                        , Method.POST);

                    byte[] fileData = null;
                    using (var binaryReader = new BinaryReader(model.ImageUpload.InputStream))
                    {
                        fileData = binaryReader.ReadBytes(model.ImageUpload.ContentLength);
                    }

                    request.AddFileBytes(model.ImageUpload.FileName,
                        fileData, model.ImageUpload.FileName, model.ImageUpload.ContentType);

                    request.AddParameter(new Parameter
                    {
                        Name = "Longitude",
                        Type = ParameterType.GetOrPost,
                        Value = "12.234"
                    });

                    request.AddParameter(new Parameter
                    {
                        Name = "Latitude",
                        Type = ParameterType.GetOrPost,
                        Value = "22.3221"
                    });


                    request.AddParameter(new Parameter
                    {
                        Name = "Description",
                        Type = ParameterType.GetOrPost,
                        Value = "Dummy description"
                    });

                    _logger.Info("Everything is valid, calling the rest client to save image");
                    try {
                        IRestResponse response = client.Execute(request);
                        var content = response.Content; // raw content as string

                        return RedirectToAction("Index");
                    }
                    catch(Exception ex)
                    {
                        _logger.Warn("Could not save image", ex);
                        ModelState.AddModelError("ImageUpload", "Could not save image, try again");
                        return View(model);
                    }
                }

                return View(model);
            }
        }

        // GET: InfringementPictures/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            infringementpicture infringementpicture = db.infringementpictures.Find(id);
            if (infringementpicture == null)
            {
                return HttpNotFound();
            }
            return View(infringementpicture);
        }

        // POST: InfringementPictures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            infringementpicture infringementpicture = db.infringementpictures.Find(id);
            db.infringementpictures.Remove(infringementpicture);
            db.SaveChanges();
            return RedirectToAction("Index");
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
