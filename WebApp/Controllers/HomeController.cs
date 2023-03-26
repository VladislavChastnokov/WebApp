using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly DContext _context;

        public HomeController(DContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string login, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Login.Equals(login) && u.Password == Models.Helper.CreateMD5(password));
                if (user != null)
                {
                    HttpContext.Session.SetInt32("userId", user.Id);
                    HttpContext.Session.SetInt32("userRole", user.RoleId);
                    HttpContext.Session.SetString("userName", string.Format("{0} {1} {2}", user.LastName, user.FirstName, user.MiddleName));
                    switch (user.RoleId)
                    {
                        case 1:
                            return RedirectToAction("", "Administration");
                        case 2:
                            return RedirectToAction("", "Examination");
                    }
                }
                else
                {
                    ViewBag.Error = true;
                    ViewBag.ErrorMessage = "Неверный логин или пароль";
                }
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                ViewBag.Error = true;
            }
            
            return View();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
