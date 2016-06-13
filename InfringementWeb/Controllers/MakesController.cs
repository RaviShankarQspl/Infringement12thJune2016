using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using InfringementWeb.Helpers;
using InfringementWeb.Models;

namespace InfringementWeb.Controllers
{
    public class MakesController : BaseController
    {

        private infringementEntities _entities = new infringementEntities();

        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // GET:  Makes
        public ActionResult Index()
        {
            using (log4net.NDC.Push("Car_Makes_Index_View"))
            {
                _logger.Info("Getting a list of all makes in sort order");
                var items = _entities.makes
                    .OrderBy(x => x.SortOrder)
                    .ToList()
                    .Select(x => MvcModelToDatabaseModelMapper.MapCarMakeForDisplay(x))
                    .ToList();
                return View(items);
            }
        }

        // GET:  Makes/Details/5
        public ActionResult Details(int id)
        {
            using (log4net.NDC.Push("Car_Make_Detail_View"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id caanot be less than 1, sending back bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.makes.FirstOrDefault(x => x.id == id);

                if (entity == null)
                {
                    _logger.Warn("Make could not be found, id = " + id);
                    return HttpNotFound();
                }
                _logger.Info("Make found create view model and send across to view");
                var model = MvcModelToDatabaseModelMapper.MapCarMakeForDisplay(entity);
                return View(model);
            }
        }

        // GET:  Makes/Create
        public ActionResult Create()
        {
            var model = new CarMakeModel
            {
                SortOrder = (_entities.makes.Max(x => x.SortOrder) ?? 0) + 100
            };

            return View(model);
        }

        // POST:  Makes/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,SortOrder")] CarMakeModel model)
        {
            using (log4net.NDC.Push("Create car make post"))
            {
                _logger.Info("Save specific make");
                _logger.Info(model);
                if (ModelState.IsValid)
                {
                    _logger.Info("Model is valid, map to entity model");
                    var entityModel = MvcModelToDatabaseModelMapper.MapCarMakeForCreate(model);
                    _entities.makes.Add(entityModel);
                    try
                    {
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Saving of city entity failed", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                    return RedirectToAction("Index");
                }

                return View(model);
            }
        }

        // GET:  Makes/Edit/5
        public ActionResult Edit(int id)
        {
            using (log4net.NDC.Push("Edit_Car_Make"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.makes.FirstOrDefault(x => x.id == id);

                if (entity == null)
                {
                    _logger.Warn("Car Make could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("Make found, mapping to view model to editing purposes");
                return View(MvcModelToDatabaseModelMapper.MapCarMakeForDisplay(entity));
            }
        }

        // POST:  Makes/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, CarMakeModel model)
        {
            using (log4net.NDC.Push("Post for editing car make"))
            {
                if (ModelState.IsValid)
                {

                    _logger.Info("Model is valid, search for make in the database" + id);
                    _logger.Info(model);
                    var entity = _entities.makes.FirstOrDefault(x => x.id == id);
                    if (entity == null)
                    {
                        _logger.Warn("Make not found");
                        return new HttpNotFoundResult();
                    }

                    _logger.Info("Make found, updating the database entity");
                    MvcModelToDatabaseModelMapper.MapCarMakeForEdit(model, entity);
                    try
                    {
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Make could not be updated", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);

                    }
                    return RedirectToAction("Index");
                }
                return View(model);
            }
        }

        // GET:  Makes/Delete/5
        public ActionResult Delete(int id)
        {
            using (log4net.NDC.Push("Delete_Make"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.makes.FirstOrDefault(x => x.id == id);

                if (entity == null)
                {
                    _logger.Warn("Make could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("Make found, mapping to view model to editing purposes");
                return View(MvcModelToDatabaseModelMapper.MapCarMakeForDisplay(entity));
            }
        }

        // POST:  Makes/Delete/5
        [HttpPost]
        public ActionResult Delete(int? id, FormCollection collection)
        {
            using (log4net.NDC.Push("Post_For_Delete"))
            {
                try
                {
                    var entity = _entities.makes.Find(id);
                    _entities.makes.Remove(entity);
                    _entities.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Warn("Error deleting make", ex);
                    //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    ViewBag.ErrorMessage = "Referenced record can not be deleted.";
                    ModelState.AddModelError("", "Referenced record can not be deleted.");
                }
            }
            return View();
        }

        // GET:  Makes/GetModelsbyMake/5
        [HttpPost]
        public JsonResult GetModelsbyMake(int makeId)
        {
            return Json(_entities.carmodels
                .Where(x => x.MakeId == makeId)
                .OrderBy(x => x.SortOrder)
                .Select(x => new { Key = x.Id, Value = x.Name })
                .ToList());
        }
    }
}
