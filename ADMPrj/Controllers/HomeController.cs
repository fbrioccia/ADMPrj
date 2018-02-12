using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using ADMPrj.Data;
using ADMPrj.Models;

namespace ADMPrj.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [WebMethod]
        public JsonResult TweetsLoad()
        {

            var dataMng = new DataManager();
            var twitterAccountList = dataMng.Neo4JInstance.TwitterAccountList();

            return Json(twitterAccountList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Tweets()
        {
           ViewBag.Message = "Your contact page.";
            return View();
        }


        public ActionResult TweetsSave(SeekResults[] SeekResults)
        {
            var dataMng = new DataManager();
            dataMng.Neo4JInstance.SaveAccountTweets(SeekResults);
            return Json(new { newUrl = Url.Action("Tweets", "Home") });
        }

        public ActionResult Seek(string searchTerms)
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        [WebMethod]
        public JsonResult SeekSearch(string searchTerms)
        {

            var twitterHandler = new TwitterLogic.TwitterHandler();
            var tweets = twitterHandler.TSearch(searchTerms);

            return Json(tweets, JsonRequestBehavior.AllowGet);
        }

        [WebMethod]
        public JsonResult ComputeScore(string tweetId)
        {
            var dataMng = new DataManager();
            var totalScore = dataMng.Neo4JInstance.ComputeScore(tweetId);

            return Json(totalScore, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Patterns()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
    }
}