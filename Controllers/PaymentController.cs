using Microsoft.AspNetCore.Mvc;

namespace IOMSAPI.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
