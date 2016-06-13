using InfringementWeb.Helpers;
using InfringementWeb.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace InfringementWeb.Controllers
{
    public class CitiesController : BaseController
    {

        private infringementEntities _entities = new infringementEntities();

        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        
        // GET: Cities
        public ActionResult Index()
        {
            using (log4net.NDC.Push("Cities_Index_View"))
            {
                _logger.Info("Getting a list of all cities in sort order");
                var cities = _entities.cities
                    .OrderBy(x => x.SortOrder)
                    .ToList()
                    .Select(x => MvcModelToDatabaseModelMapper.MapCityForDisplay(x))
                    .ToList();
                return View(cities);
            }
        }

        // GET: Cities/Details/5
        public ActionResult Details(int id)
        {
            using (log4net.NDC.Push("Cityz_Detail_View"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id caanot be less than 1, sending back bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.cities.FirstOrDefault(x => x.id == id);

                if (entity == null)
                {
                    _logger.Warn("City could not be found, id = " + id);
                    return HttpNotFound();
                }
                _logger.Info("City found create view model and send across to view");
                var model = MvcModelToDatabaseModelMapper.MapCityForDisplay(entity);
                return View(model);
            }
        }

        // GET: Cities/Create
        public ActionResult Create()
        {
            var model = new CityModel {
                SortOrder = (_entities.cities.Max(x => x.SortOrder) ?? 0) + 100
            };

            return View(model);
        }

        // POST: Cities/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,SortOrder")] CityModel model)
        {
            using (log4net.NDC.Push("Create city post"))
            {
                _logger.Info("Save specific city");
                _logger.Info(model);
                if (ModelState.IsValid)
                {
                    _logger.Info("Model is valid, map to entity model");
                    var entityModel = MvcModelToDatabaseModelMapper.MapCityForCreate(model);
                    _entities.cities.Add(entityModel);
                    try
                    {
                        _entities.SaveChanges();
                    }
                    catch (Exception ex) {
                        _logger.Warn("Saving of city entity failed", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                    return RedirectToAction("Index");
                }

                return View(model);
            }
        }

        // GET: Cities/Edit/5
        public ActionResult Edit(int id)
        {
            using (log4net.NDC.Push("Edit_City"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.cities.FirstOrDefault(x => x.id == id);

                if (entity == null)
                {
                    _logger.Warn("City could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("City found, mapping to view model to editing purposes");
                return View(MvcModelToDatabaseModelMapper.MapCityForDisplay(entity));
            }
        }

        // POST: Cities/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, CityModel cityModel)
        {
            using (log4net.NDC.Push("Post for editing city"))
            {
                if (ModelState.IsValid)
                {
                    
                    _logger.Info("Model is valid, search for city in the database" + id);
                    _logger.Info(cityModel);
                    var cityEntity = _entities.cities.FirstOrDefault(x => x.id == id);
                    if(cityEntity == null)
                    {
                        _logger.Warn("City not found");
                        return new HttpNotFoundResult();
                    }

                    _logger.Info("City found, updating the database entity");
                    MvcModelToDatabaseModelMapper.MapCityForEdit(cityModel, cityEntity);
                    try
                    {
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("City could not be updated", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);

                    }
                    return RedirectToAction("Index");
                }
                return View(cityModel);
            }
        }

        // GET: Cities/Delete/5
        public ActionResult Delete(int id)
        {
            using (log4net.NDC.Push("Delete_City"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.cities.FirstOrDefault(x => x.id == id);

                if (entity == null)
                {
                    _logger.Warn("City could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("City found, mapping to view model to editing purposes");
                return View(MvcModelToDatabaseModelMapper.MapCityForDisplay(entity));
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
                    var entity = _entities.cities.Find(id);
                    _entities.cities.Remove(entity);
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
