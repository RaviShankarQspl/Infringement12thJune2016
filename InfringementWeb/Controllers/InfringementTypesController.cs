using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InfringementWeb;
using InfringementWeb.Models;
using InfringementWeb.Helpers;

namespace InfringementWeb.Controllers
{
    public class InfringementTypesController : BaseController
    {
        private infringementEntities _entities = new infringementEntities();
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(
    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Cities
        public ActionResult Index()
        {
            using (log4net.NDC.Push("InfringementType_Index_View"))
            {
                _logger.Info("Getting a list of all infringement types in sort order");
                var items = _entities.infringementtypes
                    .OrderBy(x => x.SortOrder)
                    .ToList()
                    .Select(x => MvcModelToDatabaseModelMapper.MapInfringementTypeForDisplay(x))
                    .ToList();
                return View(items);
            }
        }

        [HttpPost]
        public JsonResult GetInfringementAmount(int infringementTypeId)
        {
            return Json(
                _entities.infringementtypes
                .Where(x => x.Id == infringementTypeId)
                .ToList()
                .Select(x => x.Amount.ToString("###,###.00"))
                .FirstOrDefault()
                );
        }

        // GET: InfringementTypes/Details/5
        public ActionResult Details(int id)
        {
            using (log4net.NDC.Push("Infrin.Type_Detail_View"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id caanot be less than 1, sending back bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.infringementtypes.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("Infringement Type could not be found, id = " + id);
                    return HttpNotFound();
                }
                _logger.Info("Infringement Type found create view model and send across to view");
                var model = MvcModelToDatabaseModelMapper.MapInfringementTypeForDisplay(entity);
                return View(model);
            }
        }

        // GET: InfringementTypes/Create
        public ActionResult Create()
        {
            var model = new InfringementTypeModel
            {
                SortOrder = (_entities.infringementtypes.Max(x => x.SortOrder) ?? 0) + 100
            };

            return View(model);
        }

        // POST: InfringementTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Type,Amount,SortOrder")] InfringementTypeModel model)
        {
            using (log4net.NDC.Push("Create infrn. type post"))
            {
                _logger.Info("Save specific infringement type");
                _logger.Info(model);
                if (ModelState.IsValid)
                {
                    _logger.Info("Model is valid, map to entity model");
                    var entityModel = MvcModelToDatabaseModelMapper.MapInfringementTypeForCreate(model);
                    _entities.infringementtypes.Add(entityModel);
                    try
                    {
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Saving of infrin. type entity failed", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                    return RedirectToAction("Index");
                }

                return View(model);
            }
        }

        // GET: InfringementTypes/Edit/5
        public ActionResult Edit(int id)
        {
            using (log4net.NDC.Push("Edit_Infrin_Type"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.infringementtypes.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("Infrin. Type could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("Infrin. type found, mapping to view model to editing purposes");
                return View(MvcModelToDatabaseModelMapper.MapInfringementTypeForDisplay(entity));
            }
        }

        // POST: InfringementTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Type,Amount,SortOrder")] InfringementTypeModel model)
        {
            using (log4net.NDC.Push("Post for editing infrin. type"))
            {
                if (ModelState.IsValid)
                {

                    _logger.Info("Model is valid, search for city in the database" + model.Id);
                    _logger.Info(model);
                    var entity = _entities.infringementtypes.FirstOrDefault(x => x.Id == model.Id);
                    if (entity == null)
                    {
                        _logger.Warn("Infrin. type not found");
                        return new HttpNotFoundResult();
                    }

                    _logger.Info("Infrin. type found, updating the database entity");
                    MvcModelToDatabaseModelMapper.MapInfringementTypeForEdit(model, entity);
                    try
                    {
                        _entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Infrin. type could not be updated", ex);
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);

                    }
                    return RedirectToAction("Index");
                }
                return View(model);
            }
        }

        // GET: Cities/Delete/5
        public ActionResult Delete(int id)
        {
            using (log4net.NDC.Push("Delete_Infrin_type"))
            {
                if (id <= 0)
                {
                    _logger.Warn("Id cannot be less than 1, bad request");
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var entity = _entities.infringementtypes.FirstOrDefault(x => x.Id == id);

                if (entity == null)
                {
                    _logger.Warn("infrin. type could not be found, not found ");
                    return HttpNotFound();
                }

                _logger.Info("Infrin. type found, mapping to view model to editing purposes");
                return View(MvcModelToDatabaseModelMapper.MapInfringementTypeForDisplay(entity));
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
                    var entity = _entities.infringementtypes.Find(id);
                    _entities.infringementtypes.Remove(entity);
                    _entities.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Warn("Error deleting infrin. type", ex);
                    //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    ViewBag.ErrorMessage = "Referenced record can not be deleted.";
                    ModelState.AddModelError("", "Referenced record can not be deleted.");
                }
            }
            return View();
        }
    }
}
