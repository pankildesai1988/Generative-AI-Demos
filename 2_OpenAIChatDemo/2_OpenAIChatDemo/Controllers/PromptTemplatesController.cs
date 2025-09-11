using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;
using _2_OpenAIChatDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace _2_OpenAIChatDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // 👈 Require Admin role for all endpoints
    public class PromptTemplateController : ControllerBase
    {
        private readonly IPromptTemplateService _service;

        public PromptTemplateController(IPromptTemplateService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromptTemplateCreateDto dto)
        {
            foreach (var param in dto.Parameters)
            {
                if (param.IsRequired && string.IsNullOrWhiteSpace(param.DefaultValue))
                    return BadRequest($"Parameter {param.Name} is required.");

                if (!string.IsNullOrEmpty(param.RegexPattern))
                {
                    try
                    {
                        var regex = new System.Text.RegularExpressions.Regex(param.RegexPattern);
                        if (!regex.IsMatch(param.DefaultValue ?? ""))
                            return BadRequest($"Parameter {param.Name} does not match regex {param.RegexPattern}.");
                    }
                    catch
                    {
                        return BadRequest($"Invalid regex for parameter {param.Name}.");
                    }
                }
            }

            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PromptTemplateUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.SoftDeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // 🔥 NEW: Get Version History
        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetVersions(int id)
        {
            var result = await _service.GetVersionsAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // 🔥 NEW: Rollback to Previous Version
        [HttpPost("{id}/rollback/{version}")]
        public async Task<IActionResult> Rollback(int id, int version)
        {
            var result = await _service.RollbackAsync(id, version);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("{id}/preview")]
        public async Task<IActionResult> Preview(int id, [FromBody] PromptPreviewDto dto)
        {
            if(id == 0)
    {
                // Direct ad-hoc preview, without DB
                string rendered = dto.TemplateText;

                foreach (var kvp in dto.Parameters)
                {
                    string key = $"{{{kvp.Key}}}";
                    rendered = rendered.Replace(key, kvp.Value ?? $"[{kvp.Key}]");
                }

                return Ok(new PromptPreviewResultDto { RenderedPrompt = rendered });
            }

            var result = await _service.PreviewAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

    }
}
    