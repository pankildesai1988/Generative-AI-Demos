using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace _2_OpenAIChatFrontend.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IConfiguration _config;

        protected BaseController(IConfiguration config)
        {
            _config = config;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            ViewBag.ApiBaseUrl = _config["ApiSettings:BaseUrl"] ?? "http://localhost:5000"; // ✅ fallback
            base.OnActionExecuting(context);
        }
    }
}
