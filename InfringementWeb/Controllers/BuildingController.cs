using InfringementWeb.Helpers;
using InfringementWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace InfringementWeb.Controllers
{
    public class BuildingController : BaseController
    {
        private infringementEntities _entities = new infringementEntities();

        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Building
        public ActionResult Index()
        {
            using (log4net.NDC.Push("Buildings_Index_View"))
            {
                _logger.Info("Getting a list of all buildings in sort order");
                var building = _entities.parking_location
                    .OrderBy(x => x.SortOrder)
                    .ToList()
                    .Select(x => MvcModelToDatabaseModelMapper.MapBuildingForDisplay(x))
                    .ToList();
                ViewBag.Cities = _entities.cities.ToDictionary(x => x.id, x => x.name);
                return View(building);
            }
        }

        // GET: Building/Details/5
        public ActionResult Details(int id)
        {
            using (log4net.NDC.Push("Building_Detail_View"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, sending back bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.parking_location.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("Building could not be found, id = " + id);
                    return HttpNotFound();
                }
                _logger.Info("Building found create view model and send across to view");
                var model = MvcModelToDatabaseModelMapper.MapBuildingForDisplay(entity);
                var city = _entities.cities.FirstOrDefault(x => x.id == entity.CityId);
                if (city != null)
                {
                    _logger.Info("City found " + entity.CityId);
                    ViewBag.CityName = city.name;
                    return View(model);
                }
                else
                {
                    _logger.Warn("City not found - CityId=" + entity.CityId);
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
        }

        [HttpPost]
        public JsonResult GetBuildingsForCity(int cityId)
        {
            return Json(_entities.parking_location
                .Where(x => x.CityId == cityId)
                .OrderBy(x => x.SortOrder)
                .Select(x => new { Key = x.Id, Value = x.Name })
                .ToList());
        }

        // GET: Building/Create
        public ActionResult Create()
        {
            var model = new CarParkBuildingModel();
            model.SortOrder = (_entities.parking_location.Max(x => x.SortOrder) ?? 0) + 100;
            MvcModelToDatabaseModelMapper.MapBuildingForCreate(model);
            PopulateCitiesInViewBag(0);
            return View(model);
        }

        // POST: Building/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CityId,Name,Address,ImageLocation,Description,Longitude,Latitude,SortOrder")] CarParkBuildingModel model)
        {
            using (log4net.NDC.Push("Create building post"))
            {
                _logger.Info("Save specific building");
                _logger.Info(model);
                if (ModelState.IsValid)
                {
                    _logger.Info("Model is valid, map to entity model");
                    var entityModel = MvcModelToDatabaseModelMapper.MapBuildingForCreate(model);
                    _entities.parking_location.Add(entityModel);
                    try
                    {
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Saving of building entity failed", ex);
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
            using (log4net.NDC.Push("Building_Detail_View"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, sending back bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.parking_location.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("Building could not be found, id = " + id);
                    return HttpNotFound();
                }
                _logger.Info("Building found create view model and send across to view");
                var model = MvcModelToDatabaseModelMapper.MapBuildingForDisplay(entity);
                PopulateCitiesInViewBag(entity.CityId);



                if (ViewBag.Cities != null)
                {
                    _logger.Info("Cities Loaded " + entity.CityId);
                    return View(model);
                }
                else
                {
                    _logger.Warn("Cities not loaded");
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
        }

        private void PopulateCitiesInViewBag(int cityId)
        {
            ViewBag.Cities = _entities.cities
                    .OrderBy(x => x.SortOrder)
                    .Select(x =>
                    new SelectListItem
                    {
                        Value = x.id.ToString(),
                        Text = x.name,
                        Selected = cityId == x.id
                    }).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CityId,Name,Address,ImageLocation,Description,Longitude,Latitude,SortOrder")] CarParkBuildingModel model)
        {
            using (log4net.NDC.Push("Edit Building POST"))
            {
                _logger.Info("Check if model is valid");
                if (ModelState.IsValid)
                {
                    _logger.Info("Model is valid, save building");
                    var entityRecord = _entities.parking_location.FirstOrDefault(x => x.Id == model.Id);
                    MvcModelToDatabaseModelMapper.MapBuildingForEdit(model, entityRecord);
                    try
                    {
                        _logger.Info("Save building");
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("building could not be updated", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }

                    return RedirectToAction("Index");
                }
                _logger.Warn("Model is not valid, cannot save building");

                return View(model);
            }
        }

        // GET: Cities/Delete/5
        public ActionResult Delete(int id)
        {
            using (log4net.NDC.Push("Delete_Building"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.parking_location.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("Building could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("Building found, mapping to view model to editing purposes");
                var city = _entities.cities.FirstOrDefault(x => x.id == entity.CityId);
                if (city != null)
                {
                    _logger.Info("City found " + entity.CityId);
                    ViewBag.CityName = city.name;
                    return View(MvcModelToDatabaseModelMapper.MapBuildingForDisplay(entity));
                }
                else
                {
                    _logger.Warn("City not found - CityId=" + entity.CityId);
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
                    var entity = _entities.parking_location.Find(id);
                    _entities.parking_location.Remove(entity);
                    _entities.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Warn("Error deleting city", ex);
                    //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    ViewBag.ErrorMessage = "Referenced record can not be deleted.";
                    ModelState.AddModelError("", "Referenced record can not be deleted.");
                }
            }
            return View();
        }
    }
}
