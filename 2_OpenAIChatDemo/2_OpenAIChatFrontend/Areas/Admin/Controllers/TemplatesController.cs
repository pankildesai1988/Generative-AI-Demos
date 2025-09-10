using _2_OpenAIChatFrontend.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _2_OpenAIChatFrontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")] // restricts to Admins only
    public class TemplatesController : BaseController
    {
        // UI entry points only, data comes from API via AJAX

        private readonly IConfiguration _config;

        public TemplatesController(IConfiguration config) : base(config) { }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create() => View();

        public IActionResult Edit(int id)
        {
            ViewBag.TemplateId = id; // JS will use this to load data via API
            return View();
        }
    }
}
