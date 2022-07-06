using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using slnShoppingCa.Models;

namespace slnShoppingCa.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        // GET: Member
        public ActionResult Index()
        {
            //取得所有產品放入products
            var products = db.tProduct.OrderByDescending(m => m.fId).ToList();
            return View("../Home/Index", "_LayoutMember", products);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }

        //Get:Member/shoppingCar
        public ActionResult ShoppingCar()
        {
            //取的登入會員的帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //找出未成為訂單明細的資料，即購物車內容
            var orderDetails = db.tOrderDetail.Where(m => m.fUserId == fUserId && m.fIsApproved == "否").ToList();
            //view使用orderdetails
            return View(orderDetails);
        }

        //Post:Member/shoppingcar
        [HttpPost]
        public ActionResult ShoppingCar(string fReceiver, string fEmail, string fAddress)
        {
            string fUserId = User.Identity.Name;
            string guid = Guid.NewGuid().ToString();

            tOrder order = new tOrder();
            order.fOrderGuid = guid;
            order.fUserId = fUserId;
            order.fReceiver = fReceiver;
            order.fEmail = fEmail;
            order.fAddress = fAddress;
            order.fDate = DateTime.Now;
            db.tOrder.Add(order);

            var carList = db.tOrderDetail.Where(m => m.fIsApproved == "否" && m.fUserId == fUserId).ToList();

            foreach(var item in carList)
            {
                item.fOrderGuid = guid;
                item.fIsApproved = "是";
            }

            db.SaveChanges();
            return RedirectToAction("OrderList");
        }

        public ActionResult AddCar(string fPId)
        {
            //取得會員帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //找出會員放入訂單明細的產品，該產品的fisapproved為"否"
            //表示該產品是購物車狀態
            var currentCar = db.tOrderDetail.Where(m => m.fPId == fPId && m.fIsApproved == "否" && m.fUserId == fUserId).FirstOrDefault();
            //若currentCar等於null，表示會員選購的產品不是購物車狀態
            if(currentCar == null)
            {
                //找出目前選購的產品並指定給product
                var product = db.tProduct.Where(m => m.fPId == fPId).FirstOrDefault();
                //將產品放入訂單明細，因為產品的fIsAPPROVED為"否"，表示為購物車狀態
                tOrderDetail orderDetail = new tOrderDetail();
                orderDetail.fUserId = fUserId;
                orderDetail.fPId = product.fPId;
                orderDetail.fName = product.fName;
                orderDetail.fPrice = product.fPrice;
                orderDetail.fQty = 1;
                orderDetail.fIsApproved = "否";
                db.tOrderDetail.Add(orderDetail);
            }
            else
            {
                //若產品為購物車狀態,，即將該產品數量加1
                currentCar.fQty += 1;
            }
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }

        public ActionResult DeleteCar(int fId)
        {
            //依fid找出要刪除購物車狀態的產品
            var orderDetail = db.tOrderDetail.Where(m => m.fId == fId).FirstOrDefault();
            //刪除購物車狀態商品
            db.tOrderDetail.Remove(orderDetail);
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }

        public ActionResult OrderList()
        {
            string fUserId = User.Identity.Name;
            var orders = db.tOrder.Where(m => m.fUserId == fUserId).OrderByDescending(m => m.fDate).ToList();
            return View(orders);
        }

        public ActionResult OrderDetail( string fOrderGuid)
        {
            var orderDetails = db.tOrderDetail.Where(m => m.fOrderGuid == fOrderGuid).ToList();
            return View(orderDetails);
        }
    }
}