using Microsoft.AspNetCore.Mvc;

namespace _2_OpenAIChatFrontend.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(IConfiguration config) : base(config) { }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
    }
}
