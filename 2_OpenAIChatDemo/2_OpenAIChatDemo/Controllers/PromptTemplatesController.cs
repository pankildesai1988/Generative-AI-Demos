using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace _2_OpenAIChatDemo.Controllers
{
    using _2_OpenAIChatDemo.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;

    [ApiController]
    [Route("api/[controller]")]
    public class PromptTemplatesController : ControllerBase
    {
        private readonly ITemplateService _templateService;

        public PromptTemplatesController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _templateService.GetTemplatesAsync();
            var result = templates.Select(t => new {
                t.Id,
                t.Name,
                t.KeyName,
                t.TemplateText,
                Parameters = t.Parameters.Select(p => new {
                    p.Id,
                    p.Name,
                    p.KeyName,
                    Options = p.Options?.Split(','),
                    p.DefaultValue
                }).ToList()
            });

            return Ok(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> AddTemplate([FromBody] PromptTemplate template)
        {
            var newTemplate = await _templateService.AddTemplateAsync(template);
            return Ok(new { success = true, data = newTemplate });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] PromptTemplate template)
        {
            template.Id = id;
            var updated = await _templateService.UpdateTemplateAsync(template);
            if (updated == null)
                return NotFound(new { success = false, error = "Template not found" });

            return Ok(new { success = true, data = updated });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            await _templateService.DeleteTemplateAsync(id);
            return Ok(new { success = true });
        }
    }
}
