using Microsoft.AspNetCore.Mvc;

namespace ArNir.API.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
