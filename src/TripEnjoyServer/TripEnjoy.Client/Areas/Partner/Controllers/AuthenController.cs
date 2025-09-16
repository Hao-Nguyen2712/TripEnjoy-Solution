using Microsoft.AspNetCore.Mvc;
using TripEnjoy.Client.ViewModels;

namespace TripEnjoy.Client.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Route("Partner/auth")]
    public class AuthenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Route("sign-up")]
        public IActionResult SignUp()
        {
            return View(new PartnerSignUpRequestVM());
        }
        [Route("sign-in")]
        public IActionResult SignIn()
        {
            return View();
        }
    }
}
