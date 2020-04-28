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
    public class ZaginanieController : Controller
    {
        private EntityDBEntities db = new EntityDBEntities();

        // GET: Zaginanie
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
                        join z in db.Zaginanie on p.ProduktID equals z.ProduktID
                        join w in db.Wypalanie on p.ProduktID equals w.ProduktID
                        where w.Wypalono == "tak" && z.Pogieto == "nie"
                        select z;
            if (!String.IsNullOrEmpty(search))
            {
                query = from za in db.Zaginanie
                        join p in db.Produkty on za.ProduktID equals p.ProduktID
                        join z in db.Zlecenia on p.ZlecenieID equals z.ZlecenieID
                        join w in db.Wypalanie on p.ProduktID equals w.ProduktID
                        where p.Produkt.Contains(search) && za.Pogieto == "nie" && w.Wypalono == "tak" ||
                        p.Uwagi.Contains(search) && za.Pogieto == "nie" && w.Wypalono == "tak" ||
                        p.IloscZamowiona.ToString().Contains(search) && za.Pogieto == "nie" && w.Wypalono == "tak" ||
                        z.Termin.ToString().Contains(search) && za.Pogieto == "nie" && w.Wypalono == "tak"
                        select za;
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
                default:
                    query = query.OrderBy(s => s.Produkty.Zlecenia.Termin);
                    break;
                case "IloscZamowionaDSC":
                    query = query.OrderByDescending(s => s.Produkty.IloscZamowiona);
                    break;
                case "IloscZamowionaASC":
                    query = query.OrderBy(s => s.Produkty.IloscZamowiona);
                    break;

            }



            return View(query.ToPagedList(pageNumber, 6));
        }

        

        // GET: Zaginanie/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Zaginanie zaginanie = db.Zaginanie.Find(id);
            if (zaginanie == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProduktID = new SelectList(db.Produkty, "ProduktID", "Produkt", zaginanie.ProduktID);
            return View(zaginanie);
        }

        // POST: Zaginanie/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProduktID,IloscWykonana,CzasZaginania,Pogieto")] Zaginanie zaginanie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(zaginanie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProduktID = new SelectList(db.Produkty, "ProduktID", "Produkt", zaginanie.ProduktID);
            return View(zaginanie);
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

