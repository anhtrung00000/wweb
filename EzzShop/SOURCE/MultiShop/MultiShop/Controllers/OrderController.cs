using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultiShop.Models.EzShopV20;

namespace MultiShop.Controllers
{
    public class OrderController : Controller
    {
        MultiShop.Models.EzShopV20.hocaspnetEntities1 db = new MultiShop.Models.EzShopV20.hocaspnetEntities1();
        //
        // GET: /Order/
        [Authorize]
        public ActionResult Checkout()
        {
           
            var model = new Order();
            model.CustomerId = User.Identity.Name;
            model.OrderDate = DateTime.Now.Date;
            model.RequireDate = DateTime.Now.Date;
            model.Amount = ShoppingCart.Cart.Total;
            return View(model);
        }
        
        public ActionResult Purchase(Order model)
        {
            var order = new Order();
            order.OrderDate = model.OrderDate;
            order.OrderDetails = model.OrderDetails;
            order.Receiver = model.Receiver;
            order.RequireDate = model.RequireDate;
            order.Address = model.Address;
            order.Amount = model.Amount;
            order.Customer = model.Customer;
            order.CustomerId = model.CustomerId;
            order.Description = model.Description;
            db.Orders.Add(model);
            //db.SaveChanges();

            var cart = ShoppingCart.Cart;
            foreach (var p in cart.Items)
            {
                var d = new OrderDetail
                {
                    Order = model,
                    ProductId = p.Id,
                    UnitPrice = p.UnitPrice,
                    Discount = p.Discount,
                    Quantity = p.Quantity
                };
                db.OrderDetails.Add(d);
            }
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        Response.Write("Property: " + validationError.PropertyName + " Error: " +
                                       validationError.ErrorMessage);
                    }
                }
            }
                return RedirectToAction("Detail", new {id = model.Id});
            
        }

        public ActionResult Detail(int id)
        {
            var order = db.Orders.Find(id);
            return View(order);
        }

        public ActionResult List()
        {
            var orders = db.Orders
                .Where(o => o.CustomerId == User.Identity.Name);
            return View(orders);
        }
	}
}