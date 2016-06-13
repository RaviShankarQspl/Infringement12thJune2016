using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InfringementWeb;

namespace InfringementWeb.Controllers
{
    public class UsersController : BaseController
    {
        private infringementEntities db = new infringementEntities();

        // GET: Users
        public async Task<ActionResult> Index()
        {
            return View(await db.users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = await db.users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.Roles = new SelectList(db.roles, "id", "RoleName");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,FirstName,LastName,DateOfBirth,Address1,Address2,Suburb,City,HomePhone,MobilePhone,Email,JobTitle,PhotoLocation,UserType,IsActive,UserPassword")] user user)
        {
            var usert = db.users.Where(x => x.Email == user.Email).FirstOrDefault();

            if (usert == null)
            {
                if (ModelState.IsValid)
                {
                    user.IsActive = true;
                    if (user.UserType == null)
                        user.UserType = 2;  //2- normal user  1- admin user

                    db.users.Add(user);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View("Create", user);

            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(int? id)
        //public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Roles = new SelectList(db.roles, "id", "RoleName");
            user user = await db.users.FindAsync(id);
            //user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            else
                Session["EditUserObject"] = user;

            TempData["DOB"] = user.DateOfBirth;

            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FirstName,LastName,DateOfBirth,Address1,Address2,Suburb,City,HomePhone,MobilePhone,Email,JobTitle,PhotoLocation,UserType,IsActive,UserPassword")] user user)
        {
            ViewBag.Roles = new SelectList(db.roles, "id", "RoleName");

            if (ModelState.IsValid)
            {
                if (user.UserType == null)
                    user.UserType = 2;  //2- normal user  1- admin user

                Session["EditUserObject"] = user;
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = await db.users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            user user = await db.users.FindAsync(id);
            db.users.Remove(user);
            await db.SaveChangesAsync();
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

        [HttpPost]
        public JsonResult doesUserNameExist(string Email)
        {

            user lUser = null;
            using (infringementEntities entity = new infringementEntities())
            {
                lUser = (from a in entity.users
                         where a.Email == Email
                         select a).FirstOrDefault();
            }

            if (Session["EditUserObject"] != null)
            {
                user eUser = (user)Session["EditUserObject"];

                if (lUser != null && lUser.Id == eUser.Id)
                    lUser = null;
            }

            return Json(lUser == null);
        }

        [HttpPost]
        public JsonResult FutureDatecheck(DateTime DateOfBirth)
        {
            return Json(null);
        }
    }
}
