using Microsoft.AspNetCore.Mvc;

namespace Hive_Auth_Server.Controllers
{
    public class UserAuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
