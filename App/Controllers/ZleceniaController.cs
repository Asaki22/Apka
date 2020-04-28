using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using App.Models;
using PagedList;

namespace App.Controllers
{
    [Authorize]
    public class ZleceniaController : Controller
    {
        private EntityDBEntities db = new EntityDBEntities();
        
        // GET: Zlecenia
        
        public ActionResult Index(string sortowanie, string search, string filter, int? page)
        {
            
           // var query = from z in db.Zlecenia
            //            select z;
            int pageNumber = (page ?? 1);

            ViewBag.Wyszukiwanie = search;
            ViewBag.Strona = page;
            ViewBag.SortBy = sortowanie;
            ViewBag.Firma = sortowanie == "FirmaDSC" ? "FirmaASC" : "FirmaDSC";
            ViewBag.Termin = sortowanie == null ? "TerminDSC" : "";

            if(search != null)
            {
                page = 1;
            }
            else
            {
                search = filter;
            }
            ViewBag.filter = search;

            //var query = db.Zlecenia.Include(s => s.Produkty);
            var query = from z in db.Zlecenia
                        select z;

            if (!String.IsNullOrEmpty(search))
            {

                //query.Where(s => s.Firma.Contains(search) || s.Termin.ToString().Contains(search));
                query = from z in db.Zlecenia
                        where z.Firma.Contains(search) || z.Termin.ToString().Contains(search)
                        select z;

                //return View(query.ToPagedList(pageNumber, 6));
            }

            switch (sortowanie)
            {
                case "FirmaDSC":
                    query = query.OrderByDescending(s => s.Firma);
                        break;
                case "FirmaASC":
                    query = query.OrderBy(s => s.Firma);
                    break;
                case "TerminDSC":
                    query = query.OrderByDescending(s => s.Termin);
                    break;
                default:
                    query = query.OrderBy(s => s.Termin);
                    break;
            }
            
            

            return View(query.ToPagedList(pageNumber, 6));
        }






        // GET: Zlecenia/Create
        [Authorize(Roles="Admin")]
        public ActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ZlecenieID,Firma,Termin")] Zlecenia zlecenia)
        {
            if (ModelState.IsValid)
            {
                db.Zlecenia.Add(zlecenia);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(zlecenia);
        }

        // GET: Zlecenia/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Zlecenia zlecenia = db.Zlecenia.Find(id);
            if (zlecenia == null)
            {
                return HttpNotFound();
            }
            return View(zlecenia);
        }

        // POST: Zlecenia/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ZlecenieID,Firma,Termin")] Zlecenia zlecenia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(zlecenia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(zlecenia);
        }

        // GET: Zlecenia/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Zlecenia zlecenia = db.Zlecenia.Find(id);
            if (zlecenia == null)
            {
                return HttpNotFound();
            }
            return View(zlecenia);
        }

        // POST: Zlecenia/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Zlecenia zlecenia = db.Zlecenia.Find(id);
            db.Zlecenia.Remove(zlecenia);
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
