using Microsoft.AspNetCore.Mvc;

namespace HealthGuage.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult NotFoundPage()
        {
            return View();
        }
    }
}
