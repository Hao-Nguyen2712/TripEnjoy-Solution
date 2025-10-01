using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TripEnjoy.Client.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Get partner information from claims
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var accountId = User.FindFirst("AccountId")?.Value;
            var partnerId = User.FindFirst("PartnerId")?.Value;

            ViewBag.Email = email;
            ViewBag.AccountId = accountId;
            ViewBag.PartnerId = partnerId;

            return View();
        }
    }
}