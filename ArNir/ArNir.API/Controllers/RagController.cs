using ArNir.Core.DTOs.RAG;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly IRagService _ragService;
        public RagController(IRagService ragService) => _ragService = ragService;

        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] RagRequestDto dto)
        {
            var result = await _ragService.RunRagAsync(dto.Query, dto.TopK, dto.UseHybrid,
                dto.PromptStyle, dto.SaveAsNew, dto.Provider, dto.Model);
            return Ok(result);
        }
    }
}
