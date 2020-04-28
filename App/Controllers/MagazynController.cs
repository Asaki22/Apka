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
    public class MagazynController : Controller
    {
        
        private EntityDBEntities db = new EntityDBEntities();

        // GET: Magazyn
        public ActionResult Index(string sortowanie, string search, string filter, int? page)
        {
            int pageNumber = (page ?? 1);

            ViewBag.Wyszukiwanie = search;
            ViewBag.Strona = page;
            ViewBag.SortBy = sortowanie;
            ViewBag.Produkt = sortowanie == "ProduktDSC" ? "ProduktASC" : "ProduktDSC";
            ViewBag.Termin = sortowanie == "TerminDSC" ? "TerminASC" : "TerminDSC";
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
                        join m in db.Magazyn on p.ProduktID equals m.ProduktID
                        join s in db.Spawanie on p.ProduktID equals s.ProduktID
                        where s.Pospawano == "tak" && m.Wyslano == "nie"
                        select m;

            if (!String.IsNullOrEmpty(search))
            {
                query = from m in db.Magazyn
                        join p in db.Produkty on m.ProduktID equals p.ProduktID
                        join z in db.Zlecenia on p.ZlecenieID equals z.ZlecenieID
                        join s in db.Spawanie on p.ProduktID equals s.ProduktID
                        where p.Produkt.Contains(search) && s.Pospawano == "tak" ||
                        p.Uwagi.Contains(search) && s.Pospawano == "tak" ||
                        p.IloscZamowiona.ToString().Contains(search) && s.Pospawano == "tak" ||
                        z.Termin.ToString().Contains(search) && s.Pospawano == "tak"
                        select m;
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

        
        // GET: Magazyn/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Magazyn magazyn = db.Magazyn.Find(id);
            if (magazyn == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProduktID = new SelectList(db.Produkty, "ProduktID", "Produkt", magazyn.ProduktID);
            return View(magazyn);
        }

        // POST: Magazyn/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProduktID,Spakowano,Wyslano,Miejsce")] Magazyn magazyn)
        {
            if (ModelState.IsValid)
            {
                db.Entry(magazyn).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProduktID = new SelectList(db.Produkty, "ProduktID", "Produkt", magazyn.ProduktID);
            return View(magazyn);
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
