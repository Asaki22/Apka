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
    public class ProduktyController : Controller
    {
        private EntityDBEntities db = new EntityDBEntities();

        // GET: Produkty
        public ActionResult Index(string sortowanie, string search, string filter, int? page)
        {

            ViewBag.Wyszukiwanie = search;
            ViewBag.Strona = page;
            ViewBag.SortBy = sortowanie;
            ViewBag.Produkt = sortowanie == null ? "ProduktDSC" : "";
            ViewBag.IloscZamowiona = sortowanie == "IloscZamowionaDSC" ? "IloscZamowionaASC" : "IloscZamowionaDSC";
            ViewBag.Firma = sortowanie == "FirmaDSC" ? "FirmaASC" : "FirmaDSC";
            ViewBag.Uwagi = sortowanie == "UwagiDSC" ? "UwagiASC" : "UwagiDSC";

            if (search != null)
            {
                page = 1;
            }
            else
            {
                search = filter;
            }
            ViewBag.filter = search;

            int pageNumber = (page ?? 1);
            var query = from p in db.Produkty
                        select p;

            if (!String.IsNullOrEmpty(search))
            {
                query = from p in db.Produkty
                        join z in db.Zlecenia on p.ZlecenieID equals z.ZlecenieID
                        where p.Produkt.Contains(search) || p.Uwagi.Contains(search) ||
                        p.IloscZamowiona.ToString().Contains(search) || z.Firma.Contains(search)
                        select p;
            }

            switch (sortowanie)
            {
                case "ProduktDSC":
                    query = query.OrderByDescending(s => s.Produkt);
                    break;
                case "IloscZamowionaDSC":
                    query = query.OrderByDescending(s => s.IloscZamowiona);
                    break;
                case "IloscZamowionaASC":
                    query = query.OrderBy(s => s.IloscZamowiona);
                    break;
                case "FirmaDSC":
                    query = query.OrderByDescending(s => s.Zlecenia.Firma);
                    break;
                case "FirmaASC":
                    query = query.OrderBy(s => s.Zlecenia.Firma);
                    break;
                case "UwagiDSC":
                    query = query.OrderByDescending(s => s.Uwagi);
                    break;
                case "UwagiASC":
                    query = query.OrderBy(s => s.Uwagi);
                    break;
                default:
                    query = query.OrderBy(s => s.Produkt);
                    break;
            }



            return View(query.ToPagedList(pageNumber, 6));
        }


        // GET: Produkty/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.ZlecenieID = new SelectList(db.Zlecenia, "ZlecenieID", "Firma");
            return View();
        }

        // POST: Produkty/Create
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProduktID,ZlecenieID,Produkt,IloscZamowiona,Uwagi")] Produkty produkty)
        {
            if (ModelState.IsValid)
            {
                Wypalanie wypalanie = new Wypalanie();
                Zaginanie zaginanie = new Zaginanie();
                Spawanie spawanie = new Spawanie();
                Magazyn magazyn = new Magazyn();

                wypalanie.Wypalono = "nie";
                zaginanie.Pogieto = "nie";
                spawanie.Pospawano = "nie";
                magazyn.Spakowano = "nie";
                magazyn.Wyslano = "nie";

                produkty.ProduktID = wypalanie.ProduktID;
                produkty.ProduktID = zaginanie.ProduktID;
                produkty.ProduktID = spawanie.ProduktID;
                produkty.ProduktID = magazyn.ProduktID;

                db.Magazyn.Add(magazyn);
                db.Spawanie.Add(spawanie);
                db.Zaginanie.Add(zaginanie);
                db.Wypalanie.Add(wypalanie);
                db.Produkty.Add(produkty);

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ZlecenieID = new SelectList(db.Zlecenia, "ZlecenieID", "Firma", produkty.ZlecenieID);
            return View(produkty);
        }

        // GET: Produkty/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produkty produkty = db.Produkty.Find(id);
            if (produkty == null)
            {
                return HttpNotFound();
            }
            ViewBag.ZlecenieID = new SelectList(db.Zlecenia, "ZlecenieID", "Firma", produkty.ZlecenieID);
            return View(produkty);
        }

        // POST: Produkty/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProduktID,ZlecenieID,Produkt,IloscZamowiona,Uwagi")] Produkty produkty)
        {
            if (ModelState.IsValid)
            {
                db.Entry(produkty).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ZlecenieID = new SelectList(db.Zlecenia, "ZlecenieID", "Firma", produkty.ZlecenieID);
            
            return View(produkty);
        }

        // GET: Produkty/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Produkty produkty = db.Produkty.Find(id);
            if (produkty == null)
            {
                return HttpNotFound();
            }
            return View(produkty);
        }

        // POST: Produkty/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Magazyn magazyn = db.Magazyn.Find(id);
            Spawanie spawanie = db.Spawanie.Find(id);
            Zaginanie zaginanie = db.Zaginanie.Find(id);
            Wypalanie wypalanie = db.Wypalanie.Find(id);
            Produkty produkty = db.Produkty.Find(id);

            db.Magazyn.Remove(magazyn);
            db.Spawanie.Remove(spawanie);
            db.Zaginanie.Remove(zaginanie);
            db.Wypalanie.Remove(wypalanie);
            db.Produkty.Remove(produkty);
            
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
        public ActionResult Postep(string sortowanie,string search, string filter, int? page)
        {

            ViewBag.Strona = page;
            ViewBag.SortBy = sortowanie;
            ViewBag.Wyszukiwanie = search;

            

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
                        select p;

            if (!String.IsNullOrEmpty(search))
            {
                query = from z in db.Zlecenia
                        join p in db.Produkty on z.ZlecenieID equals p.ZlecenieID
                        join w in db.Wypalanie on p.ProduktID equals w.ProduktID
                        join za in db.Zaginanie on p.ProduktID equals za.ProduktID
                        join s in db.Spawanie on p.ProduktID equals s.ProduktID
                        join m in db.Magazyn on p.ProduktID equals m.ProduktID
                        where p.Produkt.Contains(search) || z.Firma.Contains(search) ||
                        z.Termin.ToString().Contains(search) || w.Wypalono.Contains(search) ||
                        za.Pogieto.Contains(search) || s.Pospawano.Contains(search) ||
                        m.Spakowano.Contains(search) || m.Wyslano.Contains(search) ||
                        p.Uwagi.Contains(search)
                        select p;

            }
            ViewBag.Produkt = sortowanie == "ProduktDSC" ? "ProduktASC" : "ProduktDSC";
            ViewBag.Firma = sortowanie == "FirmaDSC" ? "FirmaASC" : "FirmaDSC";
            ViewBag.Termin = sortowanie == null ? "TerminDSC" : "";
            ViewBag.Wypalono = sortowanie == "WypalonoDSC" ? "WypalonoASC" : "WypalonoDSC";
            ViewBag.Pogieto = sortowanie == "PogietoDSC" ? "PogietoASC" : "PogietoDSC";
            ViewBag.Pospawano = sortowanie == "PospawanoDSC" ? "PospawanoASC" : "PospawanoDSC";
            ViewBag.Spakowano = sortowanie == "SpakowanoDSC" ? "SpakowanoASC" : "SpakowanoDSC";
            ViewBag.Wyslano = sortowanie == "WyslanoDSC" ? "WyslanoASC" : "WyslanoDSC";
            ViewBag.Uwagi = sortowanie == "UwagiDSC" ? "UwagiASC" : "UwagiDSC";

            switch (sortowanie)
            {
                case "ProduktDSC":
                    query = query.OrderByDescending(s => s.Produkt);
                    break;
                case "ProduktASC":
                    query = query.OrderBy(s => s.Produkt);
                    break;
                case "FirmaDSC":
                    query = query.OrderByDescending(s => s.Zlecenia.Firma);
                    break;
                case "FirmaASC":
                    query = query.OrderBy(s => s.Zlecenia.Firma);
                    break;
                case "TerminDSC":
                    query = query.OrderByDescending(s => s.Zlecenia.Termin);
                    break;
                case "WypalonoDSC":
                    query = query.OrderByDescending(s => s.Wypalanie.Wypalono);
                    break;
                case "WypalonoASC":
                    query = query.OrderBy(s => s.Wypalanie.Wypalono);
                    break;
                case "PogietoDSC":
                    query = query.OrderByDescending(s => s.Zaginanie.Pogieto);
                    break;
                case "PogietoASC":
                    query = query.OrderBy(s => s.Zaginanie.Pogieto);
                    break;
                case "PospawanoDSC":
                    query = query.OrderByDescending(s => s.Spawanie.Pospawano);
                    break;
                case "PospawanoASC":
                    query = query.OrderBy(s => s.Spawanie.Pospawano);
                    break;
                case "SpakowanoDSC":
                    query = query.OrderByDescending(s => s.Magazyn.Spakowano);
                    break;
                case "SpakowanoASC":
                    query = query.OrderBy(s => s.Magazyn.Spakowano);
                    break;
                case "WyslanoDSC":
                    query = query.OrderByDescending(s => s.Magazyn.Wyslano);
                    break;
                case "WyslanoASC":
                    query = query.OrderBy(s => s.Magazyn.Wyslano);
                    break;
                case "UwagiDSC":
                    query = query.OrderByDescending(s => s.Uwagi);
                    break;
                case "UwagiASC":
                    query = query.OrderBy(s => s.Uwagi);
                    break;
                default:
                    query = query.OrderBy(s => s.Zlecenia.Termin);
                    break;
            }

            int pageNumber = (page ?? 1);
             
            return View(query.ToPagedList(pageNumber, 6));
        }
    }
}
