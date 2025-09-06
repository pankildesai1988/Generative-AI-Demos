using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace _2_OpenAIChatDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromptTemplatesController : ControllerBase
    {
        private readonly ChatDbContext _db;

        public PromptTemplatesController(ChatDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetTemplates()
        {
            var templates = _db.PromptTemplates
                .OrderBy(t => t.Name)
                .Select(t => new { t.Id, t.Name, t.KeyName, t.TemplateText })
                .ToList();

            return Ok(new { success = true, data = templates });
        }

        [HttpPost]
        public IActionResult AddTemplate([FromBody] PromptTemplate template)
        {
            if (string.IsNullOrEmpty(template.Name) || string.IsNullOrEmpty(template.KeyName))
                return BadRequest(new { success = false, error = "Name and KeyName are required" });

            _db.PromptTemplates.Add(template);
            _db.SaveChanges();

            return Ok(new { success = true, data = template });
        }
    }

}
