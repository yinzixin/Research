using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HelloWorld.Models;

namespace HelloWorld.Controllers
{ 
    public class MovieController : Controller
    {
        private MovieDBContext db = new MovieDBContext();

        //
        // GET: /Movie/

        public ViewResult Index()
        {
            return View(db.Movies.ToList());
        }

        //
        // GET: /Movie/Details/5

        public ViewResult Details(int id)
        {
            Movie movie = db.Movies.Find(id);
            return View(movie);
        }

        //
        // GET: /Movie/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Movie/Create

        [HttpPost]
        public ActionResult Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                db.Movies.Add(movie);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(movie);
        }
        
        //
        // GET: /Movie/Edit/5
 
        public ActionResult Edit(int id)
        {
            Movie movie = db.Movies.Find(id);
            return View(movie);
        }

        //
        // POST: /Movie/Edit/5

        [HttpPost]
        public ActionResult Edit(Movie movie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        //
        // GET: /Movie/Delete/5
 
        public ActionResult Delete(int id)
        {
            Movie movie = db.Movies.Find(id);
            return View(movie);
        }

        //
        // POST: /Movie/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Movie movie = db.Movies.Find(id);
            db.Movies.Remove(movie);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}