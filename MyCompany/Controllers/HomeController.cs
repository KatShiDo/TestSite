using Microsoft.AspNetCore.Mvc;

namespace MyCompany.Controllers
{
    public class Home : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}