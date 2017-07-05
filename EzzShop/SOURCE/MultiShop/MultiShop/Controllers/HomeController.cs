using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Configuration;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime;
using MultiShop.Models.EzShopV20;
using Newtonsoft.Json.Linq;

namespace MultiShop.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        MultiShop.Models.EzShopV20.hocaspnetEntities1 db = new MultiShop.Models.EzShopV20.hocaspnetEntities1();
        public ActionResult Index()
        {   
            try
            {
                var model = db.Categories
                .Where(c => c.Products.Count >= 4)
                .OrderBy(c => Guid.NewGuid()).ToList();
                return View(model);

            }
            catch (Exception exception)
            {
                    
                throw;
            }
        }

        public ActionResult Search()
        {
            var name = Request["term"];

            var data = db.Products
                .Where(p => p.Name.Contains(name))
                .Select(p => p.Name).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
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
        
        public ActionResult Category()
        {
            var model = db.Categories;
            return PartialView("_Category",model);
        }

        public ActionResult Special()
        {
            string bestSelling = string.Empty;
            if (IsTranditional())
            {
                bestSelling = db.Customers.Where(c => c.Id == User.Identity.Name).Select(c=>c.TCBestSellingForTC).FirstOrDefault();
            }
            else
            {
                bestSelling = db.Customers.Where(c => c.Id == User.Identity.Name).Select(c=>c.NCNewItemForNewCLient).FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(bestSelling) || !string.IsNullOrWhiteSpace(bestSelling))
            {
                Dictionary<int,float> resultDictionary = new Dictionary<int, float>();
                JArray resultArray = JArray.Parse(bestSelling);
                foreach (var job in resultArray)
                {
                    resultDictionary.Add((int)job["Key"],(float)job["Value"]);
                }
                var result = resultDictionary.OrderByDescending(r => r.Value);
                var model = new List<Product>();
                int i = 0;
                foreach (var priority in result)
                {
                    if (i ==5)
                    {
                        break;
                    }
                    var product = db.Products.First(p => p.Id == priority.Key);
                    model.Add(product);
                    i++;
                }
                return PartialView("_Special", model);
            }
            else
            {
                var model = db.Products.Where(p => p.Special == true).Take(5);
                return PartialView("_Special", model);
            }
        }

        public ActionResult Saleoff()
        {
            //string newItems = string.Empty;
            //if (IsTranditional())
            //{
            //    newItems = db.Customers.Where(c => c.Id == User.Identity.Name).Select(c => c.TCNewItemForTC).FirstOrDefault();
            //}
            //else
            //{
            //    newItems = db.Customers.Where(c => c.Id == User.Identity.Name).Select(c => c.NCNewItemForNewCLient).FirstOrDefault();
            //}
            //if (!string.IsNullOrEmpty(newItems) || !string.IsNullOrWhiteSpace(newItems))
            //{
            //    Dictionary<int, float> resultDictionary = new Dictionary<int, float>();
            //    JArray resultArray = JArray.Parse(newItems);
            //    foreach (var job in resultArray)
            //    {
            //        resultDictionary.Add((int)job["Key"], (float)job["Value"]);
            //    }
            //    var result = resultDictionary.OrderByDescending(r => r.Value);
            //    var model = new List<Product>();
            //    int i = 0;
            //    foreach (var priority in result)
            //    {
            //        if (i == 5)
            //        {
            //            break;
            //        }
            //        var product = db.Products.First(p => p.Id == priority.Key);
            //        model.Add(product);
            //        i++;
            //    }
            //    return PartialView("_Saleoff", model);
            //}
            //else
            //{
            //    var model = db.Products.Where(p => p.ProductDate >= DateTime.Now.AddYears(-1)).Take(5);
            //    return PartialView("_Saleoff", model);
            //}
            DateTime mytTime = DateTime.Now;
            var newTime = mytTime.AddYears(-1);
            var model = db.Products.Where(p => p.ProductDate >= newTime).Take(5);
            return PartialView("_Saleoff", model);
        }
        //_NotPurchased
        public ActionResult NotPurchased()
        {
            string notpurchase = string.Empty;
            if (IsTranditional())
            {
                notpurchase = db.Customers.Where(c => c.Id == User.Identity.Name).Select(c => c.TCNotPurchasedForTC).FirstOrDefault();
            }
            else
            {
                return Content("");
            }
            if (!string.IsNullOrEmpty(notpurchase) || !string.IsNullOrWhiteSpace(notpurchase))
            {
                Dictionary<int, float> resultDictionary = new Dictionary<int, float>();
                JArray resultArray = JArray.Parse(notpurchase);
                foreach (var job in resultArray)
                {
                    resultDictionary.Add((int)job["Key"], (float)job["Value"]);
                }
                var result = resultDictionary.OrderByDescending(r => r.Value);
                var model = new List<Product>();
                int i = 0;
                foreach (var priority in result)
                {
                    if (i == 4)
                    {
                        break;
                    }
                    var product = db.Products.First(p => p.Id == priority.Key);
                    model.Add(product);
                    i++;
                }
                return PartialView("_NotPurchased", model);
            }
            else
            {
                var model = db.Products.Where(p => p.Special == true).Take(5);
                return PartialView("_NotPurchased", model);
            }
        }

        public ActionResult Purchased()
        {
            string purchase = string.Empty;
            if (IsTranditional())
            {
                purchase = db.Customers.Where(c => c.Id == User.Identity.Name).Select(c => c.TCPurchasedForTC).FirstOrDefault();
            }
            else
            {
                return Content("");
            }
            if (!string.IsNullOrEmpty(purchase) || !string.IsNullOrWhiteSpace(purchase))
            {
                Dictionary<int, float> resultDictionary = new Dictionary<int, float>();
                JObject resultArray = JObject.Parse(purchase);
                foreach (var job in resultArray)
                {
                    resultDictionary.Add(int.Parse(job.Key), (float)job.Value);
                }
                var result = resultDictionary.OrderByDescending(r => r.Value);
                var model = new List<Product>();
                int i = 0;
                foreach (var priority in result)
                {
                    if (i == 4)
                    {
                        break;
                    }
                    var product = db.Products.First(p => p.Id == priority.Key);
                    model.Add(product);
                    i++;
                }
                return PartialView("_Purchased", model);
            }
            else
            {
                return Content("");
            }
        }

        private bool IsTranditional()
        {
            var Tranditionals = db.Orders.Where(c => c.CustomerId == User.Identity.Name);
            if (Tranditionals.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}