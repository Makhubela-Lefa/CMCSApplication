using Microsoft.AspNetCore.Mvc;

namespace CMCSApplication.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Approval()
        {
            return View();
        }
        public IActionResult Reports()
        {
            return View();
        }
        public IActionResult Assign()
        {
            return View();
        }
    }
}
