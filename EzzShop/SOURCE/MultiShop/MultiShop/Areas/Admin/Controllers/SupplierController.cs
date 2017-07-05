using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultiShop.Models.EzShopV20;

namespace MultiShop.Areas.Admin.Controllers
{
    public class SupplierController : Controller
    {
        MultiShop.Models.EzShopV20.hocaspnetEntities1 db = new MultiShop.Models.EzShopV20.hocaspnetEntities1();

        public ActionResult Index()
        {
            ViewBag.Suppliers = db.Suppliers;
            return View();
        }

        public ActionResult Insert(Supplier model)
        {
            try
            {
                var f = Request.Files["Image"];
                if (f != null && f.ContentLength > 0)
                {
                    model.Logo = f.FileName;
                    //     + f.FileName.Substring(f.FileName.LastIndexOf("."));
                    f.SaveAs(Server.MapPath("/Content/img/suppliers/" + model.Logo));

                }
                //var from = Server.MapPath("/photos/"+model.Logo);
                ////model.Logo = model.Id + model.Logo.Substring(model.Logo.LastIndexOf("."));
                //var to = Server.MapPath("/Content/img/suppliers/" + model.Logo);
                //System.IO.File.Move(from, to);

                db.Suppliers.Add(model);
                db.SaveChanges();
                ModelState.AddModelError("", "Inserted");
            }
            catch
            {
                ModelState.AddModelError("", "Error");
            }

            ViewBag.Suppliers = db.Suppliers;
            return View("Index", model);
        }

        public ActionResult Update(Supplier model)
        {
            try
            {
                var f = Request.Files["Image"];
                if (f != null && f.ContentLength > 0)
                {
                    model.Logo = f.FileName;
                    //     + f.FileName.Substring(f.FileName.LastIndexOf("."));
                    f.SaveAs(Server.MapPath("/Content/img/suppliers/" + model.Logo));

                }
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                ModelState.AddModelError("", "Updated");
            }
            catch
            {
                ModelState.AddModelError("", "Error");
            }

            ViewBag.Suppliers = db.Suppliers;
            return View("Index", model);
        }

        public ActionResult Delete(String Id)
        {
            try
            {
                var model = db.Suppliers.Find(Id);
                db.Suppliers.Remove(model);
                db.SaveChanges();
                ModelState.AddModelError("", "Deleted");
            }
            catch
            {
                ModelState.AddModelError("", "Error");
            }

            ViewBag.Suppliers = db.Suppliers;
            return View("Index");
        }

        public ActionResult Edit(String Id)
        {
            var model = db.Suppliers.Find(Id);

            ViewBag.Suppliers = db.Suppliers;
            return View("Index", model);
        }

        public ActionResult Upload()
        {
            var f = Request.Files["uplLogo"];
            f.SaveAs(Server.MapPath("/Content/img/photos/" + f.FileName));
            return Content(f.FileName);
        }
    }
}