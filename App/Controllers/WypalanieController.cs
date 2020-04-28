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
    public class WypalanieController : Controller
    {
        private EntityDBEntities db = new EntityDBEntities();

        // GET: Wypalanie
        public ActionResult Index(string sortowanie, string search, string filter, int? page)
        {
            int pageNumber = (page ?? 1);

            ViewBag.Wyszukiwanie = search;
            ViewBag.Strona = page;
            ViewBag.SortBy = sortowanie;
            ViewBag.Produkt = sortowanie == "ProduktDSC" ? "ProduktASC" : "ProduktDSC";
            ViewBag.Termin = sortowanie == null ? "TerminDSC" : "";
            ViewBag.IloscZamowiona = sortowanie == "IloscZamowionaDSC" ? "IloscZamowionaASC" : "IloscZamowionaDSC";

            if (search != null)
            {
                page = 1;
            }
            else
            {
                search = filter;
            }
            ViewBag.filter = search;
            
            var query = from p in db.Produkty
                        join w in db.Wypalanie on p.ProduktID equals w.ProduktID
                        where w.Wypalono == "nie"
                        select w;

            if (!String.IsNullOrEmpty(search))
            {
                query = from w in db.Wypalanie
                        join p in db.Produkty on w.ProduktID equals p.ProduktID
                        join z in db.Zlecenia on p.ZlecenieID equals z.ZlecenieID
                        where p.Produkt.Contains(search) && w.Wypalono == "nie" ||
                        p.Uwagi.Contains(search) && w.Wypalono == "nie" ||
                        p.IloscZamowiona.ToString().Contains(search) && w.Wypalono == "nie" ||
                        z.Termin.ToString().Contains(search) && w.Wypalono == "nie"
                        select w;
            }

            switch (sortowanie)
            {
                case "ProduktDSC":
                    query = query.OrderByDescending(s => s.Produkty.Produkt);
                    break;
                case "ProduktASC":
                    query = query.OrderBy(s => s.Produkty.Produkt);
                    break;
                case "TerminDSC":
                    query = query.OrderByDescending(s => s.Produkty.Zlecenia.Termin);
                    break;
                case "IloscZamowionaDSC":
                    query = query.OrderByDescending(s => s.Produkty.IloscZamowiona);
                    break;
                case "IloscZamowionaASC":
                    query = query.OrderBy(s => s.Produkty.IloscZamowiona);
                    break;
                default:
                    query = query.OrderBy(s => s.Produkty.Zlecenia.Termin);
                    break;

            }



            return View(query.ToPagedList(pageNumber, 6));
        }

        
        // GET: Wypalanie/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Wypalanie wypalanie = db.Wypalanie.Find(id);
            if (wypalanie == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProduktID = new SelectList(db.Produkty, "ProduktID", "Produkt", wypalanie.ProduktID);
            
            return View(wypalanie);
        }

        // POST: Wypalanie/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProduktID,IloscWykonana,CzasWypalania,Wypalono")] Wypalanie wypalanie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(wypalanie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProduktID = new SelectList(db.Produkty, "ProduktID", "Produkt", wypalanie.ProduktID);
            return View(wypalanie);
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
