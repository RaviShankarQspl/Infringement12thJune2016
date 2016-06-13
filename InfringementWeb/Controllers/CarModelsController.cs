using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InfringementWeb;
using InfringementWeb.Helpers;
using InfringementWeb.Models;

namespace InfringementWeb.Controllers
{
    public class CarModelsController : BaseController
    {
        private infringementEntities _entities = new infringementEntities();

        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Building
        public ActionResult Index()
        {
            using (log4net.NDC.Push("Models_Index_View"))
            {
                _logger.Info("Getting a list of all models in sort order");
                var building = _entities.carmodels
                    .OrderBy(x => x.SortOrder)
                    .ToList()
                    .Select(x => MvcModelToDatabaseModelMapper.MapCarModelForDisplay(x))
                    .ToList();
                ViewBag.Makes = _entities.makes.ToDictionary(x => x.id, x => x.Name);
                return View(building);
            }
        }

        // GET: Building/Details/5
        public ActionResult Details(int id)
        {
            using (log4net.NDC.Push("Model_Detail_View"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, sending back bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.carmodels.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("Models could not be found, id = " + id);
                    return HttpNotFound();
                }
                _logger.Info("Model found create view model and send across to view");
                var model = MvcModelToDatabaseModelMapper.MapCarModelForDisplay(entity);
                var make = _entities.makes.FirstOrDefault(x => x.id == entity.MakeId);
                if (make != null)
                {
                    _logger.Info("Make found " + entity.MakeId);
                    ViewBag.MakeName = make.Name;
                    return View(model);
                }
                else
                {
                    _logger.Warn("Make not found - MakeId=" + entity.MakeId);
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
        }

        // GET: Building/Create
        public ActionResult Create()
        {
            var model = new ModelForCarMakeModel();
            model.SortOrder = (_entities.carmodels.Max(x => x.SortOrder) ?? 0) + 100;
            MvcModelToDatabaseModelMapper.MapCarModelForCreate(model);
            PopulateCarModelsInViewBag(0);
            return View(model);
        }

        // POST: Building/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,MakeId,Name,SortOrder")] ModelForCarMakeModel model)
        {
            using (log4net.NDC.Push("Create car model post"))
            {
                _logger.Info("Save car model");
                _logger.Info(model);
                if (ModelState.IsValid)
                {
                    _logger.Info("Model is valid, map to entity model");
                    var entityModel = MvcModelToDatabaseModelMapper.MapCarModelForCreate(model);
                    _entities.carmodels.Add(entityModel);
                    try
                    {
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Saving of car model entity failed", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                    return RedirectToAction("Index");
                }

                return View(model);
            }
        }

        // GET: Building/Edit/5
        public ActionResult Edit(int id)
        {
            using (log4net.NDC.Push("Model_Detail_View"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, sending back bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.carmodels.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("Car Model could not be found, id = " + id);
                    return HttpNotFound();
                }
                _logger.Info("Car model found create view model and send across to view");
                var model = MvcModelToDatabaseModelMapper.MapCarModelForDisplay(entity);
                PopulateCarModelsInViewBag(entity.MakeId);



                if (ViewBag.Makes != null)
                {
                    _logger.Info("Makes Loaded " + entity.MakeId);
                    return View(model);
                }
                else
                {
                    _logger.Warn("Cities not loaded");
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
        }

        private void PopulateCarModelsInViewBag(int makeId)
        {
            ViewBag.Makes = _entities.makes
                    .OrderBy(x => x.SortOrder)
                    .Select(x =>
                    new SelectListItem
                    {
                        Value = x.id.ToString(),
                        Text = x.Name,
                        Selected = makeId == x.id
                    }).ToList();
        }

        [HttpPost]
        public JsonResult GetModels(int makeId, string modelName)
        {
            return Json(_entities.carmodels
                .Where(x => x.MakeId == makeId && x.Name.Trim().ToUpper().Contains(modelName.Trim().ToUpper()))
                .OrderBy(x => x.SortOrder)
                .Select(x => x.Name)
                .ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,MakeId,Name,SortOrder")] ModelForCarMakeModel model)
        {
            using (log4net.NDC.Push("Edit Building POST"))
            {
                _logger.Info("Check if model is valid");
                if (ModelState.IsValid)
                {
                    _logger.Info("Model is valid, save model");
                    var entityRecord = _entities.carmodels.FirstOrDefault(x => x.Id == model.Id);
                    MvcModelToDatabaseModelMapper.MapCarModelForEdit(model, entityRecord);
                    try
                    {
                        _logger.Info("Save model");
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("model could not be updated", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }

                    return RedirectToAction("Index");
                }
                _logger.Warn("Model is not valid, cannot save car model");

                return View(model);
            }
        }

        // GET: Cities/Delete/5
        public ActionResult Delete(int id)
        {
            using (log4net.NDC.Push("Delete_Car_Model"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.carmodels.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("Car model could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("car model found, mapping to view model to editing purposes");
                var make = _entities.makes.FirstOrDefault(x => x.id == entity.MakeId);
                if (make != null)
                {
                    _logger.Info("City found " + entity.MakeId);
                    ViewBag.CityName = make.Name;
                    return View(MvcModelToDatabaseModelMapper.MapCarModelForDisplay(entity));
                }
                else
                {
                    _logger.Warn("Make not found - MakeId=" + entity.MakeId);
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
        }

        // POST: Cities/Delete/5
        [HttpPost]
        public ActionResult Delete(int? id, FormCollection collection)
        {
            using (log4net.NDC.Push("Post_For_Delete"))
            {
                try
                {
                    infringement infringement = (from inf in _entities.infringements
                                                 where inf.ModelId == id.Value
                                                 select inf).FirstOrDefault();

                    if (infringement == null)
                    { 
                        var entity = _entities.carmodels.Find(id);
                        _entities.carmodels.Remove(entity);
                        _entities.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        _logger.Warn("Error deleting car model");
                        //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                        ViewBag.ErrorMessage = "Referenced record can not be deleted.";
                        ModelState.AddModelError("", "Referenced record can not be deleted.");
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn("Error deleting car model", ex);
                    //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    ViewBag.ErrorMessage = "Referenced record can not be deleted.";
                    ModelState.AddModelError("", "Referenced record can not be deleted.");

                }
            }
            return View();
        }
    }
}
