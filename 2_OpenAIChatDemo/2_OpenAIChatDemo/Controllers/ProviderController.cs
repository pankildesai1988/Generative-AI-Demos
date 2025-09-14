using _2_OpenAIChatDemo.LLMProviders;
using Microsoft.AspNetCore.Mvc;

namespace _2_OpenAIChatDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProviderController : ControllerBase
    {
        private readonly IEnumerable<ILlmProvider> _providers;

        public ProviderController(IEnumerable<ILlmProvider> providers)
        {
            _providers = providers;
        }

        [HttpGet("models")]
        public IActionResult GetModels()
        {
            var result = _providers.Select(p => new
            {
                provider = p.ProviderName,
                models = p.GetAvailableModels()
            });

            return Ok(new { success = true, data = result });
        }
    }

}
