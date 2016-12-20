using Blog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //GET:Article/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                //GET articles from database
                var articles = database.Articles.Include(a => a.Author).ToList();

                return View(articles);
            }
        }

        //GET:Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get the article from database
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).First();
                if (article == null)
                {
                    return HttpNotFound();
                }
               
                return View(article);
                }

           
        }
      

        //
        //GET:Article/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        //
        //POST:Article/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(Article article)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    //Get author id
                    var authorId = db.Users.
                        Where(u => u.UserName == this.User.Identity.Name).First().Id;

                    //Set articles author
                    article.AuthorID = authorId;
                    article.DateAdded = DateTime.Now;
                    //Save the article in DB
                    db.Articles.Add(article);
                    //var dateCreated = DateTime.Now;
                   
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            return View(article);
        }

        //
        //GET:Article/Delete
        public ActionResult Delete(int? id)
        {
            if (id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get article from database
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                //validate the user
                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                //Check if article axists
                if (article==null)
                {
                    return HttpNotFound();
                }

                //Pass article to view
                return View(article);
            }
        }
        
        //
        //POST:Article/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int?id)
        {
            if (id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get article from database
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                //Check if article exist
                if (article==null)
                {
                    return HttpNotFound();
                }

                //Remove the article from database
                database.Articles.Remove(article);
                database.SaveChanges();

                //Return to Index page
                return RedirectToAction("Index");
            }
        }

        //
        //GET:Article/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get article from database
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                //validate the user
                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                //Check if article axists
                if (article == null)
                {
                    return HttpNotFound();
                }

                //Create the view model
                var model = new ArticleViewModel();
                model.Id = article.Id;
                model.Title = article.Title;
                model.Content = article.Content;

                //Pass article to view
                return View(model);
            }
        }

        //
        //POST:Article/Edit
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            //check if model state is valid
            if (ModelState.IsValid)
            {
                using (var database=new BlogDbContext())
                {
                    //Get article from database
                    var article = database.Articles.FirstOrDefault(a => a.Id == model.Id);

                    //Set article's new values
                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.DateAdded = DateTime.Now;

                    //Set the article state to modified
                    //Save the article in database
                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();

                    //Redirect to index page to see the changes
                    return RedirectToAction("Index");
                }
            }

            //If model is invalid return the same view
            return View(model);
        }


        //method for user verification to allow article edit
        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }

        //End of file Article Controller
    }
    }
