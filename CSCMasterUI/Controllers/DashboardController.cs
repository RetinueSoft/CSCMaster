using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;

namespace ReXLPosUI.Controllers
{
    //[Authorize(AuthenticationSchemes = "JwtCookieScheme")]
    public class DashboardController : Controller
    {
        public IActionResult Index(string userRole)
        {
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("EntrolmentUpload");
            }

            switch (userRole.ToUpper())
            {
                case "1":
                case "User":
                case "ADMIN":
                case "BUSINESSOWNER":
                    return RedirectToAction("Admin");
                default:
                    return RedirectToAction("EntrolmentUpload");
            }
        }
        public IActionResult SuperAdmin()
        {
            return View();
        }
        public IActionResult Admin()
        {
            return View();
        }
        public IActionResult EntrolmentUpload()
        {
            return View();
        }
    }
}
