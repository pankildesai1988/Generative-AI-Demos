using _2_OpenAIChatFrontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace _2_OpenAIChatFrontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ComparisonController : BaseController
    {
        private readonly IConfiguration _config;

        public ComparisonController(IConfiguration config) : base(config) { }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult History()
        {
            return View();
        }
    }
}
