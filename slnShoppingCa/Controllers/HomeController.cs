using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using slnShoppingCa.Models;
using System.Web.Security;  //web應用程式表單驗證服務

namespace slnShoppingCa.Controllers
{
    public class HomeController : Controller
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        public ActionResult Index()
        {
            //取得所有產品放入products
            var products = db.tProduct.OrderByDescending(m => m.fId).ToList();
            return View(products);
        }

        //Get: Home/Login
        public ActionResult Login()
        {
            return View();
        }

        //Post: Home/Login
        [HttpPost]
        public ActionResult Login(string fUserId, string fPwd)
        {
            //依帳密取得會員並指定給member
            var member = db.tMember.Where(m=>m.fUserId == fUserId && m.fPwd == fPwd).FirstOrDefault();
            //若member為null，表示會員未註冊
            if(member == null)
            {
                ViewBag.Message = "帳密錯誤，登入失敗";
                return View();
            }
            //使用session 變數記錄歡迎詞
            Session["Welcome"] = member.fName + "歡迎光臨";
            //指定使用者帳號通過驗證
            FormsAuthentication.RedirectFromLoginPage(fUserId, true);
            return RedirectToAction("Index", "Member");
        }

        //Get:Home/Register
        public ActionResult Register()
        {
            return View();
        }

        //Post:Hpme/Register
        [HttpPost]
        public ActionResult Register(tMember pMember)
        {
            //若模型沒通過驗證則顯示目前的view
            if(ModelState.IsValid == false)
            {
                return View();
            }
            //依帳號取得會員並指定給member
            var member = db.tMember.Where(m => m.fUserId == pMember.fUserId).FirstOrDefault();
            //若member為null，表示會員未註冊
            if(member == null)
            {
                //將會員記錄新增到tMember資料表
                db.tMember.Add(pMember);
                db.SaveChanges();
                //執行home控制器的login動作方法
                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號已有人使用，註冊失敗";
            return View();
        }
    }
}